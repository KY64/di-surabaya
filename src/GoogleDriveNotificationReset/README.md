# Google Drive Notification Reset

This folder contains source code of function that is used to reset Google Drive Notification.

## Requirements

- [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

## How To Use

This part is step that we need to do after cloning/downloading this repository.

### 1. Install Dependency

- Go to **GoogleDriveNotificationReset** folder first, in a terminal we can do

```sh
cd src/GoogleDriveNotificationReset
```

- Install dependency

```
dotnet restore
```

### 2. Deploy

- Install Amazon.Lambda.Tools Global Tools if not already installed.

```
dotnet tool install -g Amazon.Lambda.Tools
```

- Deploy the function to lambda by typing

```
dotnet lambda deploy-function
```

> The deploy configuration is stored in the **aws-lambda-tools-defults.json** be sure to check it
> and change according to your requirement

## Project Structure

This section explain role of each file in the folder:

- Function.fs - Code file containing the function handler method, this is the main function that is executed in the AWS Lambda
- GoogleDrive.fs - Code file related to Google Drive API
- GoogleDriveNotificationReset.fsproj - Fsharp project file
- aws-lambda-tools-defaults.json - default argument settings for use with Visual Studio and command line deployment tools for AWS
