module Util

let private handler = new System.Net.Http.HttpClientHandler()
let client = new System.Net.Http.HttpClient(handler, true)

let fetch (client: System.Net.Http.HttpClient) (url: System.Uri) (method: System.Net.Http.HttpMethod) (requestOption: Types.Request) =
  let request = new System.Net.Http.HttpRequestMessage(method, url)
  requestOption.Headers |> List.iter request.Headers.Add
  requestOption.Content |> Option.iter (fun content -> request.Content <- content)
  task {
    let! response = client.SendAsync(request)
    return 
      match response.IsSuccessStatusCode with
       | true -> Ok response
       | false -> Error response
  }

let appendUri (uri: System.Uri) (relativePath: string) =
  let path = System.Uri(uri, relativePath).AbsolutePath
  System.Uri(uri, uri.AbsolutePath+path)

let getResultValue (result: Result<'a, 'a>) =
  match result with
  | Ok result -> result
  | Error result -> result

let parseEnv (input: string seq) =
  let mutable envs: Map<string, string> = Map.empty
  let mutable lastKey: string = ""
  let updateMap (content: string) =
    match content.Split('=') with
    | text when text.Length > 1 ->
      lastKey <- text[0]
      let restValue =
        text
        |> Array.skip 1
        |> String.concat "="
      envs <- Map.add lastKey restValue envs
    | text ->
      let value = envs[lastKey] + "\n" + text[0]
      envs <- Map.add lastKey value envs

  input
  |> Seq.iter updateMap

  envs

let injectConfiguration (envs: Map<string, string>) =
  envs
  |> Map.iter (fun key value -> System.Environment.SetEnvironmentVariable(key, value))
