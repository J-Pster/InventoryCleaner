using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using System;
using Rust;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Reflection;
using Object = System.Object;
using System.Text;
using System.Numerics;
using System.Runtime;
using Network;

namespace Oxide.Plugins
{
    [Info("Inventory Cleaner", "Joao Pster", "2.1.1")]
    [Description("Allows players to clear their own or another player's inventory.")]
    public class InventoryCleaner : RustPlugin
    {
        #region Fields

        private class MyPermissions
        {
            public const string Clear = "inventorycleaner.allowed";
            public const string ClearOthers = "inventorycleaner.cleaneveryone";
            public const string ClearOnDeath = "inventorycleaner.cleanondeath";
            public const string ClearOnLogout = "inventorycleaner.cleanonexit";
        }

        private readonly string[] _permissions = GetAllPublicConstantValues<string>(typeof(MyPermissions)).ToArray();

        private static Configuration _config;

        #endregion

        #region Configuration

        private class Configuration
        {
            [JsonProperty(PropertyName = "[Message Image]")]
            public ulong MessageImage { get; set; } = 0;

            [JsonProperty(PropertyName = "[Message Prefix]")]
            public string MessagePrefix { get; set; } = "[Clear Inventory]";
        }

        private Configuration DefaultConfig() => new Configuration();

        protected override void LoadDefaultConfig()
        {
            PrintWarning("We are creating a new configuration file!");
            _config = DefaultConfig();
        }

        protected override void SaveConfig() => Config.WriteObject(_config, true);

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _config = Config.ReadObject<Configuration>();
                if (_config == null)
                {
                    LoadDefaultConfig();
                }
            }
            catch (Exception err)
            {
                PrintWarning("Failed to load the config file!");
                PrintWarning("Please, remove your config from config folder and reload plugin.");
                PrintError(err.ToString());
                return;
            }

