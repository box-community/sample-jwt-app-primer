# Box + JWT Primer

This document describes a method for provisioning Box applications using OAuth2 with JWT. It is aimed at developers wishing to build server-based applications or scripts that target personal- or group-owned enterprise content. 

## A (Very) Short Introduction to OAuth2 and the Problem it Solves

Imagine you are a Box user and you have an application that want to access your Box files. You'd like to grant this access and you have no reason to mistrust the app, but nevertheless you are a good security-minded Box user and some concerns come to mind:

1. Authentication: Will I have to share my Box/SSO credentials with the app?
2. Authorization: How do I know what the app can do with my files?
3. Revocation: How am I be assured that I can turn that sharing off in the future?

OAuth2 is an industry-standard authorization framework designed to address these concerns. Specifically:

1. Your credentials are never shared with the app. Instead, Box gives the app a unique, time-limited token with which the app can make Box API calls on your behalf.
2. The app developer must specify up front what permissions ("scopes") it needs on your files. Prior to authorizing access you can review and confirm the requested permissions. Box enforces those permissions at the API level.
3. At any time, you can tell both the app and/or Box to stop sharing your data. Future API calls from the app will be rejected as unauthorized.

## The Two Flavors of OAuth2

*"3-legged" OAuth2* was designed for mobile/browser application development. It enables users to securely share their content (such as Box files) with a 3rd party application and revoke that permission at any time. For many years this was the only authentication method provided by Box.

3-legged OAuth has some design features that make it great for mobile/browser apps but difficult to use with enterprise/administrative scripts and applications. The browser-based authentication model requires human interaction and is difficult to script. It requires the application to maintain and update certain pieces of authorization state over time, the loss of which can disable the application. It does not play nicely in environments with concurrent/HA processes.

*OAuth2 with JSON Web Token (JWT)* was designed for server-to-server interaction (batch jobs, adminstrative tasks, etc.) It differs from 3-legged OAuth in some key ways that make it more suitable for server-based applications:

+ **JWT auth does not require human interaction beyond the initial setup and configuration.** During the configuration process a developer will select the permissions the application will need to do to perform its tasks (e.g. 'Manage Enterprise Users' or 'Manage Groups'). An Enterprise co-admin pre-authorizes the application with those specific permissions. The JWT authorization process is then fully programmatic and no browser interaction is required.

+ **JWT auth is designed for multi-process/multi-node/HA environments.** Multiple API access tokens can be in use at any given time across multiple processes/nodes. When a process needs an API access token, it simply requests one from Box using a request signed by the applications' private key.

+ **JWT access tokens are stateless.** When an access token expires it is not refreshed; you simply request a new one. Eliminating refresh tokens reduces the amount of state that must be maintained and managed by the application, and removes the risk and frustration of disabling the application following a failure to properly maintain refresh tokens.

## Box Service Accounts vs Box Enterprise Accounts

Applications using 3-legged OAuth are typically intended to interact with an individual Enterprise user's account. That authorization process results in an access token that is tied to some existing Box account.

Server-based JWT-based applications *may* access user data, but they may have other intended uses: updating Box group definitions, syncing user accounts, etc. Therefore, a JWT applicaton is associated with a Service Account that is created automatically when you setup the JWT app.

The Service Account is a full-fledged Box account, however it is not directly tied to your Box Enterprise, nor can it be viewed or managed through your standard Enterprise administration tools. Nevertheless, Service Accounts are an ideal model for fine-grained access control:

+ By default, a JWT application has no access to your existing Enterprise content. Access to content can be selectively granted and managed through collaborations. 
+ Unlike 3-legged OAuth, a JWT app's permissions are not related to an existing Enterprise user. The JWT app will be granted the exact permissions that you select when configuring the app.

# Creating a Box JWT Application

