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

let mapUpdateMapInput (input: string): Option<Types.Map * string> =
  match input.Split(" ") with
  | splitInput when splitInput.Length = 3 ->
    Some (Types.Map.Create(splitInput[1]), splitInput[2])
  | _ ->
    None

let parseCommand (context: Types.UserMessagePayload) = 
  match context with
  | { Command = Some command } when context.UserID.ToString() = Config.get Config.Env.ADMIN_ID -> command
  | _ -> Types.Command.NoCommand

let handleCommand (context: Types.UserMessagePayload) (handler: Types.CommandHandler) =
  match handler with
  | Types.CommandHandler.ListFile execute -> 
    let message =
      execute (Config.get Config.Env.DRIVE_FOLDER_ID)
      |> formatFileMessage

    Actions.sendMessage {UserID = context.UserID; Text = message}
  | Types.CommandHandler.UpdateMap execute ->
    match context.Text with
    | Some text ->
      match mapUpdateMapInput text with
      | Some (mapType, mapId) ->
        let message =
          execute mapType mapId
        Actions.sendMessage {UserID = context.UserID; Text = message}
      | None -> ()
    | None -> ()