            SaveConfig();
        }

        #endregion

        #region Helper Functions

        // This is to convert the class MyPermissions into an array of permissions.
        private static List<T> GetAllPublicConstantValues<T>(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
                .Select(x => (T)x.GetRawConstantValue())
                .ToList();
        }

        private string GenerateMessage(string message, string color = "#ffffff", int size = 14, bool italic = false)
        {
            var text = italic ? $"<size={size}>{_config.MessagePrefix} <color={color}><i>{message}</i></color></size>" : $"<size={size}>{_config.MessagePrefix} <color={color}>{message}</color></size>";
            return text;
        }

        private bool HasPermission(BasePlayer player, string permissionName, bool send = true)
        {
            if (player == null) return false;

            var has = permission.UserHasPermission(player.UserIDString, permissionName);
            if (!has && send)
            {
                string message = GenerateMessage(GetMessage(MessageKey.NoPermission, player.UserIDString, permissionName), "#FFFFFF", 14, false);
                SendChatMessage(player, message);
            }

            return has;
        }

        private void SendConsoleMessage(BasePlayer player, string message)
        {
            player.ConsoleMessage(message);
        }

        private void SendChatMessage(BasePlayer player, string message)
        {
            Player.Message(player, message, _config.MessageImage);
        }

        private void ReplyPlayer(IPlayer player, string message)
        {
            player.Reply(message);
        }

        private void SendMessageToAll(string message)
        {
            Server.Broadcast(message, _config.MessageImage);
        }
        
        private bool[] hasAllPermissions(IPlayer iplayer)
        {
            List<bool> bools = new List<bool>();

            foreach (var perm in _permissions)
            {
                bools.Add(iplayer.HasPermission(perm));
            }

            return bools.ToArray();
        }

        #endregion

        #region Permissions

        private string[] CheckPermissions(string[] permArray)
        {
            var toPerm = new List<string>();

            foreach (var perm in permArray)
            {
                if (!permission.PermissionExists(perm)) toPerm.Add(perm);
            }

            return toPerm.ToArray();
        }

        private bool IsAllowed(BasePlayer player, string perm)
        {
            if (permission.UserHasPermission(player.UserIDString, perm)) return true;


            return false;
        }

        private void LoadPermissions()
        {
            var toPerm = CheckPermissions(_permissions);
            if (toPerm.Length == 0) return;
            
            PrintWarning("Registering permissions...");
            foreach (var perm in toPerm)
            {
                permission.RegisterPermission(perm, this);
            }
        }

        #endregion

        #region Initialization and Saving

        private void Init()
        {
            LoadPermissions();

            // Register chat/console command under many different aliases
            // When running from chat, prefix with a forward slash /

            AddCovalenceCommand(new string[] {"clearinv", "cleaninv", "clear.inv", "clean.inv", "inv.clear", "invclear", "inv.clean", "invclean"}, nameof(ClearCovalenceCommand), MyPermissions.Clear); 
        }

        #endregion

        #region Clear Hooks

        object OnPlayerDeath(BasePlayer player, HitInfo info)
        {
            if (!HasPermission(player, MyPermissions.ClearOnDeath, false)) return null;

            player.inventory.Strip();
            string msg = GenerateMessage(GetMessage(MessageKey.OnDeath, player.UserIDString, player.displayName), "green", 14);
            SendChatMessage(player, msg);

            return null;
        }

        void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player.IsDead()) return;
            if (!HasPermission(player, MyPermissions.ClearOnLogout, false)) return;

            player.inventory.Strip();
        }

        #endregion
        
        #region Panel Functions
        
        private void CommandsPanel(IPlayer iplayer, params string[] args)
        {
            var sb = new StringBuilder();
            sb.Append(GetMessage(MessageKey.Header, iplayer.Id, Author, Version));
            sb.Append(GetMessage(MessageKey.Gone, iplayer.Id));
            sb.Append(GetMessage(MessageKey.Opts, iplayer.Id));
            sb.Append(GetMessage(MessageKey.OptAll, iplayer.Id));
            sb.Append(GetMessage(MessageKey.OptInv, iplayer.Id));
            sb.Append(GetMessage(MessageKey.OptBelt, iplayer.Id));
            sb.Append(GetMessage(MessageKey.OptWear, iplayer.Id));
            sb.Append(GetMessage(MessageKey.OptEvery, iplayer.Id));

            ReplyPlayer(iplayer, sb.ToString());
        }

        private void HelpPanel(IPlayer iplayer)
        {
            var mCp = hasAllPermissions(iplayer);

            var sb = new StringBuilder();
            sb.Append(GetMessage(MessageKey.Header, iplayer.Id, Author, Version));
            sb.Append(GetMessage(MessageKey.Gone, iplayer.Id));
            sb.Append(GetMessage(MessageKey.Perms, iplayer.Id, iplayer.Name));
            sb.Append(GetMessage(MessageKey.PermUse, iplayer.Id, mCp[0]));
            sb.Append(GetMessage(MessageKey.PermEvery, iplayer.Id, mCp[1]));
            sb.Append(GetMessage(MessageKey.PermDeath, iplayer.Id, mCp[2]));
            sb.Append(GetMessage(MessageKey.PermLogout, iplayer.Id, mCp[3]));
            sb.Append(GetMessage(MessageKey.InvComands, iplayer.Id));

            ReplyPlayer(iplayer, sb.ToString());
        }
        
        #endregion

        #region Clear Functions
        
        private void DeleteFromEveryone(IPlayer iplayer, string opt)
        {
            PrintWarning($"{iplayer.Name} is trying to run Delete Everyone!");
            if (!iplayer.HasPermission(MyPermissions.ClearOthers)) return;
            PrintWarning($"{iplayer.Name}: Running Delete Everyone Started!");

            List<BasePlayer> players = BasePlayer.allPlayerList.ToList();

            foreach (var p in players)
            {
                var inv = p.inventory;
                switch (opt)
                {
                    case "main":
                        inv.Strip();
                        SendMessageToAll(GenerateMessage(GetMessage(MessageKey.EveryAllCleaned, iplayer.Id), "red", 18));
                        break;
                    case "inv":
                        inv.containerMain.Clear();
                        SendMessageToAll(GenerateMessage(GetMessage(MessageKey.EveryInvCleaned, iplayer.Id), "red", 18));
                        break;
                    case "belt":
                        inv.containerBelt.Clear();
                        SendMessageToAll(GenerateMessage(GetMessage(MessageKey.EveryBeltCleaned, iplayer.Id), "red", 18));
                        break;
                    case "wear":
                        inv.containerWear.Clear();
                        SendMessageToAll(GenerateMessage(GetMessage(MessageKey.EveryWearCleaned, iplayer.Id), "red", 18));
                        break;
                    default:
                        var msg = GenerateMessage(GetMessage(MessageKey.OptNotFound, iplayer.Id), "red", 14);
                        ReplyPlayer(iplayer, msg);
                        break;
                }
            }

            PrintWarning($"{iplayer.Name}: Running Delete Everyone Finished!");
        }

        private void ClearOneContainer(IPlayer iplayer, ItemContainer container, string msgKey, string option = "main", bool every = false)
        {
            // Everyone
            if (every)
            {
                DeleteFromEveryone(iplayer, option);
                return;
            }
            
            // Singular
            container.Clear();
            ItemManager.DoRemoves();
            
            var msg = GenerateMessage(GetMessage(msgKey, iplayer.Id, iplayer.Name), "green", 14);
            ReplyPlayer(iplayer, msg);
        }

        private void ClearAllContainers(IPlayer iplayer, string msgKey, string option = "main", bool every = false)
        {
            // Everyone
            if (every)
            {
                DeleteFromEveryone(iplayer, option);
                return;
            }

            // Singular
            var player = iplayer.Object as BasePlayer;
            player.inventory.Strip();
            ItemManager.DoRemoves();

            var msg = GenerateMessage(GetMessage(msgKey, iplayer.Id, iplayer.Name), "green", 14);
            ReplyPlayer(iplayer, msg);
        }

        #endregion

        #region Chat and/or Console Commands
        private void ClearCovalenceCommand(IPlayer iplayer, string command, string[] args)
        {
            // This is a client-oriented command, so no need to go further if ran from console
            if (iplayer.IsServer)
            {
                return;
            }

            // Get BasePlayer from IPlayer
            var player = iplayer.Object as BasePlayer;

            if (!iplayer.HasPermission(MyPermissions.Clear))
            {
                return;
            }

            if (args.Length == 0)
            {
                // Default Case
                ClearAllContainers(iplayer, MessageKey.AllCleaned);
                return;
            }

            var every = ((args.Length > 1) && (args[1].ToLower() == "everyone"));

            var opt = args[0].ToLower();

            switch (opt)
            {
                case "main":
                    ClearAllContainers(iplayer, MessageKey.AllCleaned, opt, every);
                    break;
                case "inv":
                    ClearOneContainer(iplayer, player.inventory.containerMain, MessageKey.InvCleaned, opt, every);
                    break;
                case "belt":
                    ClearOneContainer(iplayer, player.inventory.containerBelt, MessageKey.InvCleaned, opt, every);
                    break;
                case "wear":
                    ClearOneContainer(iplayer, player.inventory.containerWear, MessageKey.InvCleaned, opt, every);
                    break;
                case "help":
                    HelpPanel(iplayer);
                    break;
                case "cmds":
                    CommandsPanel(iplayer);
                    break;
                default:
                    ReplyPlayer(iplayer, GenerateMessage(GetMessage(MessageKey.OptNotFound, iplayer.Id, opt)));
                    break;
            }
        }
        #endregion

        #region Localization 

        private static class MessageKey
        {
            public const string NoPermission = "[No Permission]";
            public const string NotFound = "[Not Found]";
            public const string OptNotFound = "[Option Not Found]";
            public const string CorrectUse = "[Correct Use]";
            public const string BeltCleaned = "[Belt Cleaned]";
            public const string EveryBeltCleaned = "[Every Belt Cleaned]";
            public const string InvCleaned = "[Inventory Cleaned]";
            public const string EveryInvCleaned = "InventoryCleaner.EveryInvCleaned";
            public const string WearCleaned = "[Wear Cleaned]";
            public const string EveryWearCleaned = "InventoryCleaner.EveryWearCleaned";
            public const string AllCleaned = "[All Cleaned]";
            public const string EveryAllCleaned = "InventoryCleaner.EveryAllCleaned";
            public const string OnDeath = "[On Death]";
            public const string Header = "[Interface Header]";
            public const string Gone = "[Interface Gome]";
            public const string Opts = "[Interface Options]";
            public const string Perms = "[Interface Perms]";
            public const string OptAll = "[Interface Opt All]";
            public const string OptInv = "[Interface Opt Inv]";
            public const string OptBelt = "[Interface Opt Belt]";
            public const string OptWear = "[Interface Opt Wear]";
            public const string OptEvery = "[Interface Opt Every]";
            public const string PermUse = "[Interface Perm Use]";
            public const string PermEvery = "[Interface Perm Every]";
            public const string PermDeath = "[Interface Perm Death]";
            public const string PermLogout = "[Interface Perm Logout]";
            public const string InvComands = "[Interface Comands]";
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                [MessageKey.NoPermission] = "You don't have the permission <color=#FF0000>{0}</color> to do that!",
                [MessageKey.NotFound] = "Command <color=red>{0}</color> not found!",
                [MessageKey.OptNotFound] = "Option <color=red>/clearinv {0}</color> not found!",
                [MessageKey.CorrectUse] = "The correct use is: <color=green>/clearinv [command]</color>",
                [MessageKey.BeltCleaned] = "{0}, your belt has just been cleaned!",
                [MessageKey.EveryBeltCleaned] = "The Belt of all players logged into the server has just been removed!",
                [MessageKey.InvCleaned] = "{0}, your inventory has just been cleaned!",
                [MessageKey.EveryInvCleaned] = "The Inventory of all players logged into the server has just been removed!",
                [MessageKey.WearCleaned] = "{0}, your clothing slots has just been cleaned!",
                [MessageKey.EveryWearCleaned] = "The Clothing Slots of all players logged into the server has just been removed!",
                [MessageKey.AllCleaned] = "{0}, everything you have has just been cleaned!",
                [MessageKey.EveryAllCleaned] = "All Items of all players logged into the server has just been removed!",
                [MessageKey.OnDeath] = "{0}, you died and everything you had was deleted before your death!",
                [MessageKey.Header] = "<size=16><color=green>Clear Inventory by {0}</color></size> v{1} \n",
                [MessageKey.Gone] = "<color=#ff0000>Warning:</color> Once items removed they are GONE ! \n\n",
                [MessageKey.Opts] = "Hi, the base commands is <color=green>/clearinv [opts]</color>, see the opts:\n\n",
                [MessageKey.Perms] = "Hi <color=green>{0}</color>, this is your permissions: \n",
                [MessageKey.OptAll] = "<color=yellow>main</color>: remove all your items \n",
                [MessageKey.OptInv] = "<color=yellow>inv</color>: remove all items from your inventory \n",
                [MessageKey.OptBelt] = "<color=yellow>belt</color>: remove all items from your belt \n",
                [MessageKey.OptWear] = "<color=yellow>wear</color>: remove all items from your clothing slots \n\n",
                [MessageKey.OptEvery] = "And, if you have permission, you can do <color=red>/clearinv [opts] everyone</color> to remove the items from everyone who is logged on to the server!",
                [MessageKey.PermUse] = "<color=yellow>Use Clear:</color> {0} \n",
                [MessageKey.PermEvery] = "<color=yellow>Clear Everyone:</color> {0} \n",
                [MessageKey.PermDeath] = "<color=yellow>Clear on Death:</color> {0} \n",
                [MessageKey.PermLogout] = "<color=yellow>Clear on logout:</color> {0} \n\n",
                [MessageKey.InvComands] = "Use <color=green>/clearinv cmds</color> to see the comands.",
            }, this, "en"); ; ;
        }

        private string GetMessage(string messageKey, string playerId = null, params object[] args)
        {
            try
            {
                return string.Format(lang.GetMessage(messageKey, this, playerId), args);
            }
            catch (Exception err)
            {
                PrintError($"Error on Get Message from Lang: {err.Message}");
                throw;
            }
        }

        #endregion

    }
}
