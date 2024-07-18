namespace GoogleDriveNotificationReset


open Amazon.Lambda.Core

open System
open GoogleDrive


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer>)>]
()


type Function() =
    /// <summary>
    /// A function that set google drive notification
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    member __.FunctionHandler (input: string) (_: ILambdaContext) =
        // Google Drive will stop pushing notification after 1 day and requires a reset to expire in another 1 day
        // Reference: https://developers.google.com/drive/api/guides/push#optional-properties
        let maxGoogleDriveNotificationExpiration = System.DateTimeOffset.Now.ToUnixTimeSeconds() + 86400L
        let googleDriveFolderId = Env("DRIVE_FOLDER_ID")
        let googleDriveNotificationChannel =
          new Google.Apis.Drive.v3.Data.Channel(
            Id=Env("GOOGLE_DRIVE_NOTIFICATION_ID"),
            Address=Env("NOTIFY_ENDPOINT"),
            Expiration=maxGoogleDriveNotificationExpiration,
            ResourceId=googleDriveFolderId,
            Type="webhook"
          )

        let googleDriveService = 
          {
            auth_uri = Env("GOOGLE_DRIVE_OAUTH_AUTH_URI")
            client_email = Env("GOOGLE_DRIVE_OAUTH_CLIENT_EMAIL")
            client_id = Env("GOOGLE_DRIVE_OAUTH_CLIENT_ID")
            private_key = Env("GOOGLE_DRIVE_OAUTH_PRIVATE_KEY")
            private_key_id = Env("GOOGLE_DRIVE_OAUTH_PRIVATE_KEY_ID")
            project_id = Env("GOOGLE_DRIVE_OAUTH_PROJECT_ID")
            token_uri = Env("GOOGLE_DRIVE_OAUTH_TOKEN_URI")
            universe_domain = Env("GOOGLE_DRIVE_OAUTH_UNIVERSE_DOMAIN")
            scopes = [Google.Apis.Drive.v3.DriveService.Scope.DriveReadonly]
          }
          |> GoogleDrive.initialize

        let googleDriveResponse = googleDriveService.Files.Watch(fileId=googleDriveFolderId, body=googleDriveNotificationChannel).Execute()
        printfn "Google Drive Response: Notification created with ID %A" googleDriveResponse.Id
        {| 
          statusCode = System.Net.HttpStatusCode.OK
        |}
