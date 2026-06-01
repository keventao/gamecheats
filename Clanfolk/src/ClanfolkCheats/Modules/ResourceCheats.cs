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

        private readonly string[] _slots = new string[5];
        private int _selectedSlot;
        private List<string> _allItemKeys = new();
        private Dictionary<string, string> _keyToDisplay = new(); // internalName -> displayName
        private string _searchInput = "";
        private int _scrollOffset;
        private bool _showPicker;
        private Type? _gameManagerType;
        private object? _clanSetupMgr;
        private object? _itemManagerInst;

        private const string GameManagerTypeName = "Il2Cpp.GameManager";

        public void Register(HarmonyLib.Harmony harmony)
        {
            try
            {
                _gameManagerType = AccessTools.TypeByName(GameManagerTypeName);
                if (_gameManagerType == null) { MelonLogger.Error("[Rsrc] GameManager not found"); return; }

                var getItemMgr = AccessTools.Method(_gameManagerType, "GetItemManager");
                if (getItemMgr != null)
                    _itemManagerInst = getItemMgr.Invoke(null, null);

                var getClanMgr = AccessTools.Method(_gameManagerType, "GetClanSetupManager");
                if (getClanMgr != null)
                    _clanSetupMgr = getClanMgr.Invoke(null, null);

                DiscoverAllItemNames();

                if (_allItemKeys.Count == 0)
                {
                    MelonLogger.Warning("[Rsrc] No items discovered, using fallback");
                    _allItemKeys = new List<string> { "Wood", "Stone", "Iron", "Branches", "Water", "Food", "Fish", "Wool", "Flax", "Log" };
                    foreach (var k in _allItemKeys) _keyToDisplay[k] = k;
                }

                for (int i = 0; i < _slots.Length && i < _allItemKeys.Count; i++)
                    _slots[i] = _allItemKeys[i];

                Status = ModuleStatus.Ok;
                MelonLogger.Msg($"[Rsrc] OK. {_allItemKeys.Count} items, clanMgr={_clanSetupMgr != null}, itemMgr={_itemManagerInst != null}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Rsrc] Init failed: {ex}");
            }
        }

        public void DrawGui(Layout l)
        {
            if (_allItemKeys.Count == 0) { l.Label("Waiting for game world..."); return; }

            for (int i = 0; i < _slots.Length; i++)
            {
                var key = _slots[i];
                var display = string.IsNullOrEmpty(key) ? "empty" : GetDisplayName(key);
                l.Label($"[{i + 1}: {display}]", 20f);

                float bx = l.X + 220f;
                if (ImguiUtil.Button(new Rect(bx, l.Y - 22f, 45f, 22f), "+5"))
                    GiveItem(key, 5);
                if (ImguiUtil.Button(new Rect(bx + 50f, l.Y - 22f, 50f, 22f), "+50"))
                    GiveItem(key, 50);
                if (ImguiUtil.Button(new Rect(bx + 105f, l.Y - 22f, 45f, 22f), "Pick"))
                {
                    _selectedSlot = i; _showPicker = true; _scrollOffset = 0; _searchInput = "";
                }
            }

            l.Space(4);
            if (_showPicker) DrawPicker(l);
        }

        private void DrawPicker(Layout l)
        {
            l.Label($"Pick item for Slot {_selectedSlot + 1}:", 22f);

            // filter by search input (match against display name or internal key)
            var filtered = new List<string>();
            foreach (var key in _allItemKeys)
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

        private void GiveItem(string name, int count)
        {
            if (string.IsNullOrEmpty(name)) return;

            try
            {
                // Primary: ItemManager.SpawnItem at camera center or mouse position
                if (_itemManagerInst != null && TrySpawnItemViaMgr(name, count)) { MelonLogger.Msg($"[Rsrc] +{count} {name} (ItemManager)"); return; }
                // Fallback: GameManager.SpawnEntity on map
                if (TrySpawnEntity(name, count)) { MelonLogger.Msg($"[Rsrc] +{count} {name} (SpawnEntity)"); return; }
                MelonLogger.Warning($"[Rsrc] All methods failed for '{name}'");
            }
            catch (Exception ex) { MelonLogger.Error($"[Rsrc] GiveItem({name}): {ex.Message}"); }
        }

        private bool TrySpawnItemViaMgr(string objectType, int count)
        {
            try
            {
                // Get world position from camera center
                var cam = Camera.main;
                Vector3 pos;
                if (cam != null)
                {
                    pos = cam.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, cam.nearClipPlane + 5f));
                }
                else
                {
                    pos = Vector3.zero;
                }

                var spawnMethod = AccessTools.Method(_itemManagerInst!.GetType(), "SpawnItem");
                if (spawnMethod == null) return false;

                for (int i = 0; i < count && i < 50; i++)
                {
                    var p = pos + new Vector3(i % 10 * 0.5f, 0f, i / 10 * 0.5f);
                    spawnMethod.Invoke(_itemManagerInst, new object[] { objectType, p, Quaternion.identity, 0UL });
                }
                return true;
            }
            catch (Exception ex) { MelonLogger.Msg($"[Rsrc] ItemManager.SpawnItem failed: {ex.Message}"); return false; }
        }

        private bool TryAddInvEntity(string entType, int count)
        {
            try
            {
                var wmMgrType = AccessTools.TypeByName("Il2Cpp.WorldMapManager");
                if (wmMgrType == null) return false;
                var getWM = AccessTools.Method(_gameManagerType!, "GetWorldMapManager");
                if (getWM == null) return false;
                var wm = getWM.Invoke(null, null);
                if (wm == null) return false;

                var ecType = AccessTools.TypeByName("Il2Cpp.EntityClass");
                var itemEC = Enum.Parse(ecType!, "Item");
                var addMethod = AccessTools.Method(wmMgrType, "AddInvEntity");
                if (addMethod == null) return false;

                addMethod.Invoke(wm, new object[] { itemEC, entType, count, 1f });
                return true;
            }
            catch { return false; }
        }

        private bool TrySpawnEntity(string entType, int count)
        {
            try
            {
                var methods = _gameManagerType!.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                MethodInfo? best = null;
                foreach (var m in methods)
                {
                    if (m.Name != "SpawnEntity" || m.IsGenericMethod) continue;
                    var p = m.GetParameters();
                    if (p.Length == 3 && p[0].ParameterType == typeof(string) && p[1].ParameterType == typeof(Vector3) && p[2].ParameterType == typeof(Quaternion))
                    { best = m; break; }
                }
                if (best == null) return false;

                var pos = Camera.main != null
                    ? Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 10f))
                    : Vector3.zero;

                for (int i = 0; i < count && i < 50; i++)
                {
                    pos.x += 0.5f; pos.z += 0.3f;
                    best.Invoke(null, new object[] { entType, pos, Quaternion.identity });
                }
                return true;
            }
            catch { return false; }
        }

        private void DiscoverAllItemNames()
        {
            try
            {
                var ecType = AccessTools.TypeByName("Il2Cpp.EntityClass");
                if (ecType == null || !ecType.IsEnum) { MelonLogger.Warning("[Rsrc] EntityClass enum not found"); return; }

                var itemEC = Enum.Parse(ecType, "Item");
                var getEM = AccessTools.Method(_gameManagerType!, "GetEntityManager", new Type[] { ecType });
                if (getEM == null) { MelonLogger.Warning("[Rsrc] GetEntityManager(EntityClass) not found"); return; }

                var em = getEM.Invoke(null, new object[] { itemEC });
                if (em == null) { MelonLogger.Warning("[Rsrc] EntityManager(Item) is null — world not loaded?"); return; }

                var ga = AccessTools.Method(em.GetType(), "GetPrefabArray");
                if (ga == null) { MelonLogger.Warning("[Rsrc] GetPrefabArray not found"); return; }

                var arr = ga.Invoke(em, null);
                if (arr is not System.Collections.IEnumerable iterable) { MelonLogger.Warning("[Rsrc] PrefabArray is not enumerable"); return; }

                var entityTypeType = AccessTools.TypeByName("Il2Cpp.Entity");

                foreach (var entity in iterable)
                {
                    if (entity == null) continue;

                    // Get internal type name (myEntityType field)
                    string? key = null;
                    var myTypeField = entity.GetType().GetField("myEntityType", BindingFlags.Public | BindingFlags.Instance);
                    if (myTypeField != null)
                        key = myTypeField.GetValue(entity) as string;

                    // Get display name
                    string? display = null;
                    var dispField = entity.GetType().GetField("displayName", BindingFlags.Public | BindingFlags.Instance);
                    if (dispField != null)
                        display = dispField.GetValue(entity) as string;

                    if (!string.IsNullOrEmpty(key))
                    {
                        _allItemKeys.Add(key);
                        _keyToDisplay[key] = !string.IsNullOrEmpty(display) ? display : key;
                    }
                }

                MelonLogger.Msg($"[Rsrc] Discovered {_allItemKeys.Count} items from EntityManager(Item)");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Rsrc] Discovery failed: {ex.Message}");
            }
        }
    }
}
