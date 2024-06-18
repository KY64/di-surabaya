module Api.Telegram

open Types

let private baseUrl = System.Uri("https://api.telegram.org")
let private botToken = Config.get Config.Env.BOT_TOKEN
let private apiUrl = System.Uri(baseUrl, $"{baseUrl.AbsoluteUri}bot{botToken}")

let private telegramApi (api: string) (method: System.Net.Http.HttpMethod) (requestOption: Types.Request) =
  let endpoint = Util.appendUri apiUrl api
  requestOption
  |> Util.fetch Util.client endpoint method

type SendMessagePayload = {
  [<System.Runtime.Serialization.DataMember(Name = "chat_id")>]
  ChatId: int32
  [<System.Runtime.Serialization.DataMember(Name = "text")>]
  Text: string
}

type Bot =
  static member sendMessage (payload: Types.SendMessagePayload) = 
    let data = {
      ChatId = payload.UserID
      Text = payload.Text
    }
    {
      Headers = []
      Content = Some (System.Net.Http.Json.JsonContent.Create(data))
    }
    |> telegramApi "sendMessage" Types.Method.POST
