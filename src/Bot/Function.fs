namespace Bot


open System

open Amazon.Lambda.Core


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer>)>]
()

type Function() =
    /// <summary>
    /// A function that act as a webhook for a bot
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns>StatusCode</returns>
    member __.FunctionHandler (event: Types.Lambda.ApiGatewayEvent) (context: ILambdaContext) =
      let getConfiguration = Api.AWS.getObject (new Amazon.S3.AmazonS3Client())
      let configurationStream = (getConfiguration (Config.get Config.Env.CONFIGURATION_BUCKET) (Config.get Config.Env.CONFIGURATION_FILE)).Result.ResponseStream
      let decodedContent = (new System.IO.StreamReader(configurationStream)).ReadToEnd()
      decodedContent.Split('\n')
      |> Util.parseEnv
      |> Util.injectConfiguration

      let botEndpoint = $"/{Constants.botToken}"
      let notifyEndpoint = System.Uri(Config.get Config.Env.NOTIFY_ENDPOINT)
      let headers = ("HEADERS\n", event.headers) ||> Map.fold (fun s k v -> s + $"{k}: {v}\n")
      context.Logger.Log $"Received request on route {event.requestContext.routeKey} with {headers}"
      context.Logger.Log $"Body request {event.body}"

      match event.rawPath with
      | endpoint when endpoint = botEndpoint ->
        let payload = System.Text.Json.JsonSerializer.Deserialize<Types.Request.TelegramWebhookPayload>(
          json=event.body,
          options=Constants.jsonDeserializeOptions
        )
        context.Logger.Log $"Serialized payload {payload}"
        let parsedPayload = Api.Telegram.processWebhook(payload)
        match Commands.parseCommand parsedPayload with
        | Types.Command.ListFile ->
          { Api.GoogleDrive.credentials with scopes = [Google.Apis.Drive.v3.DriveService.Scope.DriveReadonly] }
          |> Api.GoogleDrive.initialize
          |> Api.GoogleDrive.listFile
          |> Commands.handleCommand parsedPayload
          {| 
            statusCode = System.Net.HttpStatusCode.OK
          |}
        | Types.Command.UpdateMap ->
          let updateParams: Api.Github.UpdateMapParams =
            {
              Owner = Config.get Config.Env.GITHUB_USERNAME
              Repository = Config.get Config.Env.GITHUB_REPOSITORY
              RepositoryEnvironment = Config.get Config.Env.GITHUB_REPOSITORY_ENVIRONMENT
              WorkflowId = Config.get Config.Env.GITHUB_ACTION_WORKFLOW_ID
            }
          updateParams
          |> Api.Github.updateMap
          |> Commands.handleCommand parsedPayload
          {| 
            statusCode = System.Net.HttpStatusCode.OK
          |}
        | Types.Command.NoCommand ->
          {| 
            statusCode = System.Net.HttpStatusCode.NoContent // Set this way so webhook will not retry the request
          |}
      | endpoint when endpoint = notifyEndpoint.AbsolutePath ->
        let parsedPayload: Types.UserMessagePayload = {
          UserID = Int32.Parse((Config.get Config.Env.ADMIN_ID))
          Command = Some Types.ListFile
          Text = Some ""
        }
        { Api.GoogleDrive.credentials with scopes = [Google.Apis.Drive.v3.DriveService.Scope.DriveReadonly] }
        |> Api.GoogleDrive.initialize
        |> Api.GoogleDrive.listFile
        |> Commands.handleCommand parsedPayload
        {| 
          statusCode = System.Net.HttpStatusCode.OK
        |}
      | _ ->
        {| 
          statusCode = System.Net.HttpStatusCode.NotImplemented
        |}
