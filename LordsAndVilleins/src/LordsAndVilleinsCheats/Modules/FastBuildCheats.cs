using BepInEx.Configuration;
using HarmonyLib;
using LordsAndVilleinsCheats.Core;
using LordsAndVilleinsCheats.Util;
using UnityEngine;

namespace LordsAndVilleinsCheats.Modules
{
    /// <summary>
    /// Builds finish in 1 tick. Material cost still applies (see BuildCheats for FreeBuilding).
    /// </summary>
    public class FastBuildCheats : ICheatModule
    {
        public string Id   => "FastBuild";
        public string Name => "Fast Build";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        internal static ConfigEntry<bool> FastBuild = null!;
        internal static ModConfig SharedCfg = null!;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            SharedCfg = cfg;
            FastBuild = cfg.File.Bind("FastBuild", "Enabled", false,
                "When on, building construction completes in a single tick (material cost still applies).");

            var ok = HarmonyHelpers.SafeRun("FastBuildCheats.PatchAll", () =>
            {
                var calc = AccessTools.Method(typeof(AIActivity), nameof(AIActivity.CalculateTargetTime))
                        ?? throw new System.Exception("AIActivity.CalculateTargetTime not found");
                harmony.Patch(calc,
                    postfix: new HarmonyMethod(typeof(FastBuildCheats), nameof(OnCalculateTargetTime_Postfix)));
            });
            Status = ok ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() {}

        public void DrawGui()
        {
            FastBuild.Value = GUILayout.Toggle(FastBuild.Value, "Fast build (1-tick construction)");
            GUILayout.Label("Material cost still applies. Use Build > Free building to skip cost.");
        }

        public void DisableAll() => FastBuild.Value = false;

        public static void OnCalculateTargetTime_Postfix(AIActivity __instance, ref float __result)
        {
            if (SharedCfg.GlobalDisableAll.Value) return;
            if (!FastBuild.Value) return;
            if (__instance is BuildBlueprint) __result = 0f;
        }
    }
}
