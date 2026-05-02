using BepInEx.Logging;
using UnityEngine;

namespace ForTheKingCheats.Core
{
    public sealed class CheatsRunner : MonoBehaviour
    {
        private GuiManager? _gui;
        private ManualLogSource? _log;

        public void Bind(GuiManager gui, ManualLogSource log)
        {
            _gui = gui;
            _log = log;
        }

        private void Update()
        {
            if (_gui != null && Input.GetKeyDown(KeyCode.F1))
            {
                _gui.Visible = !_gui.Visible;
                _log?.LogInfo($"GUI visible: {_gui.Visible}");
            }
        }

        private void OnGUI()
        {
            _gui?.Draw();
        }
    }
}
