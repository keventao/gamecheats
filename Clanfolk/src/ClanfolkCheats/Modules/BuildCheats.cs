using System;
using System.Reflection;
using ClanfolkCheats.Core;
using HarmonyLib;
using MelonLoader;

namespace ClanfolkCheats.Modules
{
    public class BuildCheats : ICheatModule
    {
        public string Name => "建造";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Ok;

        private static bool _instantBuild;
        private static bool _freeBuild;

        public void Register(HarmonyLib.Harmony harmony)
        {
            try
            {
                var woType = AccessTools.TypeByName("Il2Cpp.WorldObject");
                if (woType == null) { SetError("WorldObject not found"); return; }

                // Free build: zero out resource requirements
                var getBuildRes = AccessTools.Method(woType, "GetBuildResourceCount", new Type[] { typeof(string) });
                var getDesiredRes = AccessTools.Method(woType, "GetDesiredBuildResourceCount", new Type[] { AccessTools.TypeByName("Il2Cpp.Entity") });

                if (getBuildRes != null)
                    harmony.Patch(getBuildRes, prefix: new HarmonyMethod(typeof(BuildCheats), nameof(Prefix_ResCount)));
                if (getDesiredRes != null)
                    harmony.Patch(getDesiredRes, prefix: new HarmonyMethod(typeof(BuildCheats), nameof(Prefix_ResCount)));

                // Instant build: complete recipe instantly
                var recipeType = AccessTools.TypeByName("Il2Cpp.Recipe");
                if (recipeType != null)
                {
                    var changeElapsed = AccessTools.Method(recipeType, "ChangeElapsedTime");
                    if (changeElapsed != null)
                        harmony.Patch(changeElapsed, prefix: new HarmonyMethod(typeof(BuildCheats), nameof(Prefix_ChangeElapsed)));
                }

                MelonLogger.Msg("[Build] Patched WorldObject resource counts + Recipe elapsed time");
            }
            catch (Exception ex) { SetError(ex.Message); }
        }

        public void DrawGui(Layout l)
        {
            l.Label("Build Controls", 22f);
            l.Space(4);

            l.Label("Instant Build:");
            _instantBuild = l.Toggle(_instantBuild, _instantBuild ? "ON" : "OFF");

            l.Space(4);
            l.Label("Free Build (no resources):");
            _freeBuild = l.Toggle(_freeBuild, _freeBuild ? "ON" : "OFF");
        }

        private static bool Prefix_ResCount(ref int __result)
        {
            if (!_freeBuild) return true;
            __result = 0;
            return false;
        }

        private static bool Prefix_ChangeElapsed(object __instance, ref float change)
        {
            if (!_instantBuild) return true;
            change = float.MaxValue;
            return true;
        }

        private void SetError(string msg)
        {
            Status = ModuleStatus.Broken;
            MelonLogger.Error($"[Build] {msg}");
        }
    }
}
