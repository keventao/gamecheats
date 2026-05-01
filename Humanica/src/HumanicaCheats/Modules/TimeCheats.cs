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
            GUILayout.Label($"当前: Time.timeScale = {Time.timeScale:F1}");

            GUILayout.Space(8);
            if (GUILayout.Button("重置为 ×1"))
            {
                _selected = 0;
                Time.timeScale = 1f;
            }
        }
    }
}
