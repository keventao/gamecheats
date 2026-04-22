using BepInEx.Configuration;
using HarmonyLib;
using LordsAndVilleinsCheats.Core;
using LordsAndVilleinsCheats.Util;
using UnityEngine;

namespace LordsAndVilleinsCheats.Modules
{
    public class TimeCheats : ICheatModule
    {
        public string Id   => "Time";
        public string Name => "Time";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        internal static ConfigEntry<bool>  OverrideSpeed = null!;
        internal static ConfigEntry<float> SpeedValue    = null!;
        internal static ModConfig SharedCfg = null!;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            SharedCfg     = cfg;
            OverrideSpeed = cfg.File.Bind("Time", "OverrideSpeed", false, "Force gameSpeedMultiplierBySpeedLvl every frame to SpeedValue.");
            SpeedValue    = cfg.File.Bind("Time", "SpeedValue",    1.0f,  "Speed multiplier (1=normal, 32=vanilla max).");

            var ok = HarmonyHelpers.SafeRun("TimeCheats.PatchAll", () =>
            {
                var update = AccessTools.Method(typeof(GameManager), "Update")
                          ?? throw new System.Exception("GameManager.Update not found");
                harmony.Patch(update,
                    postfix: new HarmonyMethod(typeof(TimeCheats), nameof(OnGameTick_Postfix)));
            });
            Status = ok ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() {}

        public void DrawGui()
        {
            OverrideSpeed.Value = GUILayout.Toggle(OverrideSpeed.Value, "Override game speed multiplier");
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Multiplier:", GUILayout.Width(80));
                var s = GUILayout.TextField(SpeedValue.Value.ToString("0.##"), GUILayout.Width(80));
                if (float.TryParse(s, out var n)) SpeedValue.Value = Mathf.Clamp(n, 0.0f, 100.0f);
            }
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("0.5x")) { OverrideSpeed.Value = true; SpeedValue.Value = 0.5f; }
                if (GUILayout.Button("1x"))   { OverrideSpeed.Value = true; SpeedValue.Value = 1f;   }
                if (GUILayout.Button("2x"))   { OverrideSpeed.Value = true; SpeedValue.Value = 2f;   }
                if (GUILayout.Button("8x"))   { OverrideSpeed.Value = true; SpeedValue.Value = 8f;   }
                if (GUILayout.Button("32x"))  { OverrideSpeed.Value = true; SpeedValue.Value = 32f;  }
                if (GUILayout.Button("64x"))  { OverrideSpeed.Value = true; SpeedValue.Value = 64f;  }
            }
            GUILayout.Label("(Vanilla max is 32x; higher values may stress the simulation.)");
        }

        public void DisableAll() => OverrideSpeed.Value = false;

        public static void OnGameTick_Postfix(GameManager __instance)
        {
            if (SharedCfg.GlobalDisableAll.Value) return;
            if (!OverrideSpeed.Value) return;
            HarmonyHelpers.SafeRun("TimeCheats.OnGameTick_Postfix", () =>
            {
                Traverse.Create(__instance).Field("gameSpeedMultiplierBySpeedLvl").SetValue(SpeedValue.Value);
            });
        }
    }
}
