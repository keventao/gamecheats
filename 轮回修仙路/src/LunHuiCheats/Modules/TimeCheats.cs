using HarmonyLib;
using LunHuiCheats.Core;
using UnityEngine;

namespace LunHuiCheats.Modules
{
    /// <summary>
    /// Placeholder module for time-scale controls.
    /// Uses only Rect-based GUI to avoid IL2CPP GUILayout issues.
    /// </summary>
    public class TimeCheats : ICheatModule
    {
        public string Id => "time";
        public string Name => "Time";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        private float _timeScale = 1f;
        private float _appliedScale = 1f;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            Status = ModuleStatus.Ok;
        }

        public void OnGameReady()
        {
        }

        public void DrawGui()
        {
            float x = 0, y = 0, w = 400, h = 24;

            GUI.Label(new Rect(x, y, w, h), "Time Scale Controls (placeholder)");
            y += h + 4;

            GUI.Label(new Rect(x, y, w, h), $"Current Unity Time.timeScale: {Time.timeScale}");
            y += h + 4;

            float btnW = 50;
            if (GUI.Button(new Rect(x, y, btnW, h), "x0.5")) _timeScale = 0.5f;
            if (GUI.Button(new Rect(x + btnW + 4, y, btnW, h), "x1"))   _timeScale = 1f;
            if (GUI.Button(new Rect(x + (btnW + 4) * 2, y, btnW, h), "x2"))   _timeScale = 2f;
            if (GUI.Button(new Rect(x + (btnW + 4) * 3, y, btnW, h), "x5"))   _timeScale = 5f;
            if (GUI.Button(new Rect(x + (btnW + 4) * 4, y, btnW, h), "x10"))  _timeScale = 10f;
            y += h + 4;

            if (!Mathf.Approximately(_timeScale, _appliedScale))
            {
                Time.timeScale = _timeScale;
                _appliedScale = _timeScale;
            }

            GUI.Label(new Rect(x, y, w, h), $"Applied: {_timeScale}");
        }

        public void DisableAll()
        {
            Time.timeScale = 1f;
            _timeScale = 1f;
            _appliedScale = 1f;
        }
    }
}
