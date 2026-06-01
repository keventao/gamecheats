using UnityEngine;

namespace ClanfolkCheats.Core
{
    public class GuiManager
    {
        private readonly ModuleRegistry _registry;
        private bool _open;
        private int  _activeTab;
        private Rect _windowRect = new Rect(40, 40, 480, 540);
        private int  _lastToggleFrame = -1;
        private bool _dragging;
        private Vector2 _dragOffset;
        private CursorLockMode _prevLock;
        private bool          _prevCursorVisible;

        public GuiManager(ModuleRegistry registry)
        {
            _registry = registry;
        }

        public void OnGUI()
        {
            var ev = Event.current;

            if (ev != null && ev.type == EventType.KeyDown && ev.keyCode == KeyCode.F1
                && UnityEngine.Time.frameCount != _lastToggleFrame)
            {
                _lastToggleFrame = UnityEngine.Time.frameCount;
                if (!_open)
                {
                    _prevLock = Cursor.lockState;
                    _prevCursorVisible = Cursor.visible;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = _prevLock;
                    Cursor.visible = _prevCursorVisible;
                    _dragging = false;
                }
                _open = !_open;
                ev.Use();
            }

            var tagRect = new Rect(8, 8, 380, 22);
            var prev = GUI.color;
            GUI.color = Color.yellow;
            GUI.Box(tagRect, $"Clanfolk Cheats v{Plugin.Version} — F1 toggle | open={_open}");
            GUI.color = prev;

            if (!_open) return;

            GUI.Box(_windowRect, "");
            const float titleH = 22f;
            var titleRect = new Rect(_windowRect.x, _windowRect.y, _windowRect.width, titleH);
            var titlePrev = GUI.color;
            GUI.color = new Color(0.2f, 0.5f, 0.9f, 1f);
            GUI.Box(titleRect, "Clanfolk Cheats");
            GUI.color = titlePrev;

            if (ev != null)
            {
                if (ev.type == EventType.MouseDown && ev.button == 0
                    && titleRect.Contains(ev.mousePosition))
                {
                    _dragging = true;
                    _dragOffset = new Vector2(_windowRect.x - ev.mousePosition.x,
                                              _windowRect.y - ev.mousePosition.y);
                    ev.Use();
                }
                else if (_dragging && ev.type == EventType.MouseDrag)
                {
                    _windowRect.x = ev.mousePosition.x + _dragOffset.x;
                    _windowRect.y = ev.mousePosition.y + _dragOffset.y;
                    ev.Use();
                }
                else if (_dragging && (ev.type == EventType.MouseUp
                                       || ev.type == EventType.Ignore))
                {
                    _dragging = false;
                }
            }

            GUI.Label(new Rect(_windowRect.xMax - 90, _windowRect.y + 2, 80, 20),
                GameRefs.IsReady ? "● In Game" : "○ Menu");

            const float pad = 10f;
            float contentX = _windowRect.x + pad;
            float contentY = _windowRect.y + titleH + 6f;
            float contentW = _windowRect.width - 2f * pad;

            if (_registry.Modules.Count == 0)
            {
                GUI.Label(new Rect(contentX, contentY, contentW, 22), "No modules registered.");
                return;
            }

            const float tabH = 28f;
            float tabW = (contentW - (_registry.Modules.Count - 1) * 4f) / _registry.Modules.Count;
            for (int i = 0; i < _registry.Modules.Count; i++)
            {
                var m = _registry.Modules[i];
                var label = m.Name + (m.Status == ModuleStatus.Broken ? " (!)" : "");
                var r = new Rect(contentX + i * (tabW + 4f), contentY, tabW, tabH);
                if (ImguiUtil.Button(r, label, i == _activeTab))
                    _activeTab = i;
            }

            float bodyY = contentY + tabH + 8f;
            if (_activeTab < _registry.Modules.Count)
            {
                var mod = _registry.Modules[_activeTab];
                var sub = new Layout(contentX, bodyY, contentW);
                if (mod.Status == ModuleStatus.Broken)
                    sub.Label("Module patches failed. Check MelonLoader console.");
                else
                    mod.DrawGui(sub);
            }
        }
    }
}
