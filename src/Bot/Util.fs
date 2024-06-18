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
