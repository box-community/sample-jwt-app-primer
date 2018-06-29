from boxsdk import JWTAuth, Client
from boxsdk.object.folder import Folder
from pprint import pprint
import os.path

## Configuration ##

# Set the path to your JWT app config JSON file here!
pathToConfigJson = ""

# Set the path to a folder you'd like to traverse here!
folderId = "0"


## Functions ##

def get_authenticated_client(configPath):
    """Get an authenticated Box client for a JWT service account
    
    Arguments:
        configPath {str} -- Path to the JSON config file for your Box JWT app
    
    Returns:
        Client -- A Box client for the JWT service account

    Raises:
        ValueError -- if the configPath is empty or cannot be found.
    """
    if (os.path.isfile(configPath) == False):
        raise ValueError(f"configPath must be a path to the JSON config file for your Box JWT app")
    auth = JWTAuth.from_settings_file(configPath)
    print("Authenticating...")
    auth.authenticate_instance()
    return Client(auth)

def print_path(item):
    """Print the ID and path of a given Box file or folder."""
    item_id = item.id.rjust(12, ' ')
    parents = map(lambda p: p['name'], item.path_collection['entries'])
    path = f"{'/'.join(parents)}/{item.name}".strip('/')
    print(f"{item_id} /{path}")

def get_subitems(client, folder, fields = ["id","name","path_collection","size"]):
    """Get a collection of all immediate folder items
    
    Arguments:
        client {Client} -- An authenticated Box client
        folder {Folder} -- The Box folder whose contents we want to fetch
    
    Keyword Arguments:
        fields {list} -- An optional list of fields to include with each item (default: {["id","name","path_collection"]})
    
    Returns:
        list -- A collection of Box files and folders.
    """
    items = []
    offset = 0
    lastFetchedCount = -1
    while (lastFetchedCount != 0):
        # fetch folder items and add subfolders to list
        fetched = client.folder(folder_id=folder['id']).get_items(limit=1000, offset=offset, fields=fields)
        items.extend(fetched)
        # update offset and counts for terminating conditions.
        offset += len(fetched)
        lastFetchedCount = len(fetched)
    return items

def print_user_info(client):
    """Print the name and login of the current authenticated Box user
        
    Arguments:
        client {Client} -- An authenticated Box client
    """
    user = client.user('me').get()
    print("")
    print("Authenticated User")
    print(f"Name: {user.name}")
    print(f"Login: {user.login}")

def walk_folder_tree_rec(client, folder, action):
    """Traverse a Box folder tree, performing the specified action on every file and folder.
    
    Arguments:
        client {Client} -- An authenticated Box client.
        folder {Folder} -- The Box folder to traverse.
        action {Item: void} -- An action to perform on every Box file and folder.
    """
    action(folder)
    subitems = get_subitems(client, folder)
    for subfolder in filter(lambda i: i.type=="folder", subitems):
        walk_folder_tree_rec(client, subfolder, action)
    for file in filter(lambda i: i.type=="file", subitems):
        action(file)

def print_folder_tree(client, folderId="0"):
    """Print the contents of a Box folder tree
    
    Arguments:
        client {Client} -- An authenticated Box client
        folderId {str} -- The ID of the Box folder in which to start the listing. (default: "0" for the root folder)
    """
    print("")
    print("File Listing")
    print(f"{'ID'.ljust(12)} Path")
    root = client.folder(folder_id=folderId).get()
    walk_folder_tree_rec(client, root, print_path)


## Main ##

if __name__ == "__main__":
    # Get a client instance for the service account.
    client = get_authenticated_client(pathToConfigJson)
    # Print the name and login associated with the service account.
    print_user_info(client)
    # Print a file and folder listing
    print_folder_tree(client, folderId)