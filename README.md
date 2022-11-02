## What does it do?
With this plugin you can delete your own inventory, or parts of it, like `belt, wear, inv`, but you can also delete the inventory of all players logged into the server.

Your inventory can also be deleted when you log out or when you die, if you have permission to do so. 

## Permissions
* `inventorycleaner.allowed` - To use the `/clearinv` command.
* `inventorycleaner.cleaneveryone` - Perm to use the options to clean the inventory of all players.
* `inventorycleaner.cleanondeath` - To clear your inventory on death.
* `inventorycleaner.cleanonexit` - To clear your inventory on logout.

## Commands
The base command is `/clearinv`, if you type the base command, your inventory will be cleared.

If you type `/clearinv help`, you will access the help menu, with all your permissions.

![Info Panel](https://i.imgur.com/49xK4oA.png)

### Command Options
To use the options, type the base command `/clearinv [opts]` and use one option.
* `main` - remove all your items
* `inv` - remove all items from your inventory
* `belt` - remove all items from your belt
* `wear` - remove all items from your clothing sloots

**WARNING:** To remove items from all connected players, use: `/clearinv [opt] everyone` , you need to have permission to do this.

If you type `/clearinv cmds`, you will access the commands menu, with all your commands.

![Command Panel](https://i.imgur.com/9lWFaZo.png)

### Console Commands
To use console commands, type the base command `inv.clear` in **F1** and use one option.
* `main` - remove all your items
* `inv` - remove all items from your inventory
* `belt` - remove all items from your belt
* `wear` - remove all items from your clothing sloots

**WARNING:** To remove items from all connected players, use: `inv.clear [opt] everyone` , you need to have permission to do this.

**ATENTION: So, both, console and chat commands have the same Syntax, only the base command changes.**

## Localization
You can translate **all the messages** that the plugin is able to send to the chat.

```json
{
  "[No Permission]": "You don't have the permission <color=#FF0000>{0}</color> to do that!",
  "[Not Found]": "Command <color=red>{0}</color> not found!",
  "[Option Not Found]": "Option <color=red>/clearinv {0}</color> not found!",
  "[Correct Use]": "The correct use is: <color=green>/clearinv [command]</color>",
  "[Belt Cleaned]": "{0}, your belt has just been cleaned!",
  "[Every Belt Cleaned]": "The Belt of all players logged into the server has just been removed!",
  "[Inventory Cleaned]": "{0}, your inventory has just been cleaned!",
  "InventoryCleaner.EveryInvCleaned": "The Inventory of all players logged into the server has just been removed!",
  "[Wear Cleaned]": "{0}, your clothing slots has just been cleaned!",
  "InventoryCleaner.EveryWearCleaned": "The Clothing Slots of all players logged into the server has just been removed!",
  "[All Cleaned]": "{0}, everything you have has just been cleaned!",
  "InventoryCleaner.EveryAllCleaned": "All Items of all players logged into the server has just been removed!",
  "[On Death]": "{0}, you died and everything you had was deleted before your death!",
  "[Interface Header]": "<size=16><color=green>Clear Inventory by {0}</color></size> v{1} \n",
  "[Interface Gome]": "<color=#ff0000>Warning:</color> Once items removed they are GONE ! \n\n",
  "[Interface Options]": "Hi, the base commands is <color=green>/clearinv [opts]</color>, see the opts:\n\n",
  "[Interface Perms]": "Hi <color=green>{0}</color>, this is your permissions: \n",
  "[Interface Opt All]": "<color=yellow>main</color>: remove all your items \n",
  "[Interface Opt Inv]": "<color=yellow>inv</color>: remove all items from your inventory \n",
  "[Interface Opt Belt]": "<color=yellow>belt</color>: remove all items from your belt \n",
  "[Interface Opt Wear]": "<color=yellow>wear</color>: remove all items from your clothing slots \n\n",
  "[Interface Opt Every]": "And, if you have permission, you can do <color=red>/clearinv [opts] everyone</color> to remove the items from everyone who is logged on to the server!",
  "[Interface Perm Use]": "<color=yellow>Use Clear:</color> {0} \n",
  "[Interface Perm Every]": "<color=yellow>Clear Everyone:</color> {0} \n",
  "[Interface Perm Death]": "<color=yellow>Clear on Death:</color> {0} \n",
  "[Interface Perm Logout]": "<color=yellow>Clear on logout:</color> {0} \n\n",
  "[Interface Comands]": "Use <color=green>/clearinv cmds</color> to see the comands."
}
```

## Contacts
Follow my on Linkedin: [Click here!](https://www.linkedin.com/)

## Credits
To [TheDoc](https://umod.org/plugins/admin-inventory-cleaner) and [misticos](https://umod.org/plugins/inventory-cleaner) for other plugins similar to this one, which inspired me to make this one.
