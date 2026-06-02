using System;
using UnityEngine;
using ClanfolkCheats.Core;
using HarmonyLib;
using MelonLoader;

namespace ClanfolkCheats.Modules
{
    public class TimeCheats : ICheatModule
    {
        public string Name => "Time";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Ok;

        private static readonly int[]  Scales = { 1, 2, 5, 10 };
        private static readonly string[] Labels = { "x1", "x2", "x5", "x10" };
        private int _selected;
        private object? _timeManager;
        private bool _triedInit;

        public void Register(HarmonyLib.Harmony harmony)
        {
        }

        public void DrawGui(Layout l)
        {
            if (!_triedInit)
            {
                _triedInit = true;
                TryInit();
            }

            l.Label("Game Speed:");

            const float btnW = 64f, btnH = 26f;
            for (int i = 0; i < Scales.Length; i++)
            {
                var r = new Rect(l.X + i * (btnW + 6f), l.Y, btnW, btnH);
                if (ImguiUtil.Button(r, Labels[i], _selected == i))
                {
                    _selected = i;
                    SetSpeed(Scales[i]);
                }
            }
            l.Y += btnH + 6f;

            var actual = _timeManager != null ? "game TimeScale" : $"Time.timeScale = {Time.timeScale:F1}";
            l.Label($"Target: x{Scales[_selected]}   ({actual})");

            l.Space(8);
            if (l.Button("Reset to x1", 28f))
            {
                _selected = 0;
                SetSpeed(1);
            }
        }

        private void TryInit()
        {
            try
            {
                var gm = GameRefs.GetGameManager();
                if (gm == null) { _triedInit = false; return; }

                var getTM = AccessTools.Method(gm.GetType(), "GetTimeManager");
                if (getTM != null)
                    _timeManager = getTM.Invoke(gm, null);

                if (_timeManager != null)
                    MelonLogger.Msg("[Time] OK — using game TimeManager");
            }
            catch { _triedInit = false; }
        }

        private void SetSpeed(int speed)
        {
            if (_timeManager != null)
            {
                try
                {
                    var timeScaleType = AccessTools.TypeByName("Il2Cpp.TimeScale");
                    if (timeScaleType != null && timeScaleType.IsEnum)
                    {
                        var scaleValue = Enum.ToObject(timeScaleType, speed);
                        var setTS = AccessTools.Method(_timeManager.GetType(), "SetTimeScale");
                        setTS?.Invoke(_timeManager, new object[] { scaleValue });
                        return;
                    }
                }
                catch { }
            }
            Time.timeScale = speed;
        }
    }
}
