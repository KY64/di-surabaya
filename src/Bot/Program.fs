let service = Api
               .GoogleDrive
               .initialize { auth_uri = Config.get Config.Env.GOOGLE_DRIVE_OAUTH_AUTH_URI
                             client_email= Config.get Config.Env.GOOGLE_DRIVE_OAUTH_CLIENT_EMAIL
                             client_id= Config.get Config.Env.GOOGLE_DRIVE_OAUTH_CLIENT_ID
                             private_key= Config.get Config.Env.GOOGLE_DRIVE_OAUTH_PRIVATE_KEY
                             private_key_id= Config.get Config.Env.GOOGLE_DRIVE_OAUTH_PRIVATE_KEY_ID
                             project_id= Config.get Config.Env.GOOGLE_DRIVE_OAUTH_PROJECT_ID
                             token_uri= Config.get Config.Env.GOOGLE_DRIVE_OAUTH_TOKEN_URI
                             universe_domain= Config.get Config.Env.GOOGLE_DRIVE_OAUTH_UNIVERSE_DOMAIN
                             scopes= [Google.Apis.Drive.v3.DriveService.Scope.DriveReadonly] }
