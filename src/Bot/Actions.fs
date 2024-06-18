module Actions

open Types

let sendMessage (payload: SendMessagePayload) =
  match (Api.Telegram.Bot.sendMessage payload).Result with
  | Ok _ -> 
    printfn $"Successfully send message to {payload.UserID.ToString()}"
  | Error response ->
    printfn $"Failed to send message to {payload.UserID}: {response.Content.ReadAsStringAsync().Result}"