Refer to the Box documentation on [Authentication with JWT](https://developer.box.com/docs/authentication-with-jwt) for detailed information on working with JWT applications. Note that the Box documentation refers to certain Box Platform features such as 'App Auth' and 'App Users' which also use JWT but are not intended to work with existing Enterprise content.

## Configure a New JWT Application

1. Browse to https://developers.box.com/ and Log In.
2. Choose **Create New App** in the upper-right corner.
3. On the _Let's Get Started_ page, choose **Enterprise Integration**. Click **Next**.
4. On the _Authentication Method_ page, chose **OAuth 2.0 with JWT (Server Authentication)**. Click **Next**.
5. On the _What would you like to name your app?_ page, select a meaningful name for your application. Note that this will be the name visible to users on any collaboration folders to which this app has access. Click **Create App**.
6. Click **View Your App**

7. Configure the applications permissions under _Application Scopes_, _Application Scopes_, and _Advanced Features_. The following diagram represents the most restrictive (safest) permission set. You can select elevated permissions to satisfy the requirements of your application.  

![Configuration1.PNG](img/Configuration1.PNG)  

8. Under _Add and Manage Public Keys_ choose **Generate a Public/Private Keypair**. A JSON file will be downloaded by your browser. *This file contains all the credentials needed to authenticate your JWT application, so keep it stored securely and do not commit it to version control!*  

9. Click **Save Changes** to finalize your configuration.

You can come back and update this configuration at any time, however if you modify the application permissions it must be reauthorized by an enterprise co-admin.

## Authorize your JWT Application

JWT apps differ from 3-legged apps in that they are pre-authorized by your Enterprise Co-Admin for use with their requested permissions. The Co-Admin can revoke authorization at any time, and reauthorization is required if the permission set is ever changed. The steps for this authorization are layed out below. Once this is finished, continue to the [First Steps with your JWT Application](#first-steps-with-your-jwt-application).

#### For Developers

From your JWT app Configuration screen, scroll to the _OAuth 2.0 Credentials_. Send the _Client ID_ to your Box Enterprise Co-Admin. The Client _ID_ is not a secret and can safely be sent in email or chat.

![Configuration2.PNG](img/Configuration2.PNG)

#### For Co-Admins

1. Browse to your Admin Console, select the **Gear** symbol in the upper-right of the screen, and select **Elite Settings**
![AdminConsole1.PNG](img/AdminConsole1.PNG)
2. Select the **Apps** tab. Under _Custom Applications_ choose **Authorize New App**.
![AdminConsole2.PNG](img/AdminConsole2.PNG)
3. In the _API Key_ field, paste in the _Client ID_ that was sent to you by the Developer. Click **Next**.
4. Review the requested permissions. Contact the Developer if you wish to inquire as to why a specific permission is required. Click **Authorize** to finalize the approval process.
![AdminConsole3.PNG](img/AdminConsole3.PNG)
5. The JWT app is now authorized for use and will apear under the list of _Custom Applications_.

## Accessing Departmental/Group Content with your JWT App

To work with files in user or departmental Box folder, the JWT app service account must be invited to collaborate on that folder. You can limit the app's permissions on your folder by choosing the appropriate [collaboration role](https://community.box.com/t5/Collaborate-By-Inviting-Others/Understanding-Collaborator-Permission-Levels/ta-p/144). Some example code is included in this repository to help you discover the service account login. 

The Service Account can be invited to a collaboration like any other Box user. Simply copy and paste the service account login from the previous step into the invitee field and give the Service Account the appropriate level of access to the folder content.

![Collaboration1.PNG](img/Collaboration1.PNG)

The collaboration will be automatically accepted and the Service Account will appear as an external collaborator. Your JWT app can now work with data in that collaboration folder.

![Collaboration2.png](img/Collaboration2.png)

## JWT App Examples

Browse the project below for code examples to discover the service account login and list out a box folder tree. In each of these examples you will need the JSON config file that was downloaded when configuring your JWT app above.

* [.Net (C#)](examples/csharp-netcore)
* .Net (F#)
* Python
* Javascript

## Discover the Service Account Login

Authenticating with JWT requires several parameters:

1. The JWT application _Client ID_ and _Client Secret_, located in the _OAuth 2.0 Credentials_ section of the JWT app configuration.
2. The JWT application _Public Key ID_, located in the _Add and Manage Public Keys_ section of the JWT app configuration.
3. The ID of the Enterprise in which the JWT app was created. You may need to consult with your Enterprise Co-Admin to find this value.
4. The path to the private key file. This path must be accessible from the machine on which the script is run.

### Python Example

#### Modules

* pip install boxsdk --pre
* pip install boxsdk[jwt] --pre

#### Code

```python
# load the private key passphrase from the environment
import os
privateKeyPassphrase = os.environ['jwtPrivateKeyPassword']

# Create an authentication object using the JWT parameters for the application.
from boxsdk import JWTAuth, Client

auth = JWTAuth(
    client_id='8grfp2owbhpa2qrnsw4c5urmlwh5ybdz',
    client_secret='1FwTmLNWRbuSjWx7M6dTSUBS6k5h3x1y',
    enterprise_id='322105',  # IU's Dev Enterprise ID; yours may differ.     
    jwt_key_id='uaigav1k',   # the Public Key ID
    rsa_private_key_file_sys_path='C:\\OpenSSL-Win64\\bin\\private_key.pem',
    rsa_private_key_passphrase=str.encode(privateKeyPassphrase)
)

# Authenticate the JWT instance.
token = auth.authenticate_instance()

# Create an authenticated client that can interact with the Box Content API
client = Client(auth)

# Fetch the name and login (email address) of the JWT app Service Account
service_account = client.user().get()
print ('Service Account name:  ' + service_account.name)
print ('Service Account login: ' + service_account.login)
```

#### Output

```
Service Account name:  Server token test
Service Account login: AutomationUser_269418_WruhHJk0rO@boxdevedition.com
```