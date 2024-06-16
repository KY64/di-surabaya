module Constants

let private botToken = Config.get(Config.Env.BOT_TOKEN)
let private endpoint = System.Uri(Config.get(Config.Env.WEBHOOK_URL))

let ApiPath = $"/{botToken}"
let WebsocketEndpoint = System.Uri(endpoint, botToken).AbsoluteUri
