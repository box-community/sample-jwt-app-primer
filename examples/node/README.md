# F# JWT example

This simple node app will create an authenticated Box client for your JWT app service account, print the name and login for that service account, and print out the folder tree for a specified Box folder.

## Prerequisites

Download and install [Node.js](https://nodejs.org).  You can confirm that Node is installed and ready with the following command.

```
$ node --version
v8.9.4
```

## Configuration

There are two configuration variables to set in `app.js`.

| Value | Description |
|-------|-------------|
| pathToConfigJson | The path on your file system to the JSON config file for the JWT app. For more info see step 8 of the [Configure a New JWT Application](https://github.com/box-community/jwt-app-primer#configure-a-new-jwt-application) section of the primer.
| folderId | The Box folder ID whose contents you wish to list. Defaults to "0", the root folder of the Service Account. You can specify any folder for which the Service Account has read collaborations permissions.

## Usage

After setting the above configuration variables execute the following in a terminal.

```
npm install
node app.js
```
