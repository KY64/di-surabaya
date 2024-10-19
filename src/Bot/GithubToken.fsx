#r "nuget: Microsoft.IdentityModel.Tokens, 8.1.2"
#r "nuget: Microsoft.IdentityModel.JsonWebTokens, 8.1.2"

type Request = {
  Headers: List<string * string>
  Content: System.Net.Http.HttpContent option
}

type AccessTokenResponse = {
  token: string
}

let env = System.Environment.GetEnvironmentVariable
let clientId = env "GITHUB_CLIENT_ID"
let appId = env "GITHUB_APPLICATION_ID"
let appName = env "APP_NAME"
let privateKey = env "GITHUB_APPLICATION_PRIVATE_KEY"
let baseUrl = System.Uri("https://api.github.com")

let fetch (url: System.Uri) (method: System.Net.Http.HttpMethod) (requestOption: Request) =
  let handler = new System.Net.Http.HttpClientHandler()
  let client = new System.Net.Http.HttpClient(handler, true)
  let request = new System.Net.Http.HttpRequestMessage(method, url)
  requestOption.Headers |> List.iter request.Headers.Add
  requestOption.Content |> Option.iter (fun content -> request.Content <- content)
  task {
    let! response = client.SendAsync(request)
    return 
      match response.IsSuccessStatusCode with
       | true -> Ok response
       | false -> Error response
  }

let githubApi (api: string) (method: System.Net.Http.HttpMethod) (requestOption: Request) = 
  let endpoint = System.Uri(baseUrl, api)
  requestOption
  |> fetch endpoint method

let createJwt (clientId: string) =
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

let createAccessToken (clientId: string) (appId: string) =
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

let accessToken = createAccessToken clientId appId

match accessToken with
  | Ok result -> printfn "%A" result
  | Error _ -> ()
