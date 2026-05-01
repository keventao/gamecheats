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

        public void DrawGui()
        {
            GUILayout.Label("游戏速度倍率 (Time.timeScale):");
            GUILayout.BeginHorizontal();
            for (int i = 0; i < Scales.Length; i++)
            {
                var prev = GUI.color;
                GUI.color = _selected == i ? Color.green : Color.white;
                if (GUILayout.Button(Labels[i], GUILayout.Width(64)))
                {
                    _selected = i;
                    Time.timeScale = Scales[i];
                }
                GUI.color = prev;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            // 显示"目标 vs 实际":若两值不一致,说明游戏内部每帧覆写 Time.timeScale,
            // 此时需在 v0.2 用 OnUpdate 强写或 patch 覆写源。
            GUILayout.Label($"目标: ×{Scales[_selected]:F0}   实际: Time.timeScale = {Time.timeScale:F1}");

            GUILayout.Space(8);
            if (GUILayout.Button("重置为 ×1"))
            {
                _selected = 0;
                Time.timeScale = 1f;
            }
        }
    }
}
