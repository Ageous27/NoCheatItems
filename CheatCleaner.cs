using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoCheatItems
{
    internal static class CheatCleaner
    {
        internal static readonly string[] DefaultCheatPrefabs =
        {
            "SwordCheat",
            "SledgeCheat"
        };

        private static bool _sweeping;

        internal static bool IsServer()
        {
            return ZNet.instance != null && ZNet.instance.IsServer();
        }

        internal static bool IsCheatItem(ItemDrop.ItemData item, out string reason)
        {
            reason = null;
            if (item == null)
            {
                return false;
            }

            if (ModConfig.RemoveFlaggedItems.Value && item.m_cheated)
            {
                reason = "1.0 cheated flag";
                return true;
            }

            string prefab = GetPrefabName(item);
            if (IsBannedPrefab(prefab))
            {
                reason = "cheat prefab " + prefab;
                return true;
            }

            var shared = item.m_shared;
            if (shared != null)
            {
                if (ModConfig.RemoveOverstacked.Value && shared.m_maxStackSize > 0 && item.m_stack > shared.m_maxStackSize)
                {
                    reason = $"overstacked {item.m_stack}/{shared.m_maxStackSize}";
                    return true;
                }

                if (ModConfig.RemoveOverquality.Value && shared.m_maxQuality > 0 && item.m_quality > shared.m_maxQuality)
                {
                    reason = $"overquality {item.m_quality}/{shared.m_maxQuality}";
                    return true;
                }
            }

            return false;
        }

        internal static bool IsBannedPrefab(string prefab)
        {
            if (string.IsNullOrEmpty(prefab))
            {
                return false;
            }

            if (ModConfig.RemoveKnownCheatPrefabs.Value)
            {
                foreach (string name in DefaultCheatPrefabs)
                {
                    if (prefab.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            string extra = ModConfig.ExtraPrefabNames.Value;
            if (string.IsNullOrWhiteSpace(extra))
            {
                return false;
            }

            foreach (string token in extra.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (prefab.Equals(token.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        internal static string GetPrefabName(ItemDrop.ItemData item)
        {
            if (item?.m_dropPrefab != null)
            {
                return StripClone(item.m_dropPrefab.name);
            }

            return item?.m_shared != null ? item.m_shared.m_name : "";
        }

        internal static string StripClone(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "";
            }

            int index = name.IndexOf('(');
            return index > 0 ? name.Substring(0, index).Trim() : name;
        }

        internal static int SweepInventory(Inventory inventory, string context, Player owner = null)
        {
            if (inventory == null || !ModConfig.Enabled.Value)
            {
                return 0;
            }

            List<ItemDrop.ItemData> all = new List<ItemDrop.ItemData>(inventory.GetAllItems());
            if (all == null || all.Count == 0)
            {
                return 0;
            }

            var doomed = new List<ItemDrop.ItemData>();
            var reasons = new List<string>();
            foreach (ItemDrop.ItemData item in all)
            {
                if (IsCheatItem(item, out string reason))
                {
                    doomed.Add(item);
                    reasons.Add(reason);
                }
            }

            if (doomed.Count == 0)
            {
                return 0;
            }

            if (owner == null)
            {
                owner = FindPlayer(inventory);
            }

            int removed = 0;
            for (int i = 0; i < doomed.Count; i++)
            {
                ItemDrop.ItemData item = doomed[i];
                string reason = reasons[i];
                string label = Describe(item);

                if (ModConfig.LogDeletions.Value)
                {
                    string who = owner != null ? owner.GetPlayerName() : context;
                    NoCheatItemsPlugin.Log.LogInfo($"[{(ModConfig.DryRun.Value ? "dry-run" : "delete")}] {label} ({reason}) from {who}");
                }

                if (ModConfig.DryRun.Value)
                {
                    removed++;
                    continue;
                }

                if (owner != null && item.m_equipped)
                {
                    owner.UnequipItem(item, triggerEquipEffects: false);
                }

                inventory.RemoveItem(item);
                removed++;
            }

            if (removed > 0 && owner != null && ModConfig.NotifyPlayers.Value && !ModConfig.DryRun.Value)
            {
                owner.Message(MessageHud.MessageType.Center, removed == 1
                    ? "Cheated item removed."
                    : $"{removed} cheated items removed.");
            }

            return removed;
        }

        internal static void HandleWorldDrop(ItemDrop drop)
        {
            if (!ModConfig.Enabled.Value || !ModConfig.ScanWorldDrops.Value || !IsServer() || drop == null)
            {
                return;
            }

            if (!HasValidNetView(drop))
            {
                return;
            }

            if (!IsCheatItem(drop.m_itemData, out string reason) && !IsBannedPrefab(StripClone(drop.gameObject.name)))
            {
                return;
            }

            string label = Describe(drop.m_itemData);
            if (string.IsNullOrEmpty(label))
            {
                label = StripClone(drop.gameObject.name);
            }

            if (ModConfig.LogDeletions.Value)
            {
                Vector3 pos = drop.transform.position;
                NoCheatItemsPlugin.Log.LogInfo($"[{(ModConfig.DryRun.Value ? "dry-run" : "delete")}] world drop {label} ({reason ?? "cheat prefab"}) at {pos.x:F0},{pos.y:F0},{pos.z:F0}");
            }

            if (ModConfig.DryRun.Value)
            {
                return;
            }

            DestroyNetworked(drop.gameObject);
        }

        internal static void SweepLoadedWorld(string context)
        {
            if (_sweeping || !ModConfig.Enabled.Value || !IsServer())
            {
                return;
            }

            _sweeping = true;
            try
            {
                if (ModConfig.ScanPlayers.Value)
                {
                    List<Player> players = Player.GetAllPlayers();
                    if (players != null)
                    {
                        foreach (Player player in players)
                        {
                            if (player == null)
                            {
                                continue;
                            }

                            SweepInventory(player.GetInventory(), "player " + player.GetPlayerName(), player);
                        }
                    }
                }

                if (ModConfig.ScanContainers.Value)
                {
                    foreach (Container container in FindLoaded<Container>())
                    {
                        if (container == null || !HasValidNetView(container))
                        {
                            continue;
                        }

                        SweepInventory(container.GetInventory(), DescribeContainer(container));
                    }
                }

                if (ModConfig.ScanWorldDrops.Value)
                {
                    foreach (ItemDrop drop in FindLoaded<ItemDrop>())
                    {
                        HandleWorldDrop(drop);
                    }
                }

                if (ModConfig.ScanItemStands.Value)
                {
                    SweepStands();
                }
            }
            catch (Exception ex)
            {
                NoCheatItemsPlugin.Log.LogError($"Sweep ({context}) failed: {ex}");
            }
            finally
            {
                _sweeping = false;
            }
        }

        internal static bool Sweeping => _sweeping;

        internal static void RunGuardedInventorySweep(Inventory inventory)
        {
            if (_sweeping || inventory == null || !ModConfig.Enabled.Value || !IsServer())
            {
                return;
            }

            bool scanPlayers = ModConfig.ScanPlayers.Value;
            bool scanContainers = ModConfig.ScanContainers.Value;
            if (!scanPlayers && !scanContainers)
            {
                return;
            }

            Player owner = FindPlayer(inventory);
            if (owner != null)
            {
                if (!scanPlayers)
                {
                    return;
                }
            }
            else if (!scanContainers)
            {
                return;
            }

            _sweeping = true;
            try
            {
                SweepInventory(inventory, owner != null ? "player " + owner.GetPlayerName() : "container", owner);
            }
            finally
            {
                _sweeping = false;
            }
        }

        private static void SweepStands()
        {
            foreach (ItemStand stand in FindLoaded<ItemStand>())
            {
                if (stand == null || !HasValidNetView(stand) || !stand.HaveAttachment())
                {
                    continue;
                }

                string attached = PrefabNameFromHash(stand.GetAttachedItem());
                if (string.IsNullOrEmpty(attached))
                {
                    attached = stand.m_currentItemName;
                }

                if (string.IsNullOrEmpty(attached) || !IsBannedPrefab(StripClone(attached)))
                {
                    continue;
                }

                LogStand("item stand", attached, stand.transform.position);
                if (!ModConfig.DryRun.Value)
                {
                    stand.DestroyAttachment();
                }
            }

            foreach (ArmorStand stand in FindLoaded<ArmorStand>())
            {
                if (stand == null || !HasValidNetView(stand))
                {
                    continue;
                }

                for (int i = 0; i < 16; i++)
                {
                    if (!stand.HaveAttachment(i))
                    {
                        continue;
                    }

                    string attached = PrefabNameFromHash(stand.GetAttachedItem(i));
                    if (string.IsNullOrEmpty(attached) || !IsBannedPrefab(StripClone(attached)))
                    {
                        continue;
                    }

                    LogStand("armor stand", attached, stand.transform.position);
                    if (!ModConfig.DryRun.Value)
                    {
                        stand.DestroyAttachment(i);
                    }
                }
            }
        }

        private static void LogStand(string kind, string prefab, Vector3 pos)
        {
            if (!ModConfig.LogDeletions.Value)
            {
                return;
            }

            NoCheatItemsPlugin.Log.LogInfo($"[{(ModConfig.DryRun.Value ? "dry-run" : "delete")}] {kind} {prefab} at {pos.x:F0},{pos.y:F0},{pos.z:F0}");
        }

        private static T[] FindLoaded<T>() where T : UnityEngine.Object
        {
            return UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }

        private static string PrefabNameFromHash(int hash)
        {
            if (hash == 0 || ObjectDB.instance == null)
            {
                return "";
            }

            GameObject prefab = ObjectDB.instance.GetItemPrefab(hash);
            return prefab != null ? StripClone(prefab.name) : "";
        }

        private static bool HasValidNetView(Component component)
        {
            if (component == null)
            {
                return false;
            }

            ZNetView view = component.GetComponent<ZNetView>();
            return view != null && view.IsValid();
        }

        private static void DestroyNetworked(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            if (ZNetScene.instance != null)
            {
                ZNetScene.instance.Destroy(go);
            }
            else
            {
                UnityEngine.Object.Destroy(go);
            }
        }

        private static Player FindPlayer(Inventory inventory)
        {
            List<Player> players = Player.GetAllPlayers();
            if (players == null)
            {
                return null;
            }

            foreach (Player player in players)
            {
                if (player != null && player.GetInventory() == inventory)
                {
                    return player;
                }
            }

            return null;
        }

        private static string Describe(ItemDrop.ItemData item)
        {
            if (item == null)
            {
                return "item";
            }

            string prefab = GetPrefabName(item);
            string loc = item.m_shared != null ? item.m_shared.m_name : "";
            int stack = item.m_stack;
            if (!string.IsNullOrEmpty(prefab) && prefab != loc)
            {
                return $"{prefab} x{stack} ({loc})";
            }

            return string.IsNullOrEmpty(prefab) ? $"stack {stack}" : $"{prefab} x{stack}";
        }

        private static string DescribeContainer(Container container)
        {
            string name = !string.IsNullOrEmpty(container.m_name) ? container.m_name : StripClone(container.gameObject.name);
            Vector3 pos = container.transform.position;
            return $"{name} at {pos.x:F0},{pos.y:F0},{pos.z:F0}";
        }
    }
}
