using BepInEx.Configuration;
using HarmonyLib;
using LordsAndVilleinsCheats.Core;
using LordsAndVilleinsCheats.Util;
using UnityEngine;

namespace LordsAndVilleinsCheats.Modules
{
    public class BuildCheats : ICheatModule
    {
        public string Id   => "Build";
        public string Name => "Build";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        internal static ConfigEntry<bool> FreeBuilding = null!;
        internal static ModConfig SharedCfg = null!;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            SharedCfg = cfg;
            FreeBuilding = cfg.File.Bind("Build", "FreeBuilding", false,
                "When on, BuildBlueprint.HasResourcesForBlueprint always returns true (NPCs always think materials are available).");

            var ok = HarmonyHelpers.SafeRun("BuildCheats.PatchAll", () =>
            {
                var bbType = typeof(BuildBlueprint);
                // protected method — AccessTools.Method finds non-public methods too.
                var hasRes = AccessTools.Method(bbType, "HasResourcesForBlueprint")
                          ?? throw new System.Exception("BuildBlueprint.HasResourcesForBlueprint not found");
                harmony.Patch(hasRes,
                    prefix: new HarmonyMethod(typeof(BuildCheats), nameof(OnHasResources_Prefix)));
            });
            Status = ok ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() {}

        public void DrawGui()
        {
            FreeBuilding.Value = GUILayout.Toggle(FreeBuilding.Value, "Free building (skip material requirement check)");
            GUILayout.Label("Note: NPCs may still try to physically deliver materials to the blueprint.");
            GUILayout.Label("This bypasses the 'do we have enough?' gate, not the actual deduction.");
        }

        public void DisableAll() => FreeBuilding.Value = false;

        public static bool OnHasResources_Prefix(ref bool __result)
        {
            if (SharedCfg.GlobalDisableAll.Value) return true;   // run original
            if (!FreeBuilding.Value) return true;                // run original
            __result = true;
            return false;                                         // skip original
        }
    }
}
