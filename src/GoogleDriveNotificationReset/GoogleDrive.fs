module GoogleDrive

open Google.Apis.Auth.OAuth2
open Google.Apis.Drive.v3
open Google.Apis.Services

type Credential = {
  auth_uri: string
  client_email: string
  client_id: string
  private_key: string
  private_key_id: string
  project_id: string
  token_uri: string
  universe_domain: string
  scopes: string list
}

let Env = System.Environment.GetEnvironmentVariable
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

let credentials: Credential = {
  auth_uri = "https://accounts.google.com/o/oauth2/auth"
  client_email = Env("GOOGLE_DRIVE_OAUTH_CLIENT_EMAIL")
  client_id = Env("GOOGLE_DRIVE_OAUTH_CLIENT_ID")
  private_key = Env("GOOGLE_DRIVE_OAUTH_PRIVATE_KEY")
  private_key_id = Env("GOOGLE_DRIVE_OAUTH_PRIVATE_KEY_ID")
  project_id = Env("GOOGLE_DRIVE_OAUTH_PROJECT_ID")
  token_uri = "https://oauth2.googleapis.com/token"
  universe_domain = "googleapis.com"
  scopes = []
}

let initialize (credential: Credential) = 

  let initializeServiceAccount = ServiceAccountCredential.Initializer(
    credential.client_email,
    credential.token_uri,
    ProjectId=credential.project_id,
    UniverseDomain=credential.universe_domain,
    KeyId=credential.private_key_id,
    UseJwtAccessWithScopes=false
  )

  let serviceAccount = ServiceAccountCredential(
   initializeServiceAccount.FromPrivateKey(credential.private_key),
   Scopes=credential.scopes
  )

  let accessToken = serviceAccount.GetAccessTokenForRequestAsync(credential.auth_uri)
  accessToken.Wait()
  let initializer = BaseClientService.Initializer(HttpClientInitializer=serviceAccount)
  new DriveService(initializer)
