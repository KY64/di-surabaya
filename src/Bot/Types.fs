module Types

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

type Command =
  | ListFile
  | GetFile

type CommandHandler =
  | ListFile of (string -> File list)
  | GetFile of (string -> File)
