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
        private Rect _panelRect;
        private int _lastToggleFrame = -1;
        private bool _dragging;
        private Vector2 _dragOffset;
        private string _selectedCategory = "";
        private string _search = "";
        private SortKey _sort = SortKey.Name;

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

            // Top bar: search + sort
            var barRect = new Rect(x, y, w, 24);
            GUI.Label(new Rect(barRect.x, barRect.y, 40, 24), "搜索");
            _search = GUI.TextField(new Rect(barRect.x + 42, barRect.y, 160, 24), _search ?? "");
            if (GUI.Button(new Rect(barRect.x + 210, barRect.y, 90, 24), $"排序:{_sort}"))
                _sort = (SortKey)(((int)_sort + 1) % 3);
            y += 28;

            // Category sidebar (left) + module content (right)
            var categories = _registry.Categories();
            if (_selectedCategory == "" && categories.Count > 0) _selectedCategory = categories[0];

            float sidebarW = 96f;
            float contentX = x + sidebarW + 6;
            float contentW = w - sidebarW - 6;
            float areaH = _panelRect.height - (y - _panelRect.y) - 8;

            for (int i = 0; i < categories.Count; i++)
            {
                var cat = categories[i];
                var rc = new Rect(x, y + i * 26, sidebarW, 24);
                var catPrev = GUI.color;
                if (cat == _selectedCategory) GUI.color = new Color(0.3f, 0.6f, 1f);
                if (GUI.Button(rc, cat)) _selectedCategory = cat;
                GUI.color = catPrev;
            }

            // Modules in the selected category, filtered + sorted by name
            var inCat = new System.Collections.Generic.List<ICheatModule>();
            foreach (var m in _registry.Modules)
                if (m.Category == _selectedCategory) inCat.Add(m);
            var shown = FilterSort.Apply(inCat, _search, _sort, m => m.Name, m => m.Category, _ => 0);

            var contentRect = new Rect(contentX, y, contentW, areaH);
            GUI.BeginGroup(contentRect);
            float my = 0;
            foreach (var module in shown)
            {
                var marker = module.Status switch
                {
                    ModuleStatus.Broken   => " (!)",
                    ModuleStatus.Disabled => " (off)",
                    _ => "",
                };
                GUI.Label(new Rect(0, my, contentW, 20), $"▼ {module.Name}{marker}");
                my += 22;
                if (module.Status == ModuleStatus.Broken)
                {
                    GUI.Label(new Rect(8, my, contentW - 8, 20), "patches failed — see LogOutput.log");
                    my += 22;
                }
                else
                {
                    // Each module draws into its own sub-group starting at (8, my).
                    var sub = new Rect(8, my, contentW - 12, areaH - my);
                    if (sub.height < 40) break;
                    GUI.BeginGroup(sub);
                    module.DrawGui();
                    GUI.EndGroup();
                    my += EstimateModuleHeight(module);
                }
                my += 8;
            }
            GUI.EndGroup();

            _config.PanelWidth.Value  = (int)_panelRect.width;
            _config.PanelHeight.Value = (int)_panelRect.height;
        }

        private static float EstimateModuleHeight(ICheatModule module) => 180f;
    }
}
