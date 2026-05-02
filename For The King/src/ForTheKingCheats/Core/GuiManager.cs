using UnityEngine;

namespace ForTheKingCheats.Core
{
    public sealed class GuiManager
    {
        private readonly ModuleRegistry _registry;
        private Rect _windowRect = new Rect(40f, 80f, 360f, 420f);
        private Vector2 _scroll;

        public GuiManager(ModuleRegistry registry)
        {
            _registry = registry;
        }

        public bool Visible { get; set; }

        public void Draw()
        {
            if (!Visible)
            {
                return;
            }

            _windowRect = GUI.Window(8201, _windowRect, DrawWindow, "For The King Cheats");
        }

        private void DrawWindow(int id)
        {
            GUILayout.Label("F1 toggles this panel");

            _scroll = GUILayout.BeginScrollView(_scroll);
            foreach (var module in _registry.Modules)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label($"{module.Name} [{module.Status}]");
                module.Draw();
                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
        }
    }
}
