using System;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LordsAndVilleinsCheats.Core;
using LordsAndVilleinsCheats.Util;
using UnityEngine;

namespace LordsAndVilleinsCheats
{
    [BepInPlugin(PluginId, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginId      = "com.kk.lav-cheats";
        public const string PluginName    = "Lords & Villeins Cheats";
        public const string PluginVersion = "0.1.0";

        // Filled from refs/06-version-research.md
        private static readonly string[] KnownCompatibleVersions = { "1.6.15" };

        internal static ManualLogSource Log = null!;
        internal static ModConfig       Cfg = null!;
        internal static ModuleRegistry  Registry = null!;
        internal static GuiManager      Gui = null!;
        internal static Harmony         HarmonyInstance = null!;

        private void Awake()
        {
            Log = Logger;
            HarmonyHelpers.OnFailure = msg => Log.LogError(msg);

            try
            {
                CheckGameVersion();
                BackupSavesOnce();

                Cfg             = new ModConfig(Config);
                HarmonyInstance = new Harmony(PluginId);
                Registry        = new ModuleRegistry();

                Registry.Add(new Modules.EconomyCheats());
                Registry.Add(new Modules.PawnCheats());
                Registry.Add(new Modules.TimeCheats());
                Registry.Add(new Modules.BuildCheats());

                Registry.RegisterAll(Cfg, HarmonyInstance);

                Modules.BootstrapHooks.Register(HarmonyInstance);

                HarmonyHelpers.SafeRun("Harmony.PatchAll", () => HarmonyInstance.PatchAll());

                Gui = new GuiManager(Registry, Cfg);

                LogPatchSummary();
                Log.LogInfo($"{PluginName} v{PluginVersion} ready (modules: {Registry.Modules.Count}).");
            }
            catch (Exception ex)
            {
                Log.LogFatal($"Plugin failed to initialize: {ex}");
            }
        }

        private void Update()
        {
            try { Gui?.HandleInput(); } catch (Exception ex) { Log.LogError(ex); }
        }

        private void OnGUI()
        {
            try { Gui?.OnGUI(); } catch (Exception ex) { Log.LogError(ex); }
        }

        private void CheckGameVersion()
        {
            var v = Application.version;
            if (Array.IndexOf(KnownCompatibleVersions, v) < 0)
            {
                Log.LogWarning(
                    $"Game version '{v}' is not in the compatibility whitelist. " +
                    $"Mod will continue loading; broken patches will be reported per-module.");
            }
            else
            {
                Log.LogInfo($"Game version '{v}' is in compatibility whitelist.");
            }
        }

        private void BackupSavesOnce()
        {
            var localLow = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData", "LocalLow",
                "Honestly Games", "Lords and Villeins", "SaveData");

            if (!Directory.Exists(localLow))
            {
                Log.LogWarning($"Save root not found: {localLow}. Skipping backup.");
                return;
            }
            foreach (var steamDir in Directory.GetDirectories(localLow))
            {
                HarmonyHelpers.SafeRun($"SaveBackup({Path.GetFileName(steamDir)})",
                    () => SaveBackup.Run(steamDir, maxKeep: 5));
            }
        }

        private void LogPatchSummary()
        {
            var ok      = Registry.Modules.Count(m => m.Status == ModuleStatus.Ok);
            var broken  = Registry.Modules.Count(m => m.Status == ModuleStatus.Broken);
            var pending = Registry.Modules.Count(m => m.Status == ModuleStatus.Pending);
            Log.LogInfo($"Patch summary: {ok}/{Registry.Modules.Count} ok, {broken} broken, {pending} pending.");
        }
    }
}
