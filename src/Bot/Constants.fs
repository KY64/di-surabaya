module Constants

let botToken = Config.get(Config.Env.BOT_TOKEN)
let jsonDeserializeOptions = new System.Text.Json.JsonSerializerOptions(
  PropertyNamingPolicy=System.Text.Json.JsonNamingPolicy.SnakeCaseLower
)
