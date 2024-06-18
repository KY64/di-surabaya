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
      let botEndpoint = $"/{Constants.botToken}"
      let notifyEndpoint = Config.get Config.Env.NOTIFY_ENDPOINT
      let headers = ("HEADERS\n", event.headers) ||> Map.fold (fun s k v -> s + $"{k}: {v}\n")
      context.Logger.Log $"Received request on route {event.requestContext.routeKey} with {headers}"
      context.Logger.Log $"Body request {event.body}"

      let credentials: Api.GoogleDrive.Credential = {
        auth_uri = Config.get Config.Env.GOOGLE_DRIVE_OAUTH_AUTH_URI
        client_email = Config.get Config.Env.GOOGLE_DRIVE_OAUTH_CLIENT_EMAIL
        client_id = Config.get Config.Env.GOOGLE_DRIVE_OAUTH_CLIENT_ID
        private_key = Config.get Config.Env.GOOGLE_DRIVE_OAUTH_PRIVATE_KEY
        private_key_id = Config.get Config.Env.GOOGLE_DRIVE_OAUTH_PRIVATE_KEY_ID
        project_id = Config.get Config.Env.GOOGLE_DRIVE_OAUTH_PROJECT_ID
        token_uri = Config.get Config.Env.GOOGLE_DRIVE_OAUTH_TOKEN_URI
        universe_domain = Config.get Config.Env.GOOGLE_DRIVE_OAUTH_UNIVERSE_DOMAIN
        scopes = [Google.Apis.Drive.v3.DriveService.Scope.DriveReadonly]
      }

      match event.rawPath with
      | endpoint when endpoint = botEndpoint ->
        let payload = System.Text.Json.JsonSerializer.Deserialize<Types.Request.TelegramWebhookPayload>(
          json=event.body,
          options=Constants.jsonDeserializeOptions
        )
        context.Logger.Log $"Serialized payload {payload}"
        let parsedPayload = Api.Telegram.processWebhook(payload)
        credentials
        |> Api.GoogleDrive.initialize
        |> Api.GoogleDrive.listFile
        |> Commands.handleCommand parsedPayload
        {| 
          statusCode = System.Net.HttpStatusCode.OK
        |}
      | endpoint when endpoint = notifyEndpoint ->
        {| 
          statusCode = System.Net.HttpStatusCode.OK
        |}
      | _ ->
        {| 
          statusCode = System.Net.HttpStatusCode.NotImplemented
        |}
