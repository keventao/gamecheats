using System;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using LunHuiCheats.Core;
using LunHuiCheats.Util;
using UnityEngine;

namespace LunHuiCheats
{
    [BepInPlugin(PluginId, PluginName, PluginVersion)]
    public class Plugin : BasePlugin
    {
        public const string PluginId      = "com.kk.lunhui-cheats";
        public const string PluginName    = "轮回修仙路 Cheats";
        public const string PluginVersion = "0.0.1";

        // Filled from refs/00-research-checklist.md after version research
        private static readonly string[] KnownCompatibleVersions = { };

        internal static ManualLogSource LogSrc = null!;
        internal static ModConfig       Cfg = null!;
        internal static ModuleRegistry  Registry = null!;
        internal static GuiManager      Gui = null!;
        internal static Harmony         HarmonyInstance = null!;
        private static CheatsRunner? _attachedRunner;
        internal static Plugin? Instance { get; private set; }

        /// <summary>
        /// Attempts to attach the CheatsRunner to a known game host object.
        /// Falls back to a standalone DontDestroyOnLoad GameObject if no host is found.
        /// After reverse-engineering, fill <c>_hostTypeNames</c> with actual game manager type names.
        /// </summary>
        private static readonly string[] _hostTypeNames = new[]
        {
            "GameManager",
            "GameMain",
            "MainManager",
            "SceneController",
            "UIManager",
            "PlayerManager",
            "WorldManager",
            "GameController",
        };

        public static void AttachRunnerToGameHost()
        {
            try
            {
                UnityEngine.Object? host = null;
                foreach (var name in _hostTypeNames)
                {
                    var t = AccessTools.TypeByName(name);
                    if (t == null) continue;
                    host = UnityEngine.Object.FindObjectOfType(t);
                    if (host != null)
                    {
                        CheatsRunner.WriteDiag($"AttachRunnerToGameHost: found host type '{name}'.");
                        break;
                    }
                }

                if (host != null)
                {
                    AttachRunnerTo(host);
                }
                else
                {
                    CheatsRunner.WriteDiag("AttachRunnerToGameHost: no known host type found; creating standalone GameObject.");
                    AttachRunnerTo(null);
                }
            }
            catch (Exception ex)
            {
                CheatsRunner.WriteDiag($"AttachRunnerToGameHost EXCEPTION: {ex}");
            }
        }

        public static void AttachRunnerTo(UnityEngine.Object host)
        {
            if (_attachedRunner != null) return;
            try
            {
                var go = new GameObject("LunHuiCheats");
                UnityEngine.Object.DontDestroyOnLoad(go);
                CheatsRunner.WriteDiag("AttachRunnerTo: created persistent GameObject.");
                var runner = go.AddComponent<CheatsRunner>();
                runner.Bind(Gui, LogSrc);
                _attachedRunner = runner;
                CheatsRunner.WriteDiag("CheatsRunner attached to persistent GameObject.");
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

        public override void Load()
        {
            Instance = this;
            LogSrc = Log;
            HarmonyHelpers.OnFailure = msg => LogSrc.LogError(msg);

            try
            {
                CheckGameVersion();
                BackupSavesOnce();

                Cfg             = new ModConfig(Config);
                HarmonyInstance = new Harmony(PluginId);
                Registry        = new ModuleRegistry();

                Registry.Add(new Modules.TimeCheats());
                Registry.Add(new Modules.DebugDiagnostics());
                Registry.Add(new Modules.GodMode());
                Registry.Add(new Modules.PlayerStats());
                Registry.Add(new Modules.Cultivation());
                Registry.Add(new Modules.Inventory());

                Registry.RegisterAll(Cfg, HarmonyInstance);

                HarmonyHelpers.SafeRun("Harmony.PatchAll", () => HarmonyInstance.PatchAll());

                Gui = new GuiManager(Registry, Cfg);

                try
                {
                    ClassInjector.RegisterTypeInIl2Cpp<CheatsRunner>();
                    LogSrc.LogInfo("CheatsRunner registered in IL2CPP.");
                }
                catch (Exception ex)
                {
                    LogSrc.LogError($"Failed to register CheatsRunner in IL2CPP: {ex.Message}");
                }

                Modules.BootstrapHooks.Register(HarmonyInstance);

                LogPatchSummary();
                LogSrc.LogInfo($"{PluginName} v{PluginVersion} ready (modules: {Registry.Modules.Count}).");
            }
            catch (Exception ex)
            {
                LogSrc.LogFatal($"Plugin failed to initialize: {ex}");
            }
        }

        private void CheckGameVersion()
        {
            var v = Application.version;
            if (KnownCompatibleVersions.Length == 0)
            {
                LogSrc.LogWarning($"Game version '{v}' — no compatibility whitelist set yet. " +
                    "Mod will continue loading; verify in-game behavior before trusting.");
                return;
            }
            if (Array.IndexOf(KnownCompatibleVersions, v) < 0)
            {
                LogSrc.LogWarning(
                    $"Game version '{v}' is not in the compatibility whitelist. " +
                    $"Mod will continue loading; broken patches will be reported per-module.");
            }
            else
            {
                LogSrc.LogInfo($"Game version '{v}' is in compatibility whitelist.");
            }
        }

        private void BackupSavesOnce()
        {
            var saveDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData", "LocalLow", "烟水寒", "轮回修仙路", "GameDataSave_Steam");

            if (!Directory.Exists(saveDir))
            {
                LogSrc.LogWarning($"Save root not found: {saveDir}. Skipping backup.");
                return;
            }
            HarmonyHelpers.SafeRun("SaveBackup",
                () => SaveBackup.Run(saveDir, maxKeep: 5));
        }

        private void LogPatchSummary()
        {
            var ok      = Registry.Modules.Count(m => m.Status == ModuleStatus.Ok);
            var broken  = Registry.Modules.Count(m => m.Status == ModuleStatus.Broken);
            var pending = Registry.Modules.Count(m => m.Status == ModuleStatus.Pending);
            LogSrc.LogInfo($"Patch summary: {ok}/{Registry.Modules.Count} ok, {broken} broken, {pending} pending.");
        }
    }
}
