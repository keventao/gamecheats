using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ClanfolkCheats.Core;
using HarmonyLib;
using MelonLoader;

namespace ClanfolkCheats.Modules
{
    public class ResourceCheats : ICheatModule
    {
        public string Name => "资源";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        private const int SlotCount = 5;

        private readonly string[] _slots = new string[SlotCount];
        private readonly int[] _lockFloors = new int[SlotCount];
        private readonly bool[] _lockEnabled = new bool[SlotCount];
        private int _selectedSlot;
        private List<string> _playerItemKeys = new();
        private Dictionary<string, string> _keyToDisplay = new();
        private Dictionary<string, int> _playerItemCounts = new();
        private string _searchInput = "";
        private int _scrollOffset;
        private bool _showPicker;
        private int _customQty = 10;
        private bool _triedDiscover;
        private int _discoverFrameDelay = 120;
        private int _discoverFrameCounter;
        private string _lastError = "";
        private int _totalItems;

        public void Register(HarmonyLib.Harmony harmony)
        {
            MelonLogger.Msg("[Rsrc] Registered — will discover items when game world loads.");
        }

        public void OnUpdate()
        {
            if (!_triedDiscover)
            {
                _discoverFrameCounter++;
                if (_discoverFrameCounter < _discoverFrameDelay) return;
                _discoverFrameCounter = 0;
                _triedDiscover = true;
                MelonLogger.Msg("[Rsrc] Calling TryDiscover...");
                TryDiscover();
            }
        }

        public void DrawGui(Layout l)
        {
            if (_playerItemKeys.Count == 0) { l.Label("Waiting for game world..."); return; }
            if (!string.IsNullOrEmpty(_lastError)) { l.Label($"Error: {_lastError}", 18f); }

            l.Label($"Total: {_playerItemKeys.Count} types, {_totalItems} items in storage", 20f);
            l.Space(2);

            for (int i = 0; i < SlotCount; i++)
            {
                var key = _slots[i];
                var display = string.IsNullOrEmpty(key) ? "empty" : GetDisplayName(key);
                var cnt = !string.IsNullOrEmpty(key) && _playerItemCounts.TryGetValue(key, out var c) ? c : 0;
                l.Label($"[{i + 1}: {display}]  x{cnt}", 20f);

                float bx = l.X + 220f;
                if (ImguiUtil.Button(new Rect(bx, l.Y - 22f, 36f, 22f), "+"))
                    GiveItem(key, 1);
                if (ImguiUtil.Button(new Rect(bx + 40f, l.Y - 22f, 36f, 22f), $"+{_customQty}"))
                    GiveItem(key, _customQty);
                if (ImguiUtil.Button(new Rect(bx + 80f, l.Y - 22f, 45f, 22f), "Pick"))
                { _selectedSlot = i; _showPicker = true; _scrollOffset = 0; _searchInput = ""; }
                if (ImguiUtil.Button(new Rect(bx + 129f, l.Y - 22f, 45f, 22f), _lockEnabled[i] ? "Unlock" : "Lock"))
                    _lockEnabled[i] = !_lockEnabled[i];
                if (_lockEnabled[i])
                {
                    GUI.Label(new Rect(bx + 178f, l.Y - 22f, 36f, 22f), $">={_lockFloors[i]}");
                    _lockFloors[i] = ClampIntInput(bx + 210f, l.Y - 22f, _lockFloors[i]);
                }
            }

            l.Space(2);
            l.Label($"Custom qty: {_customQty}", 20f);
            _customQty = ClampIntInput(l.X + 120f, l.Y - 20f, _customQty, 1, 9999);
            l.Y += 4f;

            if (l.Button("Fill All Slots", 24f))
                for (int i = 0; i < SlotCount; i++)
                    GiveItem(_slots[i], _customQty);

            l.Space(4);
            if (_showPicker) DrawPicker(l);
        }

