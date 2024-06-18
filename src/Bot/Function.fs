namespace Bot


open Amazon.Lambda.Core

open System


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer>)>]
()

type Function() =
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    member __.FunctionHandler (event: Types.Lambda.ApiGatewayEvent) (context: ILambdaContext) =
      let botEndpoint = $"/{Config.get Config.Env.BOT_TOKEN}"
      let notifyEndpoint = Config.get Config.Env.NOTIFY_ENDPOINT
      let headers = ("HEADERS\n", event.headers) ||> Map.fold (fun s k v -> s + $"{k}: {v}\n")
      context.Logger.Log $"Received request on route {event.requestContext.routeKey} with {headers}"

      match event.rawPath with
      | endpoint when endpoint = botEndpoint ->
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
