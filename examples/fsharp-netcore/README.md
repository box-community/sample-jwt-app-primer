# F# JWT example

This cross-platform .NET Core app will create an authenticated Box client for your JWT app service account, print the name and login for that service account, and print out the folder tree for a specified Box folder.

## Prerequisites

Download and install the [.NET Core 2.1 SDK](https://www.microsoft.com/net/download/dotnet-core/sdk-2.1.300) or newer.  You can confirm that the SDK is properly isntalled and ready with the following command.

```console
user@home:~$ dotnet --info
.NET Core SDK (reflecting any global.json):
 Version:   2.1.300
```

## Configuration

There are two configuration variables to set in `Program.fs`.

| Value | Description |
|-------|-------------|
| pathToConfigJson | The path on your file system to the JSON config file for the JWT app. For more info see step 8 of the [Configure a New JWT Application](https://github.com/box-community/jwt-app-primer#configure-a-new-jwt-application) section of the primer.
| folderId | The Box folder ID whose contents you wish to list. Defaults to "0", the root folder of the Service Account. You can specify any folder for which the Service Account has read collaborations permissions.

## Usage

After setting the above configuration variables execute the following:

```console
user@home:~$ dotnet build
user@home:~$ dotnet run
```
