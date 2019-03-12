# Python JWT example

This example will create an authenticated Box client for your JWT app service account, print the name and login for that service account, and print out the folder tree for a specified Box folder.

## Prerequisites

Download and install [Python 3.x](https://www.python.org/getit/). You can confirm that Python is installed and ready with the following command.

```
$ python --version
Python 3.6.5 :: ....
```

## Configuration

There are two configuration variables to set in `app.py`.

| Value | Description |
|-------|-------------|
| pathToConfigJson | The path on your file system to the JSON config file for the JWT app. For more info see step 8 of the [Configure a New JWT Application](https://github.com/box-community/jwt-app-primer#configure-a-new-jwt-application) section of the primer.
| folderId | The Box folder ID whose contents you wish to list. Defaults to "0", the root folder of the Service Account. You can specify any folder for which the Service Account has read collaborations permissions.

## Usage

After setting the above configuration variables execute the following in a terminal.

```
pip3 install -r requirements.txt
python3 app.py
```
