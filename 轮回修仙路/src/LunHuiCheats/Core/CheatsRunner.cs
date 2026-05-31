using System;
using System.IO;
using BepInEx.Logging;
using UnityEngine;

namespace LunHuiCheats.Core
{
    /// <summary>
    /// Standalone MonoBehaviour mounted on a DontDestroyOnLoad GameObject.
    /// Ensures Update/OnGUI callbacks fire reliably regardless of game host behavior.
    /// </summary>
    public sealed class CheatsRunner : MonoBehaviour
    {
        public static readonly string DiagFile = Path.Combine(
            Path.GetDirectoryName(typeof(CheatsRunner).Assembly.Location) ?? ".",
            "lunhui-diag.txt");

        private GuiManager? _gui;
        private ManualLogSource? _log;
        private bool _firstUpdateLogged;
        private bool _firstOnGuiLogged;
        private int _updateCount;

        public void Bind(GuiManager gui, ManualLogSource log)
        {
            _gui = gui;
            _log = log;
            WriteDiag("Bind() called on CheatsRunner.");
        }

        private void Awake()     => WriteDiag("CheatsRunner.Awake fired.");
        private void OnEnable()  => WriteDiag("CheatsRunner.OnEnable fired.");
        private void Start()     => WriteDiag("CheatsRunner.Start fired.");
        private void OnDisable() => WriteDiag("CheatsRunner.OnDisable fired.");

        private void OnDestroy()
        {
            WriteDiag("CheatsRunner.OnDestroy fired.");
            Plugin.NotifyRunnerDestroyed();
        }

        private void Update()
        {
            _updateCount++;
            if (!_firstUpdateLogged)
            {
                _firstUpdateLogged = true;
                WriteDiag("CheatsRunner.Update first fired.");
                _log?.LogInfo("[Runner] Update tick is firing.");
            }
            if (_updateCount == 600 || _updateCount == 3000 || _updateCount == 9000)
                WriteDiag($"CheatsRunner.Update reached {_updateCount} ticks.");
            _gui?.HandleInput();

            if (GameRefs.IsReady && Plugin.Cfg != null && !Plugin.Cfg.GlobalDisableAll.Value)
                Plugin.Registry?.OnUpdateAll();
        }

        private void OnGUI()
        {
            if (!_firstOnGuiLogged)
            {
                _firstOnGuiLogged = true;
                WriteDiag("CheatsRunner.OnGUI first fired.");
                _log?.LogInfo("[Runner] OnGUI tick is firing.");
            }
            _gui?.OnGUI();
        }

        public static void WriteDiag(string msg)
        {
            try { File.AppendAllText(DiagFile, $"[{DateTime.Now:HH:mm:ss.fff}] {msg}\n"); }
            catch { /* swallow — diagnostic only */ }
        }
    }
}
