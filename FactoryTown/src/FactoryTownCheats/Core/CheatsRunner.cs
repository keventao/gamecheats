using BepInEx.Logging;
using FactoryTownCheats.Modules;
using UnityEngine;

namespace FactoryTownCheats.Core
{
    public sealed class CheatsRunner : MonoBehaviour
    {
        private const int WindowId = 0xFAC70;
        private Rect _windowRect = new(40, 40, 420, 160);
        private bool _open;
        private ManualLogSource? _log;

        public void Bind(ManualLogSource log)
        {
            _log = log;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _open = !_open;
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(8, 8, 380, 22), $"Factory Town Cheats v{Plugin.PluginVersion} - F1 panel");

            if (!_open)
            {
                return;
            }

            _windowRect = GUI.Window(WindowId, _windowRect, DrawWindow, "Factory Town Cheats");
        }

        private void DrawWindow(int id)
        {
            GUILayout.Label("Omni Factory");
            GUILayout.Label(OmniFactoryRecipes.LastStatus);

            if (GUILayout.Button("Inject Item Generator Omni Recipes"))
            {
                _log?.LogInfo("[GUI] Manual Omni recipe injection requested.");
                OmniFactoryRecipes.InjectNow();
            }

            GUILayout.Label("Target: ItemGenerator | Input: 1 Wood | Output: selected recipe only");
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
    }
}
