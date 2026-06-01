using UnityEngine;
using ClanfolkCheats.Core;

namespace ClanfolkCheats.Modules
{
    public class TimeCheats : ICheatModule
    {
        public string       Name   => "Time";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Ok;

        private static readonly float[]  Scales = { 1f, 2f, 5f, 10f };
        private static readonly string[] Labels = { "×1", "×2", "×5", "×10" };
        private int _selected = 0;

        public void Register(HarmonyLib.Harmony harmony)
        {
        }

        public void DrawGui(Layout l)
        {
            l.Label("Game Speed (Time.timeScale):");

            const float btnW = 64f, btnH = 26f;
            for (int i = 0; i < Scales.Length; i++)
            {
                var r = new Rect(l.X + i * (btnW + 6f), l.Y, btnW, btnH);
                if (ImguiUtil.Button(r, Labels[i], _selected == i))
                {
                    _selected = i;
                    Time.timeScale = Scales[i];
                }
            }
            l.Y += btnH + 6f;

            l.Label($"Target: ×{Scales[_selected]:F0}   Actual: Time.timeScale = {Time.timeScale:F1}");

            l.Space(8);
            if (l.Button("Reset to ×1", 28f))
            {
                _selected = 0;
                Time.timeScale = 1f;
            }
        }
    }
}
