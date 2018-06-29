using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Box.V2;
using Box.V2.Config;
using Box.V2.JWTAuth;
using Box.V2.Models;

namespace csharp_netcore
{
    class Program
    {
        static void Main(string[] args)
        {
            /** Configuration Items **/

            // Set the path to your JWT app config JSON file here!
            var pathToConfigJson = "";

            // Set the path to a folder you'd like to traverse here!
            var folderId = "0";


            /** Main **/

            // Get a Box client authenticated as the JWT app service account.
            BoxClient adminClient = GetAuthenticatedClient(pathToConfigJson);

            // Print the name and login of the JWT app service account.
            PrintServiceAccountInformation(adminClient);

            // Print the folder and file listing for the provided Box folder ID.
            PrintFolderTree(adminClient, folderId);

            Console.ReadLine();
        }

        /// <summary>
        /// Get an authenticated client for the JWT app service account.
        /// </summary>
        /// <param name="pathToConfigJson">The path to the JWT app configuration JSON file.</param>
        /// <returns>An authenticated Box client</returns>
        private static BoxClient GetAuthenticatedClient(string pathToConfigJson)
        {
            if (string.IsNullOrWhiteSpace(pathToConfigJson) || !File.Exists(pathToConfigJson))
                throw new Exception("Please set 'pathToConfigJson' with the path to your JWT app config JSON file.");

            // Read the configuration from the file.
            IBoxConfig config;
            using (var configStream = File.OpenRead(pathToConfigJson))
                config = BoxConfig.CreateFromJsonFile(configStream);

            // Create a Box client and authenticate as the service account
            var boxJwtAuth = new BoxJWTAuth(config);
            var adminToken = boxJwtAuth.AdminToken();
            return boxJwtAuth.AdminClient(adminToken);
        }

        /// <summary>
        /// Print inforamation about the currently authenticated Box user
        /// </summary>
        /// <param name="client">An authenticated Box client</param>
        private static void PrintServiceAccountInformation(Box.V2.BoxClient client)
        {
            var user = client.UsersManager.GetCurrentUserInformationAsync().Result;
            Console.Out.WriteLine("");
            Console.Out.WriteLine("Authenticated User");
            Console.Out.WriteLine($"Name: {user.Name}");
            Console.Out.WriteLine($"Login: {user.Login}");
        }

        /// <summary>
        /// List the Box folder tree starting at the specified folder ID
        /// </summary>
        /// <param name="client">An authenticated Box client</param>
        /// <param name="folderId">The Box folder ID at which to start the listing</param>
        private static void PrintFolderTree(BoxClient client, string folderId)
        {
            Console.Out.WriteLine("");
            Console.Out.WriteLine("File Listing");
            Console.Out.WriteLine("          ID Path");
            var folder = client.FoldersManager.GetInformationAsync(folderId).Result;
            WalkFolderTree(client, folder, ListPath);
        }

        /// <summary>
        /// Walk a Box folder tree, applying the specified action to every file and folder encountered.
        /// </summary>
        /// <param name="client">An authenticated Box client</param>
        /// <param name="folder">The Box folder to traverse</param>
        /// <param name="action">An action to apply to the Box files and folders</param>
        /// <param name="fields">(Optional) A comma-separated list of Box file/folder fields to fetch with every folder item. Default: name, id, path_collection.
        private static void WalkFolderTree(BoxClient client, BoxFolder folder, Action<BoxItem> action, string fields = "name, id, path_collection" )
        {
            // Apply the action to this folder
            action(folder);

            // Get all items of this folder.
            var fieldList = fields.Split(",").Select(f => f.Trim());
            var items = client.FoldersManager
                .GetFolderItemsAsync(folder.Id, 500, fields:fieldList, autoPaginate:true)
                .Result.Entries;

            // Recurse to each subfolder
            foreach (var subfolder in items.Where(i => i.Type == "folder"))
                WalkFolderTree(client, (BoxFolder)subfolder, action);

            // Print each item in this folder
            foreach (var file in items.Where(i => i.Type == "file"))
                action(file);
        }

        /// <summary>
        /// Pretty-print the ID and path of a Box item.
        /// </summary>
        /// <param name="item">The Box item</param>
        private static void ListPath(BoxItem item)
        {
            var id = item.Id.PadLeft(12);
            var parents = item.PathCollection.Entries.Select(e => e.Name);
            var path = $"{string.Join("/", parents)}/{item.Name}".TrimStart('/');
            Console.WriteLine($"{id} /{path}");
        }           
    }
}