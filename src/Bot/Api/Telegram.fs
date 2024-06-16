module Api.Telegram

open Funogram.Telegram.Types

let sendMessage (context: Funogram.Telegram.Bot.UpdateContext) (message: string) =
  match context.Update.Message with
  | Some { From = Some from } ->
    async {
      match! Funogram
             .Telegram
             .Req
             .SendMessage
             .Make(chatId=from.Id,
                   text=message) 
        |> Funogram.Api.api context.Config 
           with
           | Result.Ok _ -> 
             printfn "Message sent successfully on ID %d" from.Id
           | Result.Error _ -> printf "Cannot send message!"
    } |> Async.Start
  | _ -> ()

let createCommand (command: Types.CommandInfo): Funogram.Telegram.Types.BotCommand =
  Funogram.Telegram.Types.BotCommand.Create(command.command, command.description)
