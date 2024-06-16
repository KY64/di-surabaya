open Funogram.Types

let service = Api
               .GoogleDrive
               .initialize { auth_uri = Config.get Config.Env.GOOGLE_DRIVE_OAUTH_AUTH_URI
                             client_email= Config.get Config.Env.GOOGLE_DRIVE_OAUTH_CLIENT_EMAIL
                             client_id= Config.get Config.Env.GOOGLE_DRIVE_OAUTH_CLIENT_ID
                             private_key= Config.get Config.Env.GOOGLE_DRIVE_OAUTH_PRIVATE_KEY
                             private_key_id= Config.get Config.Env.GOOGLE_DRIVE_OAUTH_PRIVATE_KEY_ID
                             project_id= Config.get Config.Env.GOOGLE_DRIVE_OAUTH_PROJECT_ID
                             token_uri= Config.get Config.Env.GOOGLE_DRIVE_OAUTH_TOKEN_URI
                             universe_domain= Config.get Config.Env.GOOGLE_DRIVE_OAUTH_UNIVERSE_DOMAIN
                             scopes= [Google.Apis.Drive.v3.DriveService.Scope.DriveReadonly] }

let updateArrived (ctx: Funogram.Telegram.Bot.UpdateContext) =
  match ctx.Update.Message with
  | Some { From = Some from } when from.Id.ToString() = Config.get Config.Env.ADMIN_ID ->
    Funogram.Telegram.Bot.processCommands ctx [
      service
      |> Api.GoogleDrive.listFile
      |> Commands.createHandler
      |> Funogram.Telegram.Bot.cmd (Commands.getCommand(Types.Command.ListFile))
    ]
    |> ignore
  | _ -> ()

let handler = new System.Net.Http.HttpClientHandler()
let httpClient = new System.Net.Http.HttpClient(handler, true)
let botConfig = { Funogram.Telegram.Bot.Config.defaultConfig with 
                    Client = httpClient
                    Token = Config.get(Config.Env.BOT_TOKEN) }
let botCommands =
  [|
    Types.Command.ListFile
  |]
  |> Array.map Commands.createCommand
  |> Array.map Api.Telegram.createCommand

let botCommandScope = 
  Funogram.Telegram.Types.BotCommandScopeAllPrivateChats.Create("all_private_chats")
  |> Funogram.Telegram.Types.BotCommandScope.AllPrivateChats

async {
  match! Funogram
       .Telegram
       .Req
       .SetMyCommands
       .Make (botCommands, botCommandScope)
      |> Funogram.Api.api botConfig
  with
    | Ok _ -> ()
    | Error error -> 
      printfn "Failed to set bot commands: %A" error
      ()
  let! hook = 
    Funogram
     .Telegram
     .Req
     .SetWebhook
     .Make(Constants.WebsocketEndpoint)
    |> Funogram.Api.api botConfig

  match hook with
  | Ok _ ->
    use listener = new System.Net.HttpListener()
    listener.Prefixes.Add("http://*:4444/")
    listener.Start()
    let webhook = { Listener = listener; 
                    ValidateRequest = 
                      (fun request -> request.Url.LocalPath = Constants.ApiPath) }
    return! Funogram.Telegram.Bot.startBot { botConfig with WebHook = Some webhook } updateArrived None
  | Error error -> 
    printf "Can't setup webhook: %A" error
} |> Async.RunSynchronously
