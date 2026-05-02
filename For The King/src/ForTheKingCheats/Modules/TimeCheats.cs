using ForTheKingCheats.Core;
using HarmonyLib;
using UnityEngine;

namespace ForTheKingCheats.Modules
{
    public sealed class TimeCheats : ICheatModule
    {
        public string Name => "Time";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        public void Register(Harmony harmony)
        {
            Status = ModuleStatus.Ok;
        }

        public void Draw()
        {
            GUILayout.Label($"Current: {Time.timeScale:0.##}x");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("x1"))
            {
                SetScale(1f);
            }

            if (GUILayout.Button("x2"))
            {
                SetScale(2f);
            }

            if (GUILayout.Button("x5"))
            {
                SetScale(5f);
            }

            if (GUILayout.Button("x10"))
            {
                SetScale(10f);
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Reset"))
            {
                SetScale(1f);
            }
        }

        private static void SetScale(float value)
        {
            Time.timeScale = value;
        }
    }
}
