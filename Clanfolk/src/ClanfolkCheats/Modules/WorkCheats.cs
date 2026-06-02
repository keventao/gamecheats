using System;
using System.Reflection;
using ClanfolkCheats.Core;
using HarmonyLib;
using MelonLoader;

namespace ClanfolkCheats.Modules
{
    // Instant work / 瞬间工作 — the work-apply methods all advance progress proportional to
    // their `deltaTime` argument, so a prefix that scales deltaTime up makes any job finish
    // in a few frames (verified, refs/05):
    //   Node.ApplyNodeHarvestWork / ApplyNodeExtractionWork / ClearNodeForHarvest
    //   WorldObject.ApplyObjectHarvestWork / ApplyHarvestWork / ApplyStateWork (build)
    // Every overload's time param is named `deltaTime`, so one Harmony prefix injecting
    // `ref float deltaTime` matches them all by name. (Unit.GetAppliedWorkTime postfix was
    // tried first and had no effect — that value isn't the progress accumulator.)
    public class WorkCheats : ICheatModule
    {
        public string Name => "工作";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        private const float WorkMultiplier = 100f;
        private bool _instant;

        // read by the prefix; OnUpdate syncs it with the toggle
        private static float _sWorkMult = 1f;

        public void Register(HarmonyLib.Harmony harmony)
        {
            try
            {
                int patched = 0;
                patched += PatchByName(harmony, "Il2Cpp.Node",
                    "ApplyNodeHarvestWork", "ApplyNodeExtractionWork", "ClearNodeForHarvest");
                patched += PatchByName(harmony, "Il2Cpp.WorldObject",
                    "ApplyObjectHarvestWork", "ApplyHarvestWork", "ApplyStateWork");

                if (patched > 0)
                {
                    Status = ModuleStatus.Ok;
                    MelonLogger.Msg($"[Work] Patched {patched} work-apply method(s) (deltaTime prefix)");
                }
                else
                {
                    MelonLogger.Warning("[Work] no work-apply methods patched");
                }
            }
            catch (Exception ex) { MelonLogger.Error($"[Work] Register: {ex.Message}"); }
        }

        private int PatchByName(HarmonyLib.Harmony harmony, string typeName, params string[] methodNames)
        {
            var type = AccessTools.TypeByName(typeName);
            if (type == null)
            {
                MelonLogger.Warning($"[Work] type {typeName} not found");
                return 0;
            }

            int count = 0;
            foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (Array.IndexOf(methodNames, m.Name) < 0) continue;
                // only patch overloads that actually expose a `deltaTime` parameter
                if (Array.TrueForAll(m.GetParameters(), p => p.Name != "deltaTime")) continue;
                try
                {
                    harmony.Patch(m, prefix: new HarmonyMethod(typeof(WorkCheats), nameof(Prefix_ScaleDeltaTime)));
                    count++;
                }
                catch (Exception ex) { MelonLogger.Warning($"[Work] patch {typeName}.{m.Name} failed: {ex.Message}"); }
            }
            return count;
        }

        // Harmony binds `deltaTime` by parameter name across every patched overload.
        private static void Prefix_ScaleDeltaTime(ref float deltaTime)
        {
            if (_sWorkMult > 1f) deltaTime *= _sWorkMult;
        }

        public void DrawGui(Layout l)
        {
            l.Label("瞬间工作", 22f);
            l.Space(4);
            l.Label($"瞬间完成 (砍伐/采集/开采/建造) {WorkMultiplier:0}x:");
            _instant = l.Toggle(_instant, _instant ? "开" : "关");
            l.Space(2);
            l.Label("  开 = 工作进度倍率，秒完成。", 18f);
        }

        public void OnUpdate()
        {
            _sWorkMult = _instant ? WorkMultiplier : 1f;
        }
    }
}