        private void TryDiscover()
        {
            try
            {
                var gm = GameRefs.GetGameManager();
                if (gm == null) { MelonLogger.Msg("[Rsrc] GameManager null"); _triedDiscover = false; return; }

                var gmType = gm.GetType();
                MelonLogger.Msg($"[Rsrc] GameManager type: {gmType.FullName}");
                
                var ecType = gmType.Assembly.GetType("Il2Cpp.EntityClass");
                if (ecType == null || !ecType.IsEnum) { MelonLogger.Warning($"[Rsrc] EntityClass not found (null={ecType == null})"); return; }

                var itemEC = Enum.Parse(ecType, "Item");
                var getEM = gmType.GetMethod("GetEntityManager", new Type[] { ecType });
                if (getEM == null) { MelonLogger.Warning("[Rsrc] GetEntityManager not found"); return; }

                var em = getEM.Invoke(gm, new object[] { itemEC });
                if (em == null) { MelonLogger.Msg("[Rsrc] EntityManager null"); _triedDiscover = false; return; }

                var emType = em.GetType();
                MelonLogger.Msg($"[Rsrc] EntityManager type: {emType.FullName}");

                _playerItemKeys.Clear();
                _keyToDisplay.Clear();
                _playerItemCounts.Clear();

                // Always get prefab list first (all possible items)
                var ga = emType.GetMethod("GetPrefabArray", Type.EmptyTypes);
                if (ga != null)
                {
                    var arr = ga.Invoke(em, null);
                    if (arr is System.Collections.IEnumerable iterable)
                    {
                        int count = 0;
                        foreach (var entity in iterable)
                        {
                            if (entity == null) continue;
                            var t = entity.GetType();
                            var etField = t.GetField("myEntityType");
                            var key = etField?.GetValue(entity) as string;
                            if (!string.IsNullOrEmpty(key) && !_playerItemCounts.ContainsKey(key))
                            {
                                _playerItemKeys.Add(key);
                                _playerItemCounts[key] = 0;
                                var dnField = t.GetField("displayName");
                                var display = dnField?.GetValue(entity) as string;
                                _keyToDisplay[key] = !string.IsNullOrEmpty(display) ? display : key;
                                count++;
                            }
                        }
                        MelonLogger.Msg($"[Rsrc] PrefabArray: {count} items");
                    }
                }
                else
                {
                    MelonLogger.Warning("[Rsrc] GetPrefabArray method not found");
                }

                // Then scan actual entities for inventory counts
                var getAllMethod = emType.GetMethod("GetAllEntityList", Type.EmptyTypes);
                if (getAllMethod != null)
                {
                    var allList = getAllMethod.Invoke(em, null) as System.Collections.IList;
                    if (allList != null)
                    {
                        MelonLogger.Msg($"[Rsrc] EntityList: {allList.Count} entities");
                        foreach (var entity in allList)
                        {
                            if (entity == null) continue;
                            var t = entity.GetType();
                            var etField = t.GetField("myEntityType");
                            var key = etField?.GetValue(entity) as string;
                            if (!string.IsNullOrEmpty(key) && _playerItemCounts.ContainsKey(key))
                            {
                                _playerItemCounts[key]++;
                            }
                        }
                    }
                }

                if (_playerItemKeys.Count > 0)
                {
                    _totalItems = 0;
                    foreach (var c in _playerItemCounts.Values) _totalItems += c;

                    Status = ModuleStatus.Ok;
                    for (int i = 0; i < SlotCount && i < _playerItemKeys.Count; i++)
                        _slots[i] = _playerItemKeys[i];
                    MelonLogger.Msg($"[Rsrc] Found {_playerItemKeys.Count} types, {_totalItems} total items");
                }
                else
                {
                    MelonLogger.Warning("[Rsrc] No items found");
                    _triedDiscover = false;
                }
            }
            catch (Exception ex) { _lastError = ex.Message; MelonLogger.Error($"[Rsrc] {ex.Message}"); _triedDiscover = false; }
        }

        private void GiveItem(string name, int count)
        {
            if (string.IsNullOrEmpty(name) || count <= 0) return;
            try
            {
                var gm = GameRefs.GetGameManager();
                if (gm == null) return;

                var cam = Camera.main;
                var pos = cam != null
                    ? cam.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 10f))
                    : Vector3.zero;

                var gmType = gm.GetType();
                var spawnBatchMethod = gmType.GetMethod("SpawnEntitiesAtPosition");
                if (spawnBatchMethod != null)
                {
                    try { spawnBatchMethod.Invoke(gm, new object[] { name, pos, count, true, 1f, false, true }); return; }
                    catch { }
                }

