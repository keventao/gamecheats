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
        // Holds the live runner; Unity's overloaded == treats destroyed objects as null,
        // so a simple null check tells us when the host GameManager has been wiped (e.g. on save load).
        private static CheatsRunner? _attachedRunner;

        public static void AttachRunnerToGameHost()
        {
            try
            {
                var gm = UnityEngine.Object.FindObjectOfType<GameManager>();
                if (gm == null) { CheatsRunner.WriteDiag("AttachRunnerToGameHost: FindObjectOfType<GameManager>() returned null."); return; }
                AttachRunnerTo(gm);
            }
            catch (Exception ex)
            {
                CheatsRunner.WriteDiag($"AttachRunnerToGameHost EXCEPTION: {ex}");
            }
        }

        public static void AttachRunnerTo(GameManager gm)
        {
            if (gm == null) return;
            // Already attached and host is still alive — nothing to do.
            if (_attachedRunner != null) return;
            try
            {
                var hostGo = gm.gameObject;
                CheatsRunner.WriteDiag($"AttachRunnerTo: host={hostGo.name}, scene={hostGo.scene.name}, active={hostGo.activeInHierarchy}");
                var hostRunner = hostGo.AddComponent<CheatsRunner>();
                hostRunner.Bind(Gui, Log);
                _attachedRunner = hostRunner;
                CheatsRunner.WriteDiag("CheatsRunner attached to GameManager host GameObject.");
            }
            catch (Exception ex)
            {
                CheatsRunner.WriteDiag($"AttachRunnerTo EXCEPTION: {ex}");
            }
        }

        public static void NotifyRunnerDestroyed()
        {
            _attachedRunner = null;
            CheatsRunner.WriteDiag("Plugin.NotifyRunnerDestroyed: cleared attached runner.");
        }

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

                // BaseUnityPlugin.Update/OnGUI was not firing in this game host.
                // Mount our own GameObject + DontDestroyOnLoad to guarantee the loop.
                CheatsRunner.WriteDiag("About to create LAVCheatsRunner GameObject.");
                var runnerGo = new GameObject("LAVCheatsRunner");
                UnityEngine.Object.DontDestroyOnLoad(runnerGo);
                CheatsRunner.WriteDiag($"GameObject created. activeInHierarchy={runnerGo.activeInHierarchy}, scene={runnerGo.scene.name}.");
                var runner = runnerGo.AddComponent<CheatsRunner>();
                CheatsRunner.WriteDiag("AddComponent<CheatsRunner> returned.");
                runner.Bind(Gui, Log);

                LogPatchSummary();
                Log.LogInfo($"{PluginName} v{PluginVersion} ready (modules: {Registry.Modules.Count}).");
            }
            catch (Exception ex)
            {
                Log.LogFatal($"Plugin failed to initialize: {ex}");
            }
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
