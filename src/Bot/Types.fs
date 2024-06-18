module Types

type Request = {
  Headers: List<string * string>
  Content: System.Net.Http.HttpContent option
}

type Method =
  static member POST = System.Net.Http.HttpMethod.Post

type File = {
  name: string
  id: string
  modifiedTime: System.DateTime
  url: string
}

type CommandInfo = {
  command: string
  description: string
}

type SendMessagePayload = {
  UserID: int32
  Text: string
}

type Command =
  | ListFile
  | GetFile
  | NoCommand

  static member find (input: string): Command option =
    match input with
    | input when input = Command.ListFile.get.command -> Some Command.ListFile
    | input when input = Command.GetFile.get.command -> Some Command.GetFile
    | _ -> None

  member this.get: CommandInfo = 
    match this with
    | ListFile -> { command = "/list" ; description = "List file in a folder" }
    | GetFile -> { command = "/file" ; description = "Get file information" }
    | NoCommand -> { command = "" ; description = "" }

type CommandHandler =
  | ListFile of (string -> File list)
  | GetFile of (string -> File)

type UserMessagePayload = {
  UserID: int32
  Command: Command option
  Text: string option
}

module Telegram =

  type SendMessagePayload = {
    ChatId: int32
    Text: string
  }

module Lambda =

  type RequestContext = {
    domainName: string
    routeKey: string
  }

  type ApiGatewayEvent = {
    body: string
    rawPath: string
    headers: Map<string, string>
    requestContext: RequestContext
  }

module Request =

  type TelegramWebhookMessageFromPayload = {
    Id: int
    IsBot: bool
    Username: string
  }

  type TelegramWebhookMessageChatPayload = {
    Type: string
  }

  type TelegramWebhookMessageEntitiesPayload = {
    Offset: int
    Length: int
    Type: string
  }

  type TelegramWebhookMessagePayload = {
    MessageId: int
    From: TelegramWebhookMessageFromPayload
    Chat: TelegramWebhookMessageChatPayload
    Text: string
    Entities: TelegramWebhookMessageEntitiesPayload list option
  }

  type TelegramWebhookPayload = {
    UpdateId: int
    Message: TelegramWebhookMessagePayload
  }
