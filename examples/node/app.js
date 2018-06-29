var BoxSDK = require('box-node-sdk');

/** Configuration **/

var pathToConfigJson = "/Users/jhoerr/Downloads/62618999_02mlnyl7_config.json";

var folderId = "0";


/** Functions **/

/**
 * Get an authenticated Box client for a JWT app service account.
 *
 * @param {string} configPath The path to the JWT app config JSON file
 * @returns {BoxClient} An authenticated Box client
 */
let getAuthenticatedClient = (configPath) => {
    var jsonConfig = require(configPath);
    var sdk = BoxSDK.getPreconfiguredInstance(jsonConfig);
    console.log("Authenticating...")
    return sdk.getAppAuthClient('enterprise');
}

/**
 * Print the name and login of the current authenticated Box user
 *
 * @param {BoxClient} client An authenticated Box client
 */
const printCurrentUserInfo = async (client) =>
    await client.users.get(client.CURRENT_USER_ID)
    .then(user => {
        console.log("\nAuthenticated as")
        console.log('  Name:', user.name)
        console.log('  Login:', user.login)
    })
    .catch(err => console.log('Got an error!', err));

const folderOptions = { 
    fields: "name,id,path_collection"
}

/**
 * Pretty-print the path and ID of a Box file/folder item.
 *
 * @param {BoxItem} item The Box item to print
 */
const printPathAndId = (item) => {
    const id = item.id.padStart(12);
    const path = item.path_collection.entries
        .map (e => e.name)        
        .join ("/")               
        .concat(`/${item.name}`)
        .replace(/^\//g, ''); // trim leading '/'
    console.log(`${id} /${path}`)
}

/**
 * Fetch all items in a folder accounting for paging.
 *
 * @param {BoxFolder} folder The Box folder whose contents we wish to fetch.
 * @returns {Promise<Array>} A collection of BoxItems
 */
const getAllFolderItems = async (folder) => {
    let results = [];
    let offset = 0;
    let lastFetchedCount = 0
    do {
        await client.folders.getItems(folder.id, {
            fields: folderOptions.fields,
            limit: 500,
            offset: offset
        })
        .then(items => {
            results.push(...items.entries);
            lastFetchedCount = items.entries.length;
            offset += lastFetchedCount;
        });
    } while (lastFetchedCount !== 0)
    return results;
}

/**
 * Traverse a Box folder tree, applying the specified action to every file and folder. 
 *
 * @param {BoxClient} client An authenticated Box client
 * @param {BoxFolder} folder The Box folder to traverse
 * @param {function(BoxItem):void} action The action to apply
 */
const walkFolderTree = async (client, folder, action) => {
    const folders = (items) => items.filter(i => i.type === "folder");
    const files = (items) => items.filter(i => i.type === "file");
    
    // fetch all items in this folder
    const items = await getAllFolderItems(folder)

    // apply the action to this folder
    action(folder);

    // recurse to every subfolder
    for (let f of folders(items)){
        await walkFolderTree(client, f, action);    
    }

    // apply the action to each file
    for (let f of files(items)){
        action(f)
    }
}

/**
 * Print the contents of a Box folder tree.
 *
 * @param {BoxClient} client An authenticated Box client
 * @param {string} folderId The ID of the Box folder to traverse.
 */
const printFolderTree = async (client, folderId) => {
    await client.folders.get(folderId, folderOptions)
    .then(folder => {
        console.log("\nFile Listing");
        console.log("          ID Path");
        walkFolderTree(client, folder, printPathAndId);
    })
    .catch(err => console.log('Got an error!', err));    
}

/** Main **/

const client = getAuthenticatedClient(pathToConfigJson);
printCurrentUserInfo(client)
.then(printFolderTree(client, folderId))