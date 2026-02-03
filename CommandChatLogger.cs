using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;

namespace Oxide.Plugins
{
    [Info("CommandChatLogger", "SeesAll", "1.3.0")]
    [Description("Logs chat /commands used by players, with exclusions, wipe cleanup, and flexible staff logging.")]
    public class CommandChatLogger : RustPlugin
    {
        #region Configuration

        private PluginConfig config;

        private class PluginConfig
        {
            [JsonProperty(PropertyName = "Plugin Enabled")]
            public bool Enabled = true;

            // PlayersOnly = only non-admins/mods
            // AdminsOnly  = only admins/mods
            // Everyone    = both
            [JsonProperty(PropertyName = "Log Mode (PlayersOnly/AdminsOnly/Everyone)")]
            public string LogMode = "PlayersOnly";

            [JsonProperty(PropertyName = "Wipe Data File On Server Wipe")]
            public bool WipeDataOnServerWipe = true;

            [JsonProperty(PropertyName = "Ignored Commands (without /)")]
            public List<string> IgnoredCommands = new List<string>();
        }

        private static readonly List<string> DefaultIgnoredCommands = new List<string>
        {
            "mymini",
            "nomini",
            "fmini"
        };

        protected override void LoadDefaultConfig()
        {
            config = new PluginConfig
            {
                IgnoredCommands = new List<string>(DefaultIgnoredCommands),
                LogMode = "PlayersOnly",
                Enabled = true,
                WipeDataOnServerWipe = true
            };
            SaveConfig();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                config = Config.ReadObject<PluginConfig>();
                if (config == null)
                    throw new Exception("Config is null");

                // Ensure list exists
                if (config.IgnoredCommands == null)
                    config.IgnoredCommands = new List<string>();

                // If it's empty, seed defaults
                if (config.IgnoredCommands.Count == 0)
                {
                    config.IgnoredCommands = new List<string>(DefaultIgnoredCommands);
                }
                else
                {
                    // Deduplicate (case-insensitive) and trim
                    config.IgnoredCommands = config.IgnoredCommands
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Select(c => c.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }

                // Normalize LogMode
                if (string.IsNullOrWhiteSpace(config.LogMode))
                {
                    config.LogMode = "PlayersOnly";
                }
                else
                {
                    var mode = config.LogMode.Trim();
                    // Only allow these three, fallback to PlayersOnly if invalid
                    if (!string.Equals(mode, "PlayersOnly", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(mode, "AdminsOnly", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(mode, "Everyone", StringComparison.OrdinalIgnoreCase))
                    {
                        PrintWarning($"Invalid LogMode '{config.LogMode}' in config, defaulting to 'PlayersOnly'.");
                        config.LogMode = "PlayersOnly";
                    }
                    else
                    {
                        // Keep trimmed version
                        config.LogMode = mode;
                    }
                }

                SaveConfig(); // ensure cleaned config is written back
            }
            catch (Exception e)
            {
                PrintError($"Error loading config, using default. ({e.Message})");
                LoadDefaultConfig();
            }
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(config, true);
        }

        #endregion

        #region Data

        private const string DataFileName = "CommandChatLogger";
        private List<CommandLogEntry> _entries = new List<CommandLogEntry>();

        private class CommandLogEntry
        {
            public string Timestamp;
            public string PlayerName;
            public ulong SteamId;
            public string Command;
            public string FullMessage;
        }

        private void LoadData()
        {
            try
            {
                _entries = Interface.Oxide.DataFileSystem.ReadObject<List<CommandLogEntry>>(DataFileName);
                if (_entries == null)
                    _entries = new List<CommandLogEntry>();
            }
            catch
            {
                _entries = new List<CommandLogEntry>();
                SaveData();
            }
        }

        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject(DataFileName, _entries);
        }

        private void WipeData()
        {
            _entries.Clear();
            SaveData();
        }

        #endregion

        #region Hooks

        private void Init()
        {
            LoadConfig();
            LoadData();
        }

        /// <summary>
        /// Called by Rust when a new save (wipe) is created
        /// </summary>
        private void OnNewSave()
        {
            if (!config.WipeDataOnServerWipe)
                return;

            WipeData();
            Puts("Server wipe detected — CommandChatLogger data file wiped.");
        }

        private void OnPlayerChat(BasePlayer player, string message, ConVar.Chat.ChatChannel channel)
        {
            if (player == null || string.IsNullOrEmpty(message))
                return;

            if (!config.Enabled)
                return;

            if (!message.StartsWith("/"))
                return;

            if (player.net?.connection == null)
                return;

            var authLevel = player.net.connection.authLevel;
            bool isAdminOrMod = authLevel >= 1; // 1 = moderator, 2 = owner

            // Apply log mode
            switch (config.LogMode.ToLowerInvariant())
            {
                case "playersonly":
                    if (isAdminOrMod)
                        return;
                    break;

                case "adminsonly":
                    if (!isAdminOrMod)
                        return;
                    break;

                case "everyone":
                    // log both
                    break;

                default:
                    // Fallback safety: treat unknown as PlayersOnly
                    if (isAdminOrMod)
                        return;
                    break;
            }

            string stripped = message.Substring(1);
            string command = stripped;
            int spaceIndex = stripped.IndexOf(' ');
            if (spaceIndex >= 0)
                command = stripped.Substring(0, spaceIndex);

            if (config.IgnoredCommands.Exists(c =>
                c.Equals(command, StringComparison.OrdinalIgnoreCase)))
                return;

            _entries.Add(new CommandLogEntry
            {
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),
                PlayerName = player.displayName ?? "Unknown",
                SteamId = player.userID,
                Command = command,
                FullMessage = message
            });

            SaveData();
        }

        #endregion
    }
}
