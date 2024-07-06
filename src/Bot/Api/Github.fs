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

type RerunWorkflowJobParams = {
  Owner: string
  Repository: string
  JobId: string
}

type UpdateEnvironmentRequest = {
  Name: string
  Value: string
}

let appName = Config.get Config.Env.APP_NAME
let privateKey = Config.get Config.Env.GITHUB_APPLICATION_PRIVATE_KEY
let baseUrl = System.Uri("https://api.github.com")

let githubApi (api: string) (method: System.Net.Http.HttpMethod) (requestOption: Types.Request) = 
  let endpoint = Util.appendUri baseUrl api
  requestOption
  |> Util.fetch Util.client endpoint method

let createJwt (clientId: string) =
  let rsaKey = System.Security.Cryptography.RSA.Create()
  let encodedPrivateKey = System.Text.Encoding.UTF8.GetBytes(privateKey)
  rsaKey.ImportRSAPrivateKey(encodedPrivateKey) |> ignore
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
  static createAccessToken (appId: string) (jwt: string) =
    let endpoint = $"/app/installations/{appId}/access_tokens"
    {
      Headers = [
        ("X-GitHub-Api-Version", "2022-11-28")
        ("User-Agent", appName);
        ("Authorization", $"Bearer {jwt}")
      ]
      Content = None
    }
    |> githubApi endpoint System.Net.Http.HttpMethod.Post
  static updateEnvironmentVariable (data: UpdateEnvironmentParams) (accessToken: string) =
    let endpoint = $"/repos/{data.Owner}/{data.Repository}/environments/{data.RepositoryEnvironment}/variables/{data.EnvironmentVariableName}"
    let body: UpdateEnvironmentRequest = {
      Name = data.EnvironmentVariableName
      Value = data.EnvironmentVariableValue
    }
    {
      Headers = [
        ("X-GitHub-Api-Version", "2022-11-28")
        ("User-Agent", appName);
        ("Authorization", $"Bearer {accessToken}")
      ]
      Content = Some (System.Net.Http.Json.JsonContent.Create(inputValue=body, options=Constants.jsonDeserializeOptions))
    }
    |> githubApi endpoint System.Net.Http.HttpMethod.Patch
  static rerunWorkflowJob (data: RerunWorkflowJobParams) (accessToken: string) =
    let endpoint = $"/repos/{data.Owner}/{data.Repository}/actions/jobs/{data.JobId}/rerun"
    {
      Headers = [
        ("X-GitHub-Api-Version", "2022-11-28")
        ("User-Agent", appName);
        ("Authorization", $"Bearer {accessToken}")
      ]
      Content = None
    }
    |> githubApi endpoint System.Net.Http.HttpMethod.Post
