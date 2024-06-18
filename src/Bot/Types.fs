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
