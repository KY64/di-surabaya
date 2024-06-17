type Request = {
  Headers: List<string * string>
  Content: System.Net.Http.HttpContent option
}

type Command = {
  command: string
  description: string
}

type Method =
  static member POST = System.Net.Http.HttpMethod.Post

let commands = [
  { command = "/list"; description = "List file in a drive folder" }
]

let baseUrl = System.Uri("https://api.telegram.org")
let botToken = System.Environment.GetEnvironmentVariable("BOT_TOKEN")
let webHookUrl = System.Environment.GetEnvironmentVariable("WEBHOOK_URL")
let apiUrl = System.Uri(baseUrl, $"{baseUrl.AbsoluteUri}bot{botToken}")
let handler = new System.Net.Http.HttpClientHandler()
let client = new System.Net.Http.HttpClient(handler, true)

let fetch (client: System.Net.Http.HttpClient) (url: System.Uri) (method: System.Net.Http.HttpMethod) (requestOption: Request) =
  let request = new System.Net.Http.HttpRequestMessage(method, url)
  requestOption.Headers |> List.iter request.Headers.Add
  requestOption.Content |> Option.iter (fun content -> request.Content <- content)
  client.SendAsync(request)

let telegramApi (api: string) (method: System.Net.Http.HttpMethod) (requestOption: Request) =
  let endpoint = System.Uri(apiUrl, $"{apiUrl}/{api}")
  requestOption
  |> fetch client endpoint method

type Api =
  static member setWebhook = telegramApi "setWebhook" Method.POST
  static member setCommands = telegramApi "setMyCommands" Method.POST

let setWebhook =
  {
    Headers = []
    Content = Some (System.Net.Http.Json.JsonContent.Create({| url = webHookUrl |}))
  }
  |> Api.setWebhook

let setCommands =
  let payload = {|
    commands = commands
    scope = {| ``type`` = "all_private_chats" |}
  |}
  {
    Headers = []
    Content = Some (System.Net.Http.Json.JsonContent.Create(payload))
  }
  |> Api.setCommands

let responses =
  let formatResponse (task: System.Net.Http.HttpResponseMessage) =
    {|
      url = task.RequestMessage.RequestUri.LocalPath
      result = task.Content.ReadAsStringAsync().Result
    |}
  System.Threading.Tasks.Task.WhenAll([setWebhook; setCommands]).Result
  |> Array.map formatResponse
  |> Array.iter (printfn "%A")

responses
