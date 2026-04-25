using HarmonyLib;
using LordsAndVilleinsCheats.Core;
using LordsAndVilleinsCheats.Util;
using UnityEngine;

namespace LordsAndVilleinsCheats.Modules
{
    public class RoyaltyCheats : ICheatModule
    {
        public string Id   => "Royalty";
        public string Name => "Royalty";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            // No Harmony patches: favorPoints is a public int on the RoyaltyManager singleton.
            // Mirrors the game's own UICheatDialogue.OnFavorPointsGainClick (favorPoints += result).
            Status = ModuleStatus.Ok;
        }

        public void OnGameReady() {}

        public void DrawGui()
        {
            GUILayout.Label($"Favor Points: {ReadFavorPoints()}");

            GUILayout.Space(8);
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+100"))    AddFavorPoints(100);
                if (GUILayout.Button("+1000"))   AddFavorPoints(1000);
                if (GUILayout.Button("+10000"))  AddFavorPoints(10000);
            }
        }

        public void DisableAll() {}

        private static string ReadFavorPoints()
        {
            try
            {
                var rm = RoyaltyManager.instance;
                if (rm == null) return "(no RM)";
                return rm.favorPoints.ToString();
            }
            catch (System.Exception ex)
            {
                return $"(err: {ex.GetType().Name})";
            }
        }

        private static void AddFavorPoints(int amount)
        {
            HarmonyHelpers.SafeRun($"AddFavorPoints({amount})", () =>
            {
                var rm = RoyaltyManager.instance;
                if (rm == null) return;
                rm.favorPoints += amount;
                rm.OnFavorPointsChange?.Invoke();
            });
        }
    }
}
