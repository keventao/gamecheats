using UnityEngine;

namespace LordsAndVilleinsCheats.Core
{
    /// <summary>
    /// Renders the F-key panel: a tabbed window with one tab per module,
    /// plus a top-bar "Disable All" red button and a "module status" indicator.
    /// </summary>
    public class GuiManager
    {
        private readonly ModuleRegistry _registry;
        private readonly ModConfig _config;
        private bool _open;
        private int _activeTab;
        private Rect _windowRect;
        private int _lastToggleFrame = -1;
        private const int WindowId = 0xCEA751;

        public GuiManager(ModuleRegistry registry, ModConfig config)
        {
            _registry   = registry;
            _config     = config;
            _windowRect = new Rect(40, 40, config.PanelWidth.Value, config.PanelHeight.Value);
        }

        public void HandleInput()
        {
            // OnGUI's Event.current is the toggle path; legacy Input would double-toggle within the same frame.
        }

        public void OnGUI()
        {
            // Listen to IMGUI's own event stream — survives legacy Input being suppressed.
            // Frame guard prevents multi-pass IMGUI (Layout/Repaint/KeyDown) from toggling more than once per key press.
            var ev = Event.current;
            if (ev != null && ev.type == EventType.KeyDown && ev.keyCode == _config.ToggleKey.Value
                && Time.frameCount != _lastToggleFrame)
            {
                _lastToggleFrame = Time.frameCount;
                _open = !_open;
                CheatsRunner.WriteDiag($"[Input] OnGUI Event.KeyDown {ev.keyCode} -> open={_open} (frame={Time.frameCount})");
                ev.Use();
            }

            // Always-on bright tag so it survives even if the game's UI overlays the corner.
            var tagRect = new Rect(8, 8, 460, 22);
            var prev = GUI.color;
            GUI.color = Color.yellow;
            GUI.Box(tagRect, $"LAV cheats v{Plugin.PluginVersion} — {_config.ToggleKey.Value} toggles panel  |  open={_open}");
            GUI.color = prev;

            if (!_open) return;
            _windowRect = GUI.Window(WindowId, _windowRect, DrawWindow, "Lords & Villeins Cheats");
            _config.PanelWidth.Value  = (int)_windowRect.width;
            _config.PanelHeight.Value = (int)_windowRect.height;
        }

        private void DrawWindow(int id)
        {
            using (new GUILayout.HorizontalScope())
            {
                var prev = GUI.color;
                GUI.color = _config.GlobalDisableAll.Value ? Color.gray : Color.red;
                if (GUILayout.Button(_config.GlobalDisableAll.Value ? "All Disabled (click to re-enable)" : "Disable All",
                                     GUILayout.Height(24)))
                {
                    _config.GlobalDisableAll.Value = !_config.GlobalDisableAll.Value;
                    if (_config.GlobalDisableAll.Value) _registry.DisableAll();
                }
                GUI.color = prev;

                GUILayout.FlexibleSpace();
                GUILayout.Label(GameRefs.IsReady ? "● in-game" : "○ menu", GUILayout.Width(80));
            }

            GUILayout.Space(4);

            if (!GameRefs.IsReady)
            {
                GUILayout.Label("Waiting for game world to load…");
                GUI.DragWindow();
                return;
            }

            if (_registry.Modules.Count == 0)
            {
                GUILayout.Label("No modules registered.");
                GUI.DragWindow();
                return;
            }

            var tabs = new string[_registry.Modules.Count];
            for (int i = 0; i < _registry.Modules.Count; i++)
            {
                var m = _registry.Modules[i];
                var marker = m.Status switch
                {
                    ModuleStatus.Broken   => " (!)",
                    ModuleStatus.Disabled => " (off)",
                    _ => "",
                };
                tabs[i] = m.Name + marker;
            }
            _activeTab = GUILayout.Toolbar(_activeTab, tabs);

            GUILayout.Space(4);
            using (var scroll = new GUILayout.ScrollViewScope(Vector2.zero))
            {
                if (_activeTab < _registry.Modules.Count)
                {
                    var module = _registry.Modules[_activeTab];
                    if (module.Status == ModuleStatus.Broken)
                        GUILayout.Label("This module's patches failed to apply. See BepInEx LogOutput.log.");
                    else
                        module.DrawGui();
                }
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
    }
}
