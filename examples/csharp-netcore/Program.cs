using System;
using System.IO;
using System.Linq;
using Box.V2;
using Box.V2.Config;
using Box.V2.JWTAuth;

namespace net_csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            /** Configuration Items **/

            // Set the path to your JWT app config JSON file here!
            var pathToConfigJson = "/Users/jhoerr/Downloads/62618999_02mlnyl7_config.json";

            // Set the path to a folder you'd like to traverse here!
            var folderId = "0";

            if (string.IsNullOrWhiteSpace(pathToConfigJson))
                throw new Exception("Please set 'pathToConfigJson' with the path to your JWT app config JSON file.");

            // Read the configuration from the file.
            IBoxConfig config;
            using (var configStream = File.OpenRead(pathToConfigJson))
                config = BoxConfig.CreateFromJsonFile(configStream);

            // Create a Box client and authenticate as the service account
            var boxJwtAuth = new BoxJWTAuth(config);
            var adminToken = boxJwtAuth.AdminToken();
            var adminClient = boxJwtAuth.AdminClient(adminToken);

            Console.WriteLine("Service Account Information:");
            PrintServiceAccountInformation(adminClient);
         
            Console.WriteLine("Folder Tree:");
            PrintFolderTree(adminClient, folderId);

            Console.ReadLine();
        }
        private static void PrintServiceAccountInformation(Box.V2.BoxClient adminClient)
        {
            var serviceAccount = adminClient.UsersManager.GetCurrentUserInformationAsync().Result;
            Console.Out.WriteLine($"Name: {serviceAccount.Name}");
            Console.Out.WriteLine($"Login: {serviceAccount.Login}");
            Console.Out.WriteLine("");
        }

        private static void PrintFolderTree(BoxClient adminClient, string folderId, int indent = 0)
        {
            var folder = adminClient.FoldersManager.GetInformationAsync(folderId).Result;
            var items = adminClient.FoldersManager.GetFolderItemsAsync(folderId, 1000).Result;

            // Print this folder
            List($"/{folder.Name} ({folder.Id})", indent);

            // Recurse to each subfolder
            foreach (var subfolder in items.Entries.Where(i => i.Type == "folder"))
                PrintFolderTree(adminClient, subfolder.Id, indent + 2);

            // Print each item in this folder
            foreach (var file in items.Entries.Where(i => i.Type == "file"))
                List($"{file.Name} (({file.Id})", indent+2);
        }

        private static void List(string v, int indent) => 
            Console.WriteLine($"{new string(' ', indent)}{v}");
    }
}
