using BepInEx.Configuration;

namespace NoCheatItems
{
    internal static class ModConfig
    {
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<bool> RemoveFlaggedItems;
        internal static ConfigEntry<bool> RemoveKnownCheatPrefabs;
        internal static ConfigEntry<string> ExtraPrefabNames;
        internal static ConfigEntry<bool> RemoveOverstacked;
        internal static ConfigEntry<bool> RemoveOverquality;
        internal static ConfigEntry<bool> ScanPlayers;
        internal static ConfigEntry<bool> ScanWorldDrops;
        internal static ConfigEntry<bool> ScanContainers;
        internal static ConfigEntry<bool> ScanItemStands;
        internal static ConfigEntry<float> ScanIntervalSeconds;
        internal static ConfigEntry<bool> NotifyPlayers;
        internal static ConfigEntry<bool> LogDeletions;
        internal static ConfigEntry<bool> DryRun;

        internal static void Bind(ConfigFile config)
        {
            const string general = "1 - General";
            const string detection = "2 - Detection";
            const string scan = "3 - Scan";
            const string output = "4 - Output";

            Enabled = config.Bind(general, nameof(Enabled), true,
                "Master switch. When off, the plugin does nothing.");

            RemoveFlaggedItems = config.Bind(detection, nameof(RemoveFlaggedItems), true,
                "Delete items with Valheim 1.0's cheated flag (the grey italic tooltip: \"This item was summoned through cheating means.\").");

            RemoveKnownCheatPrefabs = config.Bind(detection, nameof(RemoveKnownCheatPrefabs), true,
                "Delete vanilla debug/cheat prefabs such as SwordCheat and SledgeCheat even if they are not flagged.");

            ExtraPrefabNames = config.Bind(detection, nameof(ExtraPrefabNames), "",
                "Comma-separated extra prefab names to delete (e.g. HammerCheat, MyModItem). Case-insensitive.");

            RemoveOverstacked = config.Bind(detection, nameof(RemoveOverstacked), false,
                "Delete stacks larger than the item's vanilla max stack size.");

            RemoveOverquality = config.Bind(detection, nameof(RemoveOverquality), false,
                "Delete items whose quality is higher than the prefab allows.");

            ScanPlayers = config.Bind(scan, nameof(ScanPlayers), true,
                "Remove matching items from online player inventories (including equipped items). Offline characters are client-side and can only be cleaned when that player is connected.");

            ScanWorldDrops = config.Bind(scan, nameof(ScanWorldDrops), true,
                "Delete matching items lying on the ground in loaded zones.");

            ScanContainers = config.Bind(scan, nameof(ScanContainers), true,
                "Remove matching items from chests, carts, ships, tombstones, and other containers in loaded zones.");

            ScanItemStands = config.Bind(scan, nameof(ScanItemStands), true,
                "Remove matching items from item stands and armor stands.");

            ScanIntervalSeconds = config.Bind(scan, nameof(ScanIntervalSeconds), 8f,
                "How often to sweep loaded players, containers, and world drops. Pickup and inventory changes are also cleaned immediately.");

            NotifyPlayers = config.Bind(output, nameof(NotifyPlayers), true,
                "Show a center-screen message to a player when items are taken from their inventory.");

            LogDeletions = config.Bind(output, nameof(LogDeletions), true,
                "Write a BepInEx log line for every deleted item.");

            DryRun = config.Bind(output, nameof(DryRun), false,
                "Log what would be deleted without actually removing anything.");
        }
    }
}
