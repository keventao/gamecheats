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

                PatchCaptureMethod(harmony, "FakeInventoryData", "LoadData", nameof(CaptureInventory));
                PatchCaptureMethod(harmony, "FakeInventoryData", "GetAllBackPackInvObjects", nameof(CaptureInventory));
                PatchCaptureMethod(harmony, "FakeInventoryData", "AddItem", nameof(CaptureInventory));
                PatchCaptureMethod(harmony, "CharacterData", "init", nameof(CaptureCharacterData));
                PatchCaptureMethod(harmony, "CharacterData", "AddDaoxin", nameof(CaptureCharacterData));
                PatchCaptureMethod(harmony, "CharacterData", "LeveUp", nameof(CaptureCharacterData));
            }
            catch (Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[BootstrapHooks] Failed to register hook: {ex.Message}");
            }
        }

        private static void PatchCaptureMethod(Harmony harmony, string typeName, string methodName, string patchMethod)
        {
            try
            {
                var t = AccessTools.TypeByName(typeName);
                if (t == null) { Plugin.LogSrc?.LogWarning($"[BootstrapHooks] {typeName} not found for patching."); return; }
                var m = AccessTools.Method(t, methodName);
                if (m == null) { Plugin.LogSrc?.LogWarning($"[BootstrapHooks] {typeName}.{methodName} not found."); return; }
                harmony.Patch(m, postfix: new HarmonyMethod(typeof(BootstrapHooks), patchMethod));
                Plugin.LogSrc?.LogInfo($"[BootstrapHooks] Patched {typeName}.{methodName} to capture instance.");
            }
            catch (Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[BootstrapHooks] Patch {typeName}.{methodName} failed: {ex.Message}");
            }
        }

        static void StartPostfix(object __instance)
        {
            if (_guiReady) return;
            Plugin.LogSrc?.LogInfo("[BootstrapHooks] SceneController.Start fired — attaching CheatsRunner.");
            _guiReady = true;
            GameRefs.SetReady(true, __instance as UnityEngine.Object);
            Plugin.AttachRunnerTo(__instance as UnityEngine.Object);
            TypeScanner.ScanAndDump(Plugin.LogSrc);
            FieldScanner.ScanAndDump(Plugin.LogSrc);
        }

        public static void CaptureInventory(object __instance)
        {
            GameRefs.PassInventory(__instance);
        }

        public static void CaptureCharacterData(object __instance)
        {
            GameRefs.PassCharacterData(__instance);
        }
    }
}
