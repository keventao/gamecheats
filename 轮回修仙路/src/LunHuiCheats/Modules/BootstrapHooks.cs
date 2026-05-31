using System;
using System.Reflection;
using HarmonyLib;
using LunHuiCheats.Core;
using UnityEngine;

namespace LunHuiCheats.Modules
{
    public static class BootstrapHooks
    {
        private static bool _guiReady;

        public static void Register(Harmony harmony)
        {
            try
            {
                bool hooked = false;
                var sceneControllerType = AccessTools.TypeByName("SceneController");
                if (sceneControllerType != null)
                {
                    var startMethod = AccessTools.Method(sceneControllerType, "Start");
                    if (startMethod != null)
                    {
                        harmony.Patch(startMethod, postfix: new HarmonyMethod(typeof(BootstrapHooks), nameof(StartPostfix)));
                        Plugin.LogSrc?.LogInfo("[BootstrapHooks] Patched SceneController.Start.");
                        hooked = true;
                    }
                }

                if (!hooked)
                {
                    Plugin.LogSrc?.LogWarning("[BootstrapHooks] SceneController not found; falling back to immediate standalone attach.");
                    Plugin.AttachRunnerToGameHost();
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[BootstrapHooks] Failed to register hook: {ex.Message}");
            }
        }

        static void StartPostfix(object __instance)
        {
            if (_guiReady) return;
            Plugin.LogSrc?.LogInfo("[BootstrapHooks] SceneController.Start fired — attaching CheatsRunner.");
            _guiReady = true;
            GameRefs.SetReady(true);
            Plugin.AttachRunnerTo(__instance as UnityEngine.Object);
            TypeScanner.ScanAndDump(Plugin.LogSrc);
            FieldScanner.ScanAndDump(Plugin.LogSrc);
        }
    }
}
