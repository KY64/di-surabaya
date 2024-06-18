module Api.Telegram

open Types

let private baseUrl = System.Uri("https://api.telegram.org")
let private botToken = Config.get Config.Env.BOT_TOKEN
let private apiUrl = System.Uri(baseUrl, $"{baseUrl.AbsoluteUri}bot{botToken}")

let private telegramApi (api: string) (method: System.Net.Http.HttpMethod) (requestOption: Types.Request) =
  let endpoint = Util.appendUri apiUrl api
  requestOption
  |> Util.fetch Util.client endpoint method

type Bot =
  static member sendMessage (payload: Types.SendMessagePayload) = 
    let data: Types.Telegram.SendMessagePayload = {
      ChatId = payload.UserID
      Text = payload.Text
    }
    {
      Headers = []
      Content = Some (System.Net.Http.Json.JsonContent.Create(inputValue=data, options=Constants.jsonDeserializeOptions))
    }
    |> telegramApi "sendMessage" Types.Method.POST

let processWebhook (payload: Types.Request.TelegramWebhookPayload): Types.UserMessagePayload =
  let checkCommand (entity: Types.Request.TelegramWebhookMessageEntitiesPayload) =
    match entity with
    | entity when entity.Type = "bot_command" -> Some entity
    | _ -> None

  let isCommand =
    let length =
      payload.Message.Entities
      |> Option.defaultWith (fun () -> [])
      |> List.choose checkCommand
      |> List.length
    length > 0

  let text = ("", Some payload.Message.Text) ||> Option.defaultValue
  match isCommand with
  | true ->
    {
      UserID = payload.Message.From.Id
      Command = Command.find(text)
      Text = Some payload.Message.Text
    }
  | false ->
    {
      UserID = payload.Message.From.Id
      Command = None
      Text = Some payload.Message.Text
    }
