using System;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using ForTheKingCheats.Core;
using HarmonyLib;
using UnityEngine;

namespace ForTheKingCheats
{
    [BepInPlugin(PluginId, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginId = "com.kk.ftk-cheats";
        public const string PluginName = "For The King Cheats";
        public const string PluginVersion = "0.1.0";

        internal static ManualLogSource Log = null!;
        internal static Harmony HarmonyInstance = null!;
        internal static ModuleRegistry Registry = null!;
        internal static GuiManager Gui = null!;

        private void Awake()
        {
            Log = Logger;

            try
            {
                Log.LogInfo($"Loading {PluginName} {PluginVersion} for game version {Application.version}.");

                HarmonyInstance = new Harmony(PluginId);
                Registry = new ModuleRegistry();
                Registry.Add(new Modules.TimeCheats());
                Registry.RegisterAll(HarmonyInstance);
                HarmonyInstance.PatchAll();

                Gui = new GuiManager(Registry);

                var runnerObject = new GameObject("ForTheKingCheatsRunner");
                DontDestroyOnLoad(runnerObject);
                var runner = runnerObject.AddComponent<CheatsRunner>();
                runner.Bind(Gui, Log);

                LogPatchSummary();
                Log.LogInfo($"{PluginName} ready.");
            }
            catch (Exception ex)
            {
                Log.LogFatal($"Plugin failed to initialize: {ex}");
            }
        }

        private static void LogPatchSummary()
        {
            var ok = Registry.Modules.Count(module => module.Status == ModuleStatus.Ok);
            var broken = Registry.Modules.Count(module => module.Status == ModuleStatus.Broken);
            var pending = Registry.Modules.Count(module => module.Status == ModuleStatus.Pending);

            Log.LogInfo($"Patch summary: {ok}/{Registry.Modules.Count} ok, {broken} broken, {pending} pending.");
        }
    }
}
