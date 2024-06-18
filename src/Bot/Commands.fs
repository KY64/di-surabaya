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


let handleCommand (context: Types.UserMessagePayload) (handler: Types.CommandHandler) =
  let command = 
    match context with
    | { Command = Some command } -> command
    | _ -> Types.Command.NoCommand

  match handler with
  | Types.CommandHandler.ListFile execute when command = Types.Command.ListFile -> 
    let message =
      execute (Config.get Config.Env.DRIVE_FOLDER_ID)
      |> formatFileMessage

    Actions.sendMessage {UserID = context.UserID; Text = message}
  | Types.CommandHandler.GetFile execute when command = Types.Command.GetFile ->
    ()
  | _ -> ()
