using System;
using HarmonyLib;
using LunHuiCheats.Core;
using UnityEngine;

namespace LunHuiCheats.Modules
{
    public class DebugDiagnostics : ICheatModule
    {
        public string Id => "debug";
        public string Name => "Debug";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        private bool _scanOnReady;
        private bool _hasAutoScanned;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            Status = ModuleStatus.Ok;
        }

        public void OnGameReady()
        {
            if (_scanOnReady && !_hasAutoScanned)
            {
                _hasAutoScanned = true;
                TypeScanner.ScanAndDump(Plugin.LogSrc);
                FieldScanner.ScanAndDump(Plugin.LogSrc);
            }
        }

        public void DrawGui()
        {
            float x = 0, y = 0, w = 400, h = 24;

            GUI.Label(new Rect(x, y, w, h), "Diagnostics & Reverse-Engineering Tools");
            y += h + 4;

            var prev = GUI.color;
            GUI.color = Color.cyan;
            if (GUI.Button(new Rect(x, y, 180, h), "Run Type Scanner"))
            {
                TypeScanner.ScanAndDump(Plugin.LogSrc);
            }
            GUI.color = prev;

            if (GUI.Button(new Rect(x + 190, y, 180, h), "Run Field Scanner"))
            {
                FieldScanner.ScanAndDump(Plugin.LogSrc);
            }
            y += h + 4;

            GUI.Label(new Rect(x, y, w, h), "Output files are written next to the plugin DLL.");
            y += h + 4;

            _scanOnReady = GUI.Toggle(new Rect(x, y, w, h), _scanOnReady, "Auto-scan on game ready");
        }

        public void DisableAll()
        {
            _scanOnReady = false;
        }
    }
}
