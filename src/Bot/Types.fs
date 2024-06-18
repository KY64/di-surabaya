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

type UserMessagePayload = {
  UserID: int32
  Command: string option
  Text: string option
}

type Command =
  | ListFile
  | GetFile

type CommandHandler =
  | ListFile of (string -> File list)
  | GetFile of (string -> File)

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

  type WebhookMessageFromPayload = {
    [<System.Runtime.Serialization.DataMember(Name = "id")>]
    Id: int
    [<System.Runtime.Serialization.DataMember(Name = "is_bot")>]
    IsBot: bool
    [<System.Runtime.Serialization.DataMember(Name = "username")>]
    Username: string
  }

  type WebhookMessageChatPayload = {
    [<System.Runtime.Serialization.DataMember(Name = "type")>]
    Type: string
  }

  type WebhookMessageEntitiesPayload = {
    [<System.Runtime.Serialization.DataMember(Name = "offset")>]
    Offset: int
    [<System.Runtime.Serialization.DataMember(Name = "length")>]
    Length: int
    [<System.Runtime.Serialization.DataMember(Name = "Type")>]
    Type: string
  }

  type WebhookMessagePayload = {
    [<System.Runtime.Serialization.DataMember(Name = "message_id")>]
    MessageId: int
    [<System.Runtime.Serialization.DataMember(Name = "from")>]
    From: WebhookMessageFromPayload
    [<System.Runtime.Serialization.DataMember(Name = "chat")>]
    Chat: WebhookMessageChatPayload
    [<System.Runtime.Serialization.DataMember(Name = "text")>]
    Text: string
    [<System.Runtime.Serialization.DataMember(Name = "entities")>]
    Entities: WebhookMessageEntitiesPayload list option
  }

  type WebhookPayload = {
    [<System.Runtime.Serialization.DataMember(Name = "id")>]
    UpdateId: int
    [<System.Runtime.Serialization.DataMember(Name = "message")>]
    Message: WebhookMessagePayload
  }
