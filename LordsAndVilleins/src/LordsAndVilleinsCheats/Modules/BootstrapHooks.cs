using HarmonyLib;
using LordsAndVilleinsCheats.Core;
using LordsAndVilleinsCheats.Util;

namespace LordsAndVilleinsCheats.Modules
{
    /// <summary>
    /// Registers Harmony hooks on LoadingManager.InitGame / ExitToMainMenu
    /// so GameRefs.IsReady tracks whether the player is in a loaded world.
    /// </summary>
    internal static class BootstrapHooks
    {
        public static void Register(Harmony harmony)
        {
            HarmonyHelpers.SafeRun("BootstrapHooks.LoadHook", () =>
            {
                var initGame = AccessTools.Method(typeof(LoadingManager), "InitGame")
                            ?? throw new System.Exception("LoadingManager.InitGame not found");
                harmony.Patch(initGame,
                    postfix: new HarmonyMethod(typeof(BootstrapHooks), nameof(OnLoadGame_Postfix)));
            });
            HarmonyHelpers.SafeRun("BootstrapHooks.UnloadHook", () =>
            {
                var exit = AccessTools.Method(typeof(LoadingManager), "ExitToMainMenu")
                        ?? throw new System.Exception("LoadingManager.ExitToMainMenu not found");
                harmony.Patch(exit,
                    postfix: new HarmonyMethod(typeof(BootstrapHooks), nameof(OnExitToMainMenu_Postfix)));
            });
        }

        public static void OnLoadGame_Postfix(LoadingManager __instance, bool loadedSaveFile)
        {
            HarmonyHelpers.SafeRun("OnLoadGame_Postfix", () =>
            {
                CheatsRunner.WriteDiag($"BootstrapHooks.OnLoadGame_Postfix fired. IsInMainMenu={__instance.IsInMainMenu()}");
                if (__instance.IsInMainMenu()) return;
                GameRefs.IsReady = true;
                Plugin.Registry?.NotifyGameReady();
                Plugin.AttachRunnerToGameHost();
            });
        }

        public static void OnExitToMainMenu_Postfix()
        {
            HarmonyHelpers.SafeRun("OnExitToMainMenu_Postfix", () =>
            {
                CheatsRunner.WriteDiag("BootstrapHooks.OnExitToMainMenu_Postfix fired.");
                GameRefs.Reset();
                Plugin.Registry?.ResetGameReady();
            });
        }
    }
}
