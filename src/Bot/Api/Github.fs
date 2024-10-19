module Api.Github

open Types

type AccessTokenResponse = {
  token: string
}

type UpdateEnvironmentParams = {
  Owner: string
  Repository: string
  RepositoryEnvironment: string
  EnvironmentVariableName: string
  EnvironmentVariableValue: string
}

type RunWorkflowAfterUpdateEnvironmentParams = {
  Owner: string
  Repository: string
  RepositoryEnvironment: string
  EnvironmentVariableName: string
  EnvironmentVariableValue: string
  WorkflowFile: string
}

type RunWorkflowParams = {
  Owner: string
  Repository: string
  WorkflowFile: string
}

type RunWorkflowRequest = {
  Ref: string
}

type UpdateMapParams = {
  Owner: string
  Repository: string
  RepositoryEnvironment: string
  WorkflowFile: string
}

type UpdateEnvironmentRequest = {
  Name: string
  Value: string
}

let appName = Config.get Config.Env.APP_NAME
let appId = Config.get Config.Env.GITHUB_APPLICATION_ID
let clientId = Config.get Config.Env.GITHUB_CLIENT_ID
let privateKey = Config.get Config.Env.GITHUB_APPLICATION_PRIVATE_KEY
let baseUrl = System.Uri("https://api.github.com")

let githubApi (api: string) (method: System.Net.Http.HttpMethod) (requestOption: Types.Request) = 
  let endpoint = System.Uri(baseUrl, api)
  requestOption
  |> Util.fetch Util.client endpoint method

let private createJwt (clientId: string) =
  let rsaKey = System.Security.Cryptography.RSA.Create()
  rsaKey.ImportFromPem(privateKey) |> ignore
  let securityKey = new Microsoft.IdentityModel.Tokens.RsaSecurityKey(rsaKey)
  let algorithm = Microsoft.IdentityModel.Tokens.SecurityAlgorithms.RsaSha256
  let credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, algorithm)
  let descriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor(
    IssuedAt=System.DateTime.Now.AddMinutes(-5),
    Issuer=clientId,
    Expires=System.DateTime.Now.AddMinutes(2),
    SigningCredentials=credentials
  )
  let handler = Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler(SetDefaultTimesOnTokenCreation=false);
  handler.CreateToken(descriptor)

type ApiRequest =
  static createAccessToken (clientId: string) (appId: string) =
    let jwt = createJwt clientId
    let endpoint = $"/app/installations/{appId}/access_tokens"
    let response =
      {
        Headers = [
          ("X-GitHub-Api-Version", "2022-11-28")
          ("User-Agent", appName);
          ("Authorization", $"Bearer {jwt}")
        ]
        Content = None
      }
      |> githubApi endpoint System.Net.Http.HttpMethod.Post
    match response.Result with
    | Ok result ->
      Ok (System.Text.Json.JsonSerializer.Deserialize<AccessTokenResponse>(result.Content.ReadAsStringAsync().Result).token)
    | Error result ->
      Error $"Failed to get access token: {result.ReasonPhrase}"
  static updateEnvironmentVariable (data: UpdateEnvironmentParams) (accessToken: string) =
    let endpoint = $"/repos/{data.Owner}/{data.Repository}/environments/{data.RepositoryEnvironment}/variables/{data.EnvironmentVariableName}"
    let body: UpdateEnvironmentRequest = {
      Name = data.EnvironmentVariableName
      Value = data.EnvironmentVariableValue
    }
    let response =
      {
        Headers = [
          ("X-GitHub-Api-Version", "2022-11-28")
          ("User-Agent", appName);
          ("Authorization", $"Bearer {accessToken}")
        ]
        Content = Some (System.Net.Http.Json.JsonContent.Create(inputValue=body, options=Constants.jsonDeserializeOptions))
      }
      |> githubApi endpoint System.Net.Http.HttpMethod.Patch
    match response.Result with
    | Ok _ ->
      Ok $"Successfully update environment variable '{data.EnvironmentVariableName}' to '{data.EnvironmentVariableValue}'"
    | Error result ->
      Error $"Failed to update environment variable: {result.ReasonPhrase}"
  static runWorkflow (data: RunWorkflowParams) (accessToken: string) =
    let endpoint = $"/repos/{data.Owner}/{data.Repository}/actions/workflows/{data.WorkflowFile}/dispatches"
    let body: RunWorkflowRequest = {
      Ref = "main" // Always run workflow from 'main' branch
    }
    let response =
      {
        Headers = [
          ("X-GitHub-Api-Version", "2022-11-28")
          ("User-Agent", appName);
          ("Authorization", $"Bearer {accessToken}")
        ]
        Content = Some (System.Net.Http.Json.JsonContent.Create(inputValue=body, options=Constants.jsonDeserializeOptions))
      }
      |> githubApi endpoint System.Net.Http.HttpMethod.Post
    match response.Result with
    | Ok _ ->
      Ok $"Successfully rerun workflow job '{data.WorkflowFile}'"
    | Error result ->
      Error $"Failed to rerun workflow job '{data.WorkflowFile}': {result.ReasonPhrase}"

let private runWorkflowJobAfterUpdateEnvironment (config: RunWorkflowAfterUpdateEnvironmentParams) =
  let updateEnvironmentParams: UpdateEnvironmentParams = 
    {
      Owner = config.Owner
      Repository = config.Repository
      RepositoryEnvironment = config.RepositoryEnvironment
      EnvironmentVariableName = config.EnvironmentVariableName
      EnvironmentVariableValue = config.EnvironmentVariableValue
    }
  let runWorkflowJobParams: RunWorkflowParams =
    {
      Owner = config.Owner
      Repository = config.Repository
      WorkflowFile = config.WorkflowFile
    }
  let accessToken = ApiRequest.createAccessToken clientId appId
  match
    accessToken
    |> Result.bind (ApiRequest.updateEnvironmentVariable updateEnvironmentParams)
  with
  | Ok _ ->
    accessToken
    |> Result.bind (ApiRequest.runWorkflow runWorkflowJobParams)
    |> Util.getResultValue
  | Error error ->
    error

let updateMap (config: UpdateMapParams): Types.CommandHandler =
  let execute (mapType: Types.Map) (mapId: string) =
    let environmentVariableName =
      match mapType with
      | Types.Map.Bus ->
        Config.Env.BUS_MAP_FILE_ID.ToString() |> Some
      | Types.Map.Train ->
        Config.Env.TRAIN_MAP_FILE_ID.ToString() |> Some
      | Types.Map.Undefined -> None

    match environmentVariableName with
    | Some name ->
      {
        Owner = config.Owner
        Repository = config.Repository
        RepositoryEnvironment = config.RepositoryEnvironment
        EnvironmentVariableName = name
        EnvironmentVariableValue = mapId
        WorkflowFile = config.WorkflowFile
      }
      |> runWorkflowJobAfterUpdateEnvironment
    | None ->
      $"Unknown map type: only '{Types.Map.Bus.ToString()}' and '{Types.Map.Train.ToString()}' supported"
  Types.CommandHandler.UpdateMap execute
