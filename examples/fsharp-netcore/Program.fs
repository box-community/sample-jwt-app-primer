// Learn more about F# at http://fsharp.org

open System
open System.IO
open Box.V2.JWTAuth
open Box.V2.Config
open Box.V2.Models
open Box.V2

// ** Configuration Items **

// Set the path to your JWT app config JSON file here!
let pathToConfigJson = "/Users/jhoerr/Downloads/62618999_02mlnyl7_config.json";

// Set the path to a folder you'd like to traverse here!
let folderId = "0";


// ** Functions **

// Raise an exception if the JWT app JSON config file can't be found.
let assertConfigFileExists pathToConfigJson = 
    if String.IsNullOrWhiteSpace(pathToConfigJson) 
        || (not (File.Exists(pathToConfigJson)))
    then 
        Exception("Please set 'pathToConfigJson' with the path to your JWT app config JSON file.")
        |> raise
    pathToConfigJson

/// Get an authenticated client for the JWT app service account.
/// <param name="pathToConfigJson">The path to the JWT app configuration JSON file.</param>
/// <returns>An authenticated Box client</returns>
let getAuthenticatedClient pathToConfigJson =
    use stream = 
        pathToConfigJson
        |> assertConfigFileExists
        |> File.OpenRead

    printfn "Authenticating..."
    stream
    |> BoxConfig.CreateFromJsonFile
    |> BoxJWTAuth
    |> (fun auth -> auth.AdminToken() |> auth.AdminClient)

// Synchronously return the result of some async Task<'T>
let awaitResult f =
    f 
    |> Async.AwaitTask 
    |> Async.RunSynchronously

// Print the name and login for the currently authenticated Box user
let printServiceAccountInformation (client:BoxClient) =
    let user = 
        client.UsersManager.GetCurrentUserInformationAsync [] 
        |> awaitResult

    printfn """
Authenticated as:
  Name: %s
  Login: %s""" user.Name user.Login

// Print the ID and path of a Box item.
let listPath (item:BoxItem) =
    let id = item.Id.PadLeft(12)
    let path = 
        item.PathCollection.Entries
        |> Seq.map (fun e -> e.Name)
        |> String.concat "/"
        |> fun folders -> sprintf "%s/%s" folders (item.Name)
        |> fun str -> str.TrimStart('/')
    printfn "%s /%s" id path

let rec walkFolderTree 
    (client: BoxClient) 
    (itemAction: BoxItem -> unit) 
    (folder: BoxFolder) =
    
    // Fetch all the items in this folder
    let fields = ["name"; "id"; "path_collection"]   
    let items = 
        client.FoldersManager.GetFolderItemsAsync((folder.Id), 500, 0, fields, true)
        |> awaitResult

    // apply the action to this folder
    itemAction folder
      
    // recurse to every subfolder of this folder
    items.Entries
    |> Seq.filter (fun i -> i.Type = "folder")
    |> Seq.cast<BoxFolder>
    |> Seq.iter (walkFolderTree client itemAction)

    // apply the action to every file in this folder
    items.Entries
    |> Seq.filter (fun i -> i.Type = "file")
    |> Seq.iter itemAction
   
// Print the complete file and folder listing for the specified folderId
let printFolderTree folderId (client:BoxClient) = 
    printfn """
File Listing:

          ID Path"""

    folderId 
    |> client.FoldersManager.GetInformationAsync
    |> awaitResult
    |> walkFolderTree client listPath

[<EntryPoint>]
let main argv =
    let client = getAuthenticatedClient pathToConfigJson
    client |> printServiceAccountInformation
    client |> printFolderTree folderId
    0