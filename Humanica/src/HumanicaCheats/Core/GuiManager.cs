using UnityEngine;

namespace HumanicaCheats.Core
{
    public class GuiManager
    {
        private readonly ModuleRegistry _registry;
        private bool _open;
        private int _activeTab;
        private Rect _windowRect = new Rect(40, 40, 480, 540);
        private int _lastToggleFrame = -1;
        private Vector2 _scrollPos;
        private const int WindowId = 0xC1EA75;

        public GuiManager(ModuleRegistry registry) => _registry = registry;

        public void OnGUI()
        {
            var ev = Event.current;
            if (ev != null && ev.type == EventType.KeyDown && ev.keyCode == KeyCode.F1
                && UnityEngine.Time.frameCount != _lastToggleFrame)
            {
                _lastToggleFrame = UnityEngine.Time.frameCount;
                _open = !_open;
                ev.Use();
            }

            var tagRect = new Rect(8, 8, 380, 22);
            var prev = GUI.color;
            GUI.color = Color.yellow;
            GUI.Box(tagRect, $"Humanica Cheats v{Plugin.Version} — F1 切换面板 | open={_open}");
            GUI.color = prev;

            if (!_open) return;
            _windowRect = GUI.Window(WindowId, _windowRect, (GUI.WindowFunction)(new System.Action<int>(DrawWindow)), "Humanica Cheats");
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(GameRefs.IsReady ? "● 游戏中" : "○ 菜单", GUILayout.Width(80));
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            if (_registry.Modules.Count == 0)
            {
                GUILayout.Label("无模块已注册。");
                GUI.DragWindow();
                return;
            }

            var tabs = new string[_registry.Modules.Count];
            for (int i = 0; i < _registry.Modules.Count; i++)
            {
                var m = _registry.Modules[i];
                tabs[i] = m.Name + (m.Status == ModuleStatus.Broken ? " (!)" : "");
            }
            _activeTab = GUILayout.Toolbar(_activeTab, tabs);

            GUILayout.Space(4);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            if (_activeTab < _registry.Modules.Count)
            {
                var mod = _registry.Modules[_activeTab];
                if (mod.Status == ModuleStatus.Broken)
                    GUILayout.Label("该模块 Patch 失败。查看 MelonLoader 控制台。");
                else
                    mod.DrawGui();
            }
            GUILayout.EndScrollView();

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
    }
}
