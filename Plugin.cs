using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace NoCheatItems
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class NoCheatItemsPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "greg.nocheatitems";
        public const string PluginName = "NoCheatItems";
        public const string PluginVersion = "1.0.0";

        internal static NoCheatItemsPlugin Instance { get; private set; }
        internal static ManualLogSource Log { get; private set; }

        private Harmony _harmony;
        private float _scanTimer;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            ModConfig.Bind(Config);

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.LogInfo($"{PluginName} {PluginVersion} loaded. Server-side cheated-item cleanup is active.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        private void Update()
        {
            if (!ModConfig.Enabled.Value || !CheatCleaner.IsServer())
            {
                return;
            }

            float interval = Mathf.Max(1f, ModConfig.ScanIntervalSeconds.Value);
            _scanTimer += Time.deltaTime;
            if (_scanTimer < interval)
            {
                return;
            }

            _scanTimer = 0f;
            CheatCleaner.SweepLoadedWorld("periodic");
        }
    }
}
