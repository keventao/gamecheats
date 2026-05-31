using UnityEngine;

namespace LunHuiCheats.Core
{
    /// <summary>
    /// Renders the cheat panel using only Rect-based IMGUI APIs.
    /// Avoids GUI.Window (requires IL2CPP delegate) and GUILayout.
    /// </summary>
    public class GuiManager
    {
        private readonly ModuleRegistry _registry;
        private readonly ModConfig _config;
        private bool _open;
        private int _activeTab;
        private Rect _panelRect;
        private int _lastToggleFrame = -1;
        private bool _dragging;
        private Vector2 _dragOffset;

        public GuiManager(ModuleRegistry registry, ModConfig config)
        {
            _registry   = registry;
            _config     = config;
            _panelRect  = new Rect(40, 40, config.PanelWidth.Value, config.PanelHeight.Value);
        }

        public void HandleInput()
        {
        }

        public void OnGUI()
        {
            var ev = Event.current;
            if (ev != null && ev.type == EventType.KeyDown && ev.keyCode == _config.ToggleKey.Value
                && Time.frameCount != _lastToggleFrame)
            {
                _lastToggleFrame = Time.frameCount;
                _open = !_open;
                ev.Use();
            }

            // Status tag (always visible)
            var tagRect = new Rect(8, 8, 460, 22);
            var prevColor = GUI.color;
            GUI.color = Color.yellow;
            GUI.Box(tagRect, $"LunHui cheats v{Plugin.PluginVersion} — {_config.ToggleKey.Value} toggles panel  |  open={_open}");
            GUI.color = prevColor;

            if (!_open) return;

            HandleDrag();
            DrawPanel();
        }

        private void HandleDrag()
        {
            var ev = Event.current;
            if (ev == null) return;
            var titleBar = new Rect(_panelRect.x, _panelRect.y, _panelRect.width, 24);

            if (ev.type == EventType.MouseDown && titleBar.Contains(ev.mousePosition))
            {
                _dragging = true;
                _dragOffset = new Vector2(_panelRect.x - ev.mousePosition.x, _panelRect.y - ev.mousePosition.y);
                ev.Use();
            }
            else if (ev.type == EventType.MouseDrag && _dragging)
            {
                _panelRect.x = ev.mousePosition.x + _dragOffset.x;
                _panelRect.y = ev.mousePosition.y + _dragOffset.y;
                ev.Use();
            }
            else if (ev.type == EventType.MouseUp)
            {
                _dragging = false;
            }
        }

        private void DrawPanel()
        {
            // Background box
            GUI.Box(_panelRect, "");

            float x = _panelRect.x + 8;
            float y = _panelRect.y + 4;
            float w = _panelRect.width - 16;

            // Title
            GUI.Label(new Rect(x, y, w, 20), "轮回修仙路 Cheats");
            y += 24;

            // Disable All button + status indicator
            var btnRect = new Rect(x, y, 180, 24);
            var prev = GUI.color;
            GUI.color = _config.GlobalDisableAll.Value ? Color.gray : Color.red;
            if (GUI.Button(btnRect, _config.GlobalDisableAll.Value ? "All Disabled (click to re-enable)" : "Disable All"))
            {
                _config.GlobalDisableAll.Value = !_config.GlobalDisableAll.Value;
                if (_config.GlobalDisableAll.Value) _registry.DisableAll();
            }
            GUI.color = prev;

            GUI.Label(new Rect(x + 190, y, w - 190, 24), GameRefs.IsReady ? "● in-game" : "○ menu");
            y += 30;

            if (!GameRefs.IsReady)
            {
                GUI.Label(new Rect(x, y, w, 20), "Waiting for game world to load…");
                return;
            }

            if (_registry.Modules.Count == 0)
            {
                GUI.Label(new Rect(x, y, w, 20), "No modules registered yet. Add modules in Plugin.Awake().");
                return;
            }

            // Tabs
            float tabW = w / _registry.Modules.Count;
            for (int i = 0; i < _registry.Modules.Count; i++)
            {
                var m = _registry.Modules[i];
                var marker = m.Status switch
                {
                    ModuleStatus.Broken   => " (!)",
                    ModuleStatus.Disabled => " (off)",
                    _ => "",
                };
                var tabRect = new Rect(x + i * tabW, y, tabW - 4, 24);
                var tabPrev = GUI.color;
                if (i == _activeTab) GUI.color = new Color(0.3f, 0.6f, 1f);
                if (GUI.Button(tabRect, m.Name + marker)) _activeTab = i;
                GUI.color = tabPrev;
            }
            y += 28;

            // Module content area
            var contentRect = new Rect(x, y, w, _panelRect.height - (y - _panelRect.y) - 8);
            GUI.BeginGroup(contentRect);
            if (_activeTab < _registry.Modules.Count)
            {
                var module = _registry.Modules[_activeTab];
                if (module.Status == ModuleStatus.Broken)
                    GUI.Label(new Rect(0, 0, contentRect.width, 20), "This module's patches failed to apply. See BepInEx LogOutput.log.");
                else
                    module.DrawGui();
            }
            GUI.EndGroup();

            _config.PanelWidth.Value  = (int)_panelRect.width;
            _config.PanelHeight.Value = (int)_panelRect.height;
        }
    }
}
