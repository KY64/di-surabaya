module Commands

open Types

let private formatFileDetails (initialState: string) (file: Types.File) =
  let modifiedTime = file.modifiedTime.ToString("f")
  initialState 
  + $"\n\n[{file.name}]"
  + $"\nModified: {modifiedTime}"
  + $"\nID: {file.id}"
  + $"\n{file.url}"

let private formatFileMessage (files: Types.File list) =
  ("", files)
  ||> List.fold formatFileDetails

let createCommand (command: Types.Command): CommandInfo =
  match command with
  | Types.ListFile -> { command = "/list" ; description = "List file in a folder" }
  | Types.GetFile -> { command = "/file" ; description = "Get file information" }

let getCommand (command: Types.Command): string =
  createCommand(command).command

let createHandler (handler: Types.CommandHandler) (context: Types.UserMessagePayload) =
  match handler with
  | Types.CommandHandler.ListFile execute -> 
    let message =
      execute (Config.get Config.Env.DRIVE_FOLDER_ID)
      |> formatFileMessage

    Actions.sendMessage {UserID = context.UserID; Text = message}
  | Types.CommandHandler.GetFile execute ->
    ()
