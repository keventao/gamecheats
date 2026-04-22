using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using LordsAndVilleinsCheats.Core;
using LordsAndVilleinsCheats.Util;
using Needs;
using Skills;
using UnityEngine;

namespace LordsAndVilleinsCheats.Modules
{
    public class PawnCheats : ICheatModule
    {
        public string Id   => "Pawn";
        public string Name => "Pawn";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        internal static ConfigEntry<bool> ClearHunger = null!;
        internal static ConfigEntry<bool> MaxHealth   = null!;
        internal static ConfigEntry<bool> MaxMood     = null!;
        internal static ModConfig SharedCfg = null!;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            SharedCfg   = cfg;
            ClearHunger = cfg.File.Bind("Pawn", "ClearHunger", false, "Each tick, set hunger need to full (1.0) on all family pawns.");
            MaxHealth   = cfg.File.Bind("Pawn", "MaxHealth",   false, "Each tick, set HP to maxHP on all family pawns.");
            MaxMood     = cfg.File.Bind("Pawn", "MaxMood",     false, "Each tick, set baseMood/baseHappiness to high on all pawns.");

            var ok = HarmonyHelpers.SafeRun("PawnCheats.PatchAll", () =>
            {
                var update = AccessTools.Method(typeof(GameManager), "Update")
                          ?? throw new Exception("GameManager.Update not found");
                harmony.Patch(update,
                    postfix: new HarmonyMethod(typeof(PawnCheats), nameof(OnGameTick_Postfix)));
            });
            Status = ok ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() {}

        public void DrawGui()
        {
            ClearHunger.Value = GUILayout.Toggle(ClearHunger.Value, "Keep family hunger full (Eat need = 1)");
            MaxHealth.Value   = GUILayout.Toggle(MaxHealth.Value,   "Keep family HP at max");
            MaxMood.Value     = GUILayout.Toggle(MaxMood.Value,     "Keep family baseMood high");

            GUILayout.Space(8);
            if (GUILayout.Button("Unlock all skills (all family) — one-shot"))
                UnlockAllSkills();
        }

        public void DisableAll()
        {
            ClearHunger.Value = false;
            MaxHealth.Value   = false;
            MaxMood.Value     = false;
        }

        // ----------- helpers -----------

        private static List<WorldNPC> GetFamilyPawns()
        {
            var pm = PlayerManager.instance;
            if (pm == null) return new List<WorldNPC>();
            try
            {
                var org = pm.GetWorldRulingOrganization();
                if (org == null) return new List<WorldNPC>();
                return org.GetFamilyMembersAsReference();
            }
            catch { return new List<WorldNPC>(); }
        }

        private static void UnlockAllSkills()
        {
            HarmonyHelpers.SafeRun("UnlockAllSkills", () =>
            {
                var skills = new List<SkillName>();
                foreach (SkillName s in Enum.GetValues(typeof(SkillName)))
                    if (s != SkillName.Undefined) skills.Add(s);

                foreach (var npc in GetFamilyPawns())
                {
                    if (npc.aquiredSkills == null) continue;
                    foreach (var s in skills) npc.aquiredSkills.Add(s);
                }
            });
        }

        // ----------- patch body -----------

        public static void OnGameTick_Postfix()
        {
            if (SharedCfg.GlobalDisableAll.Value) return;
            if (!ClearHunger.Value && !MaxHealth.Value && !MaxMood.Value) return;

            HarmonyHelpers.SafeRun("PawnCheats.tick", () =>
            {
                foreach (var npc in GetFamilyPawns())
                {
                    if (MaxHealth.Value)
                    {
                        npc.HP = npc.maxHP;
                    }

                    var activeNpc = npc.activeNPC;
                    if (activeNpc == null) continue;

                    if (MaxMood.Value)
                    {
                        // baseMood and baseHappiness are public fields on ActiveNPC
                        activeNpc.baseMood      = 1f;
                        activeNpc.baseHappiness = 1f;
                    }

                    if (ClearHunger.Value)
                    {
                        // AIAgent is a public field; needs is Dictionary<AgentNeedName, AgentNeed>
                        var aiAgent = activeNpc.AIAgent;
                        if (aiAgent?.needs == null) continue;
                        if (aiAgent.needs.TryGetValue(AgentNeedName.Eat, out var eatNeed) && eatNeed != null)
                            eatNeed.SetValue(1f);
                    }
                }
            });
        }
    }
}
