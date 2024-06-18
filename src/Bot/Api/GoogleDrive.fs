module Api.GoogleDrive

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
  
let listFile (service: DriveService): Types.CommandHandler =
  let execute (fileId: string): Types.File list =
    let query = $"'{fileId}' in parents"
    let listRequest = service.Files.List(Q=query, Fields="*")
    let fileList = listRequest.Execute()
    let getFileInfo (googleFile: Google.Apis.Drive.v3.Data.File): Types.File =
      let fileModifiedTime = googleFile.ModifiedTimeDateTimeOffset.Value.ToUniversalTime()
      { name = googleFile.Name
        id = googleFile.Id
        url = googleFile.WebViewLink
        modifiedTime = fileModifiedTime.DateTime }

    fileList.Files 
    |> Seq.map getFileInfo 
    |> List.ofSeq
  Types.CommandHandler.ListFile execute