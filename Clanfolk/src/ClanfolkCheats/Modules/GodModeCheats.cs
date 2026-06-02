using System;
using System.Reflection;
using ClanfolkCheats.Core;
using HarmonyLib;
using MelonLoader;

namespace ClanfolkCheats.Modules
{
    public class GodModeCheats : ICheatModule
    {
        public string Name => "上帝";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Ok;

        private static bool _invulnerable;
        private bool _noStarvation;
        private bool _noFreezing;

        public void Register(HarmonyLib.Harmony harmony)
        {
            try
            {
                var healthType = AccessTools.TypeByName("Il2Cpp.AttributeHealth");
                if (healthType == null) { SetError("AttributeHealth not found"); return; }

                var changeHealth = AccessTools.Method(healthType, "ChangeCurrentHealth");
                if (changeHealth == null) { SetError("ChangeCurrentHealth not found"); return; }

                harmony.Patch(changeHealth,
                    prefix: new HarmonyMethod(typeof(GodModeCheats), nameof(Prefix_ChangeCurrentHealth)));

                MelonLogger.Msg("[God] Patched AttributeHealth.ChangeCurrentHealth");
            }
            catch (Exception ex) { SetError(ex.Message); }
        }

        public void DrawGui(Layout l)
        {
            l.Label("God Mode", 22f);
            l.Space(4);

            l.Label("Invulnerability (no damage):");
            _invulnerable = l.Toggle(_invulnerable, _invulnerable ? "ON" : "OFF");

            l.Space(4);
            l.Label("No Starvation (WIP):");
            _noStarvation = l.Toggle(_noStarvation, _noStarvation ? "ON" : "OFF");
            if (_noStarvation) l.Label("  Needs food attribute research.", 18f);

            l.Space(4);
            l.Label("No Freezing (WIP):");
            _noFreezing = l.Toggle(_noFreezing, _noFreezing ? "ON" : "OFF");
            if (_noFreezing) l.Label("  Needs warmth attribute research.", 18f);
        }

        private static bool Prefix_ChangeCurrentHealth(ref int change)
        {
            if (!_invulnerable) return true;
            if (change < 0) { change = 0; } // Block all damage
            return true;
        }

        private void SetError(string msg)
        {
            Status = ModuleStatus.Broken;
            MelonLogger.Error($"[God] {msg}");
        }
    }
}
