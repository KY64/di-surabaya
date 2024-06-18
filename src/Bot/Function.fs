namespace Bot


open Amazon.Lambda.Core

open System


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer>)>]
()

type RequestContext = {
  domainName: string
  routeKey: string
}

type LambdaEvent = {
  body: string
  rawPath: string
  headers: System.Collections.Generic.IDictionary<string, string>
  requestContext: RequestContext
  http: Object list
}

type Function() =
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    member __.FunctionHandler (event: LambdaEvent) (_: ILambdaContext) =
        "Ok"
