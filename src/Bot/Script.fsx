#r "nuget: Google.Apis.Drive.v3, 1.68.0.3428"
#r "nuget: Google.Apis.Auth, 1.68.0"

open Google.Apis.Auth.OAuth2
open Google.Apis.Drive.v3
open Google.Apis.Services

type Request = {
  Headers: List<string * string>
  Content: System.Net.Http.HttpContent option
}

type Command = {
  command: string
  description: string
}

type Credential = {
  auth_uri: string
  client_email: string
  client_id: string
  private_key: string
  private_key_id: string
  project_id: string
  token_uri: string
  universe_domain: string
  scopes: string list
}

type Method =
  static member POST = System.Net.Http.HttpMethod.Post

let commands = [
  { command = "/list"; description = "List file in a drive folder" }
]

let getEnv = System.Environment.GetEnvironmentVariable
let baseUrl = System.Uri("https://api.telegram.org")
let botToken = getEnv("BOT_TOKEN")
let webHookUrl = System.Uri(getEnv("WEBHOOK_URL"))
let maxGoogleDriveNotificationExpiration = System.DateTimeOffset.Now.ToUnixTimeMilliseconds() + 86400000L
let googleDriveFolderId = getEnv("DRIVE_FOLDER_ID")
let apiUrl = System.Uri(baseUrl, $"{baseUrl.AbsoluteUri}bot{botToken}")
let handler = new System.Net.Http.HttpClientHandler()
let client = new System.Net.Http.HttpClient(handler, true)
let googleDriveNotificationChannel =
  new Google.Apis.Drive.v3.Data.Channel(
    Id=getEnv("GOOGLE_DRIVE_NOTIFICATION_ID"),
    Address=getEnv("NOTIFY_ENDPOINT"),
    Expiration=maxGoogleDriveNotificationExpiration,
    ResourceId=googleDriveFolderId,
    Type="webhook"
  )

let initializeGoogleService (credential: Credential) = 

  let initializeServiceAccount = ServiceAccountCredential.Initializer(
    credential.client_email,
    credential.token_uri,
    ProjectId=credential.project_id,
    UniverseDomain=credential.universe_domain,
    KeyId=credential.private_key_id,
    UseJwtAccessWithScopes=false
  )

  let serviceAccount = ServiceAccountCredential(
   initializeServiceAccount.FromPrivateKey(credential.private_key),
   Scopes=credential.scopes
  )

  let accessToken = serviceAccount.GetAccessTokenForRequestAsync(credential.auth_uri)
  accessToken.Wait()
  let initializer = BaseClientService.Initializer(HttpClientInitializer=serviceAccount)
  new DriveService(initializer)

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
    Content = Some (System.Net.Http.Json.JsonContent.Create({| 
      url = System.Uri(webHookUrl, botToken).AbsoluteUri 
    |}))
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

let googleDriveService = 
  {
    auth_uri = getEnv("GOOGLE_DRIVE_OAUTH_AUTH_URI")
    client_email = getEnv("GOOGLE_DRIVE_OAUTH_CLIENT_EMAIL")
    client_id = getEnv("GOOGLE_DRIVE_OAUTH_CLIENT_ID")
    private_key = getEnv("GOOGLE_DRIVE_OAUTH_PRIVATE_KEY")
    private_key_id = getEnv("GOOGLE_DRIVE_OAUTH_PRIVATE_KEY_ID")
    project_id = getEnv("GOOGLE_DRIVE_OAUTH_PROJECT_ID")
    token_uri = getEnv("GOOGLE_DRIVE_OAUTH_TOKEN_URI")
    universe_domain = getEnv("GOOGLE_DRIVE_OAUTH_UNIVERSE_DOMAIN")
    scopes = [Google.Apis.Drive.v3.DriveService.Scope.DriveReadonly]
  }
  |> initializeGoogleService

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

let googleDriveResponse = googleDriveService.Files.Watch(fileId=googleDriveFolderId, body=googleDriveNotificationChannel).Execute()
printfn "Google Drive Response: Notification created with ID %A" googleDriveResponse.Id
