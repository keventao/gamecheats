using UnityEngine;
using HumanicaCheats.Core;

namespace HumanicaCheats.Modules
{
    public class TimeCheats : ICheatModule
    {
        public string       Name   => "时间";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Ok;

        private static readonly float[]  Scales = { 1f, 2f, 5f, 10f };
        private static readonly string[] Labels = { "×1", "×2", "×5", "×10" };
        private int _selected = 0;

        public void Register(HarmonyLib.Harmony harmony)
        {
            // Time.timeScale 是 Unity 全局,无需 patch
        }

        public void DrawGui(Layout l)
        {
            l.Label("游戏速度倍率 (Time.timeScale):");

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

            // 显示"目标 vs 实际":若两值不一致,说明游戏内部每帧覆写 Time.timeScale,
            // 此时需在 v0.2 用 OnUpdate 强写或 patch 覆写源。
            l.Label($"目标: ×{Scales[_selected]:F0}   实际: Time.timeScale = {Time.timeScale:F1}");

            l.Space(8);
            if (l.Button("重置为 ×1", 28f))
            {
                _selected = 0;
                Time.timeScale = 1f;
            }
        }
    }
}