                var spawnMethod = gmType.GetMethod("SpawnEntity", new Type[] { typeof(string), typeof(Vector3), typeof(Quaternion) });
                if (spawnMethod != null)
                {
                    for (int i = 0; i < count && i < 200; i++)
                    {
                        var p = pos + new Vector3((i % 20) * 0.5f, 0f, (i / 20) * 0.5f);
                        spawnMethod.Invoke(gm, new object[] { name, p, Quaternion.identity });
                    }
                    return;
                }
            }
            catch (Exception ex) { MelonLogger.Warning($"[Rsrc] GiveItem({name}): {ex.Message}"); }
        }

        private void DrawPicker(Layout l)
        {
            l.Label($"Pick item for Slot {_selectedSlot + 1}:", 22f);

            var filtered = new List<string>();
            foreach (var key in _playerItemKeys)
            {
                var display = GetDisplayName(key);
                if (string.IsNullOrEmpty(_searchInput)
                    || display.ToLowerInvariant().Contains(_searchInput.ToLowerInvariant())
                    || key.ToLowerInvariant().Contains(_searchInput.ToLowerInvariant()))
                    filtered.Add(key);
            }

            var sr = new Rect(l.X, l.Y, l.Width, 22f);
            GUI.Box(sr, string.IsNullOrEmpty(_searchInput) ? "Type to search..." : _searchInput);
            l.Y += 24f;

            var ev = Event.current;
            if (ev != null && sr.Contains(ev.mousePosition))
            {
                if (ev.type == EventType.KeyDown)
                {
                    if (ev.keyCode == KeyCode.Backspace && _searchInput.Length > 0)
                        _searchInput = _searchInput[..^1];
                    else if (ev.keyCode == KeyCode.Return || ev.keyCode == KeyCode.KeypadEnter)
                    { if (filtered.Count > 0) { _slots[_selectedSlot] = filtered[0]; _showPicker = false; } }
                    else if (ev.keyCode == KeyCode.Escape)
                        _showPicker = false;
                    else if (ev.character != 0 && !char.IsControl(ev.character) && _searchInput.Length < 40)
                        _searchInput += ev.character;
                    if (ev.type == EventType.KeyDown) { ev.Use(); _scrollOffset = 0; }
                }
            }

            const int vr = 8;
            int mx = Math.Max(0, filtered.Count - vr);
            _scrollOffset = Math.Clamp(_scrollOffset, 0, mx);

            if (ImguiUtil.Button(new Rect(l.X + l.Width - 24f, l.Y, 24f, 20f), "▲") && _scrollOffset > 0) _scrollOffset--;
            if (ImguiUtil.Button(new Rect(l.X + l.Width - 24f, l.Y + vr * 22f, 24f, 20f), "▼") && _scrollOffset < mx) _scrollOffset++;

            for (int i = 0; i < vr && (_scrollOffset + i) < filtered.Count; i++)
            {
                int idx = _scrollOffset + i;
                var key = filtered[idx];
                var display = GetDisplayName(key);
                if (ImguiUtil.Button(new Rect(l.X, l.Y + i * 22f, l.Width - 28f, 20f), $"{display} ({key})"))
                { _slots[_selectedSlot] = key; _showPicker = false; }
            }
            l.Y += (vr + 1) * 22f;
            if (ImguiUtil.Button(new Rect(l.X, l.Y, 60f, 22f), "Close")) _showPicker = false;
            l.Y += 28f;
        }

        private string GetDisplayName(string key)
        {
            if (_keyToDisplay.TryGetValue(key, out var dn) && !string.IsNullOrEmpty(dn) && dn != key)
                return dn;
            return key;
        }

        private static int ClampIntInput(float x, float y, int current, int min = 0, int max = 9999)
        {
            var r = new Rect(x, y, 40f, 20f);
            GUI.Box(r, current.ToString());
            var ev = Event.current;
            if (ev == null || !r.Contains(ev.mousePosition)) return current;
            if (ev.type == EventType.ScrollWheel)
            {
                ev.Use();
                return Math.Clamp(current + (ev.delta.y > 0 ? -1 : 1), min, max);
            }
            return current;
        }
    }
}
