# Bot

This folder contains source code of the bot that is used to do administrative task of the website.

## Requirements

- [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

## How To Use

This part is step that we need to do after cloning/downloading this repository.

### 1. Install Dependency

- Go to **Bot** folder first, in a terminal we can do

```sh
cd src/Bot
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

- Api - It's a directory where it contains source code that wraps third party API/library
- Actions.fs - Code file that contains the bot action
- Commands.fs - Code file that contains function to handle Bot command
- Config.fs - Code file contains global configuration that can be used throughout the codebase
- Constants.fs - Code file that contains constant value
- Script.fsx - A script that is used to setup the Bot webhook and reset Google Drive Notification
- Types.fs - Code file that contains domain type 
- Util.fs - Code file that contains utility function
- Function.fs - Code file containing the function handler method, this is the main function that is executed in the AWS Lambda
- aws-lambda-tools-defaults.json - default argument settings for use with Visual Studio and command line deployment tools for AWS
