using HarmonyLib;

namespace NoCheatItems
{
    [HarmonyPatch(typeof(Inventory), "Changed")]
    internal static class InventoryChangedPatch
    {
        private static void Postfix(Inventory __instance)
        {
            CheatCleaner.RunGuardedInventorySweep(__instance);
        }
    }

    [HarmonyPatch(typeof(ItemDrop), "Awake")]
    internal static class ItemDropAwakePatch
    {
        private static void Postfix(ItemDrop __instance)
        {
            CheatCleaner.HandleWorldDrop(__instance);
        }
    }

    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Load))]
    internal static class ItemDropLoadPatch
    {
        private static void Postfix(ItemDrop __instance)
        {
            CheatCleaner.HandleWorldDrop(__instance);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
    internal static class PlayerSpawnedPatch
    {
        private static void Postfix(Player __instance)
        {
            if (!ModConfig.Enabled.Value || !ModConfig.ScanPlayers.Value || !CheatCleaner.IsServer())
            {
                return;
            }

            CheatCleaner.RunGuardedInventorySweep(__instance.GetInventory());
        }
    }

    [HarmonyPatch(typeof(Terminal), "InitTerminal")]
    internal static class TerminalInitPatch
    {
        private static void Postfix()
        {
            new Terminal.ConsoleCommand("nocheatitems_sweep", "Sweep loaded cheated items now", args =>
            {
                if (!CheatCleaner.IsServer())
                {
                    args.Context.AddString("NoCheatItems only runs on the server.");
                    return;
                }

                CheatCleaner.SweepLoadedWorld("console");
                args.Context.AddString("NoCheatItems sweep complete. See BepInEx log for details.");
            }, isCheat: false, isNetwork: false, onlyServer: true);
        }
    }
}
