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
    [<System.Runtime.Serialization.DataMember(Name = "chat_id")>]
    ChatId: int32
    [<System.Runtime.Serialization.DataMember(Name = "text")>]
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
    [<System.Runtime.Serialization.DataMember(Name = "id")>]
    Id: int
    [<System.Runtime.Serialization.DataMember(Name = "is_bot")>]
    IsBot: bool
    [<System.Runtime.Serialization.DataMember(Name = "username")>]
    Username: string
  }

  type TelegramWebhookMessageChatPayload = {
    [<System.Runtime.Serialization.DataMember(Name = "type")>]
    Type: string
  }

  type TelegramWebhookMessageEntitiesPayload = {
    [<System.Runtime.Serialization.DataMember(Name = "offset")>]
    Offset: int
    [<System.Runtime.Serialization.DataMember(Name = "length")>]
    Length: int
    [<System.Runtime.Serialization.DataMember(Name = "Type")>]
    Type: string
  }

  type TelegramWebhookMessagePayload = {
    [<System.Runtime.Serialization.DataMember(Name = "message_id")>]
    MessageId: int
    [<System.Runtime.Serialization.DataMember(Name = "from")>]
    From: TelegramWebhookMessageFromPayload
    [<System.Runtime.Serialization.DataMember(Name = "chat")>]
    Chat: TelegramWebhookMessageChatPayload
    [<System.Runtime.Serialization.DataMember(Name = "text")>]
    Text: string
    [<System.Runtime.Serialization.DataMember(Name = "entities")>]
    Entities: TelegramWebhookMessageEntitiesPayload list option
  }

  type TelegramWebhookPayload = {
    [<System.Runtime.Serialization.DataMember(Name = "id")>]
    UpdateId: int
    [<System.Runtime.Serialization.DataMember(Name = "message")>]
    Message: TelegramWebhookMessagePayload
  }
