using ForTheKingCheats.Core;
using HarmonyLib;
using UnityEngine;

namespace ForTheKingCheats.Modules
{
    public sealed class PlayerCheats : ICheatModule
    {
        private static bool _lockPartyHealth;

        public string Name { get { return "Players"; } }
        public ModuleStatus Status { get; private set; }

        public static bool LockPartyHealth
        {
            get { return _lockPartyHealth; }
        }

        public void Register(Harmony harmony)
        {
            Status = ModuleStatus.Ok;
        }

        public void Draw()
        {
            _lockPartyHealth = GUILayout.Toggle(_lockPartyHealth, "Lock party HP");

            if (GUILayout.Button("Heal party to full"))
            {
                HealPartyToFull();
            }
        }

        private static void HealPartyToFull()
        {
            var stats = Object.FindObjectsOfType<CharacterStats>();
            foreach (var stat in stats)
            {
                if (IsPlayerStat(stat))
                {
                    stat.SetSpecificHealth(stat.MaxHealth, true);
                }
            }
        }

        private static bool IsPlayerStat(CharacterStats stat)
        {
            return stat != null
                && stat.m_CharacterOverworld != null
                && stat.m_CharacterOverworld.m_FTKPlayerID != null
                && stat.m_CharacterOverworld.m_FTKPlayerID.IsPlayer();
        }

        [HarmonyPatch(typeof(CharacterStats), "SetSpecificHealthRPC")]
        private static class SetSpecificHealthRpcPatch
        {
            private static void Prefix(CharacterStats __instance, ref int _newHp)
            {
                if (!LockPartyHealth || !IsPlayerStat(__instance))
                {
                    return;
                }

                _newHp = PlayerHealthPolicy.GetProtectedHealth(
                    true,
                    __instance.m_HealthCurrent,
                    _newHp,
                    __instance.MaxHealth);
            }
        }
    }
}
