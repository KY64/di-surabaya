module Actions

let sendMessage (context) (message: string) =
  Api.Telegram.sendMessage context message
