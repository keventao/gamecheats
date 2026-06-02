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
        private static readonly Dictionary<string, string> ZhNames = new()
        {
            { "ItemWood", "木材" }, { "Wood", "木材" }, { "Log", "原木" }, { "Plank", "木板" },
            { "ItemStone", "石头" }, { "Stone", "石头" }, { "Rock", "岩石" }, { "Flint", "燧石" },
            { "ItemIron", "铁" }, { "Iron", "铁" }, { "IronOre", "铁矿石" }, { "IronIngot", "铁锭" },
            { "ItemCopper", "铜" }, { "Copper", "铜" }, { "CopperOre", "铜矿石" },
            { "ItemGold", "金" }, { "Gold", "金" }, { "GoldOre", "金矿石" }, { "GoldIngot", "金锭" },
            { "ItemCoal", "煤" }, { "Coal", "煤" },
            { "ItemClay", "黏土" }, { "Clay", "黏土" },
            { "ItemSand", "沙" }, { "Sand", "沙" },
            { "ItemFiber", "纤维" }, { "Fiber", "纤维" },
            { "ItemWool", "羊毛" }, { "Wool", "羊毛" },
            { "ItemLeather", "皮革" }, { "Leather", "皮革" },
            { "ItemHide", "皮" }, { "Hide", "皮" },
            { "ItemStraw", "稻草" }, { "Straw", "稻草" }, { "Hay", "干草" },
            { "ItemThatch", "茅草" }, { "Thatch", "茅草" },
            { "ItemWheat", "小麦" }, { "Wheat", "小麦" },
            { "ItemFlour", "面粉" }, { "Flour", "面粉" },
            { "ItemBread", "面包" }, { "Bread", "面包" },
            { "ItemMeat", "肉" }, { "Meat", "肉" }, { "RawMeat", "生肉" }, { "CookedMeat", "熟肉" },
            { "ItemFish", "鱼" }, { "Fish", "鱼" },
            { "ItemBerry", "浆果" }, { "Berry", "浆果" }, { "Berries", "浆果" },
            { "ItemFruit", "水果" }, { "Fruit", "水果" },
            { "ItemVegetable", "蔬菜" }, { "Vegetable", "蔬菜" },
            { "ItemGrain", "谷物" }, { "Grain", "谷物" },
            { "ItemWater", "水" }, { "Water", "水" }, { "FreshWater", "淡水" },
            { "ItemMud", "泥" }, { "Mud", "泥" },
            { "ItemReed", "芦苇" }, { "Reed", "芦苇" },
            { "ItemThread", "线" }, { "Thread", "线" }, { "String", "线绳" },
            { "ItemCloth", "布" }, { "Cloth", "布" }, { "Linen", "亚麻布" },
            { "ItemMilk", "牛奶" }, { "Milk", "牛奶" },
            { "ItemEgg", "蛋" }, { "Egg", "蛋" },
            { "ItemSeed", "种子" }, { "Seed", "种子" }, { "Seeds", "种子" },
            { "ItemSapling", "树苗" }, { "Sapling", "树苗" },
            { "ItemTool", "工具" }, { "Tool", "工具" },
            { "ItemAxe", "斧" }, { "Axe", "斧" }, { "StoneAxe", "石斧" }, { "IronAxe", "铁斧" },
            { "ItemPickaxe", "镐" }, { "Pickaxe", "镐" }, { "Pick", "镐" },
            { "ItemShovel", "铲" }, { "Shovel", "铲" },
            { "ItemSword", "剑" }, { "Sword", "剑" },
            { "ItemBow", "弓" }, { "Bow", "弓" },
            { "ItemArrow", "箭" }, { "Arrow", "箭" },
            { "ItemSpear", "矛" }, { "Spear", "矛" },
            { "ItemShield", "盾" }, { "Shield", "盾" },
            { "ItemArmor", "护甲" }, { "Armor", "护甲" },
            { "ItemPotion", "药水" }, { "Potion", "药水" },
            { "ItemMedicine", "药" }, { "Medicine", "药" },
            { "ItemHerb", "草药" }, { "Herb", "草药" },
            { "ItemFur", "毛皮" }, { "Fur", "毛皮" },
            { "ItemBone", "骨头" }, { "Bone", "骨头" },
            { "ItemAntler", "鹿角" }, { "Antler", "鹿角" },
            { "ItemBrick", "砖" }, { "Brick", "砖" },
            { "ItemMortar", "砂浆" }, { "Mortar", "砂浆" },
            { "ItemGlass", "玻璃" }, { "Glass", "玻璃" },
            { "ItemCharcoal", "木炭" }, { "Charcoal", "木炭" },
            { "ItemTorch", "火把" }, { "Torch", "火把" },
            { "ItemCandle", "蜡烛" }, { "Candle", "蜡烛" },
            { "ItemOil", "油" }, { "Oil", "油" },
            { "ItemHoney", "蜂蜜" }, { "Honey", "蜂蜜" },
            { "ItemSalt", "盐" }, { "Salt", "盐" },
            { "ItemMap", "地图" }, { "Map", "地图" },
            { "ItemBook", "书" }, { "Book", "书" },
            { "ItemCoin", "硬币" }, { "Coin", "硬币" }, { "Money", "钱" },
            { "ItemClothing", "衣服" }, { "Clothing", "衣服" },
        };
        private string _searchInput = "";
        private int _scrollOffset;
        private bool _showPicker;
        private int _customQty = 100;
        private bool _triedDiscover;
        private int _discoverFrameDelay = 120;
        private int _discoverFrameCounter;
        private string _lastError = "";
        private int _totalItems;
        private int _discoverAttempts;

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
            if (_playerItemKeys.Count == 0) { l.Label("等待游戏世界加载…"); return; }
            if (!string.IsNullOrEmpty(_lastError)) { l.Label($"错误: {_lastError}", 18f); }

            l.Label($"共 {_playerItemKeys.Count} 种, {_totalItems} 件物品", 20f);
            l.Space(2);

            for (int i = 0; i < SlotCount; i++)
            {
                var key = _slots[i];
                var display = string.IsNullOrEmpty(key) ? "空" : FormatItemName(key);
                var cnt = !string.IsNullOrEmpty(key) && _playerItemCounts.TryGetValue(key, out var c) ? c : 0;
                l.Label($"[{i + 1}: {display}]  x{cnt}", 20f);

                float bx = l.X + 220f;
                if (ImguiUtil.Button(new Rect(bx, l.Y - 22f, 36f, 22f), "+10"))
                    GiveItem(key, 10);
                if (ImguiUtil.Button(new Rect(bx + 40f, l.Y - 22f, 36f, 22f), $"+{_customQty}"))
                    GiveItem(key, _customQty);
                if (ImguiUtil.Button(new Rect(bx + 80f, l.Y - 22f, 45f, 22f), "选择"))
                { _selectedSlot = i; _showPicker = true; _scrollOffset = 0; _searchInput = ""; }
                if (ImguiUtil.Button(new Rect(bx + 129f, l.Y - 22f, 45f, 22f), _lockEnabled[i] ? "解锁" : "锁定"))
                    _lockEnabled[i] = !_lockEnabled[i];
                if (_lockEnabled[i])
                {
                    GUI.Label(new Rect(bx + 178f, l.Y - 22f, 36f, 22f), $">={_lockFloors[i]}");
                    _lockFloors[i] = ClampIntInput(bx + 210f, l.Y - 22f, _lockFloors[i]);
                }
            }

            l.Space(2);
            l.Label($"自定义数量: {_customQty}", 20f);
            _customQty = ClampIntInput(l.X + 120f, l.Y - 20f, _customQty, 1, 9999);
            l.Y += 4f;

            if (l.Button("全部填充", 24f))
                for (int i = 0; i < SlotCount; i++)
                    GiveItem(_slots[i], _customQty);

            l.Space(4);
            if (_showPicker) DrawPicker(l);
        }

        private void TryDiscover()
        {
            try
            {
                _discoverAttempts++;
                var gm = GameRefs.GetGameManager();
                if (gm == null) { RetryLater("GameManager null"); return; }

                var gmType = gm.GetType();
                MelonLogger.Msg($"[Rsrc] GameManager type: {gmType.FullName}");
                
                var ecType = gmType.Assembly.GetType("Il2Cpp.EntityClass")
                    ?? gmType.Assembly.GetType("EntityClass")
                    ?? GameRefs.ResolveType("EntityClass");
                if (ecType == null || !ecType.IsEnum) { RetryLater($"EntityClass not found (null={ecType == null})"); return; }

                var itemEC = Enum.Parse(ecType, "Item");
                var getEM = gmType.GetMethod("GetEntityManager", new Type[] { ecType });
                if (getEM == null) { RetryLater("GetEntityManager not found"); return; }

                var em = getEM.Invoke(gm, new object[] { itemEC });
                if (em == null) { RetryLater("EntityManager null"); return; }

                var emType = em.GetType();
                _itemEntityManager = em;   // kept for GetEntityTypeToken -> GetText fallback
                MelonLogger.Msg($"[Rsrc] EntityManager type: {emType.FullName}");

                _playerItemKeys.Clear();
                _keyToDisplay.Clear();
                _playerItemCounts.Clear();

                var itemManager = InvokeStatic(gmType, "GetItemManager");
                int prefabCount = AddPrefabs(itemManager, "ItemManager.GetLoadedPrefabArray");
                if (prefabCount == 0)
                    prefabCount += AddPrefabs(em, "EntityManager.GetLoadedPrefabArray");
                if (prefabCount == 0)
                    prefabCount += AddPrefabs(em, "EntityManager.GetPrefabArray");
                if (prefabCount == 0)
                    prefabCount += AddPrefabLookup(em, "myEntityPrefabLookup");
                MelonLogger.Msg($"[Rsrc] Prefab discovery: {prefabCount} item types");

                // Then scan actual entities for inventory counts
                var getAllMethod = emType.GetMethod("GetAllEntityList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (getAllMethod != null)
                {
                    var allList = getAllMethod.Invoke(em, null) as System.Collections.IList;
                    if (allList != null)
                    {
                        MelonLogger.Msg($"[Rsrc] EntityList: {allList.Count} entities");
                        foreach (var entity in allList)
                        {
                            if (entity == null) continue;
                            var key = GetEntityKey(entity);
                            if (!string.IsNullOrEmpty(key))
                            {
                                if (!_playerItemCounts.ContainsKey(key))
                                    AddItemKey(key, GetEntityDisplayName(entity));
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
                    RetryLater("No items found");
                }
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                MelonLogger.Error($"[Rsrc] {ex.Message}");
                RetryLater("exception during discovery");
            }
        }

        private void RetryLater(string reason)
        {
            _triedDiscover = false;
            if (_discoverAttempts == 1 || _discoverAttempts % 30 == 0)
                MelonLogger.Msg($"[Rsrc] Waiting for game world: {reason} (attempt {_discoverAttempts})");
        }

        private int AddPrefabs(object? manager, string source)
        {
            if (manager == null) return 0;

            foreach (var methodName in new[] { "GetLoadedPrefabArray", "GetPrefabArray" })
            {
                if (!source.EndsWith(methodName, StringComparison.Ordinal)) continue;

                var method = manager.GetType().GetMethod(
                    methodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    Type.EmptyTypes,
                    null);
                if (method == null) continue;

                var added = AddEntities(method.Invoke(manager, null) as System.Collections.IEnumerable);
                MelonLogger.Msg($"[Rsrc] {source}: {added}");
                return added;
            }

            return 0;
        }

        private int AddPrefabLookup(object manager, string memberName)
        {
            var lookup = GetMemberValue(manager, memberName);
            if (lookup == null) return 0;

            var count = 0;
            var keys = GetMemberValue(lookup, "Keys") as System.Collections.IEnumerable;
            if (keys != null)
            {
                foreach (var rawKey in keys)
                {
                    var key = rawKey as string;
                    if (!string.IsNullOrEmpty(key) && AddItemKey(key, key))
                        count++;
                }
            }

            if (count > 0)
                MelonLogger.Msg($"[Rsrc] {memberName}.Keys: {count}");
            return count;
        }

        private int AddEntities(System.Collections.IEnumerable? entities)
        {
            if (entities == null) return 0;

            var count = 0;
            foreach (var entity in entities)
            {
                if (entity == null) continue;
                var key = GetEntityKey(entity);
                if (!string.IsNullOrEmpty(key) && AddItemKey(key, GetEntityDisplayName(entity)))
                    count++;
            }
            return count;
        }

        private bool AddItemKey(string key, string? display)
        {
            if (_playerItemCounts.ContainsKey(key)) return false;
            _playerItemKeys.Add(key);
            _playerItemCounts[key] = 0;
            var resolved = !string.IsNullOrEmpty(display) && display != key
                ? display!
                : ResolveZhName(key);
            _keyToDisplay[key] = resolved;
            return true;
        }

        private static string ResolveZhName(string key)
        {
            var zh = TryGameLocalize(key) ?? TryTokenLocalize(key);
            if (!string.IsNullOrEmpty(zh) && zh != key)
                return zh;
            if (ZhNames.TryGetValue(key, out var mapped))
                return mapped;
            foreach (var kvp in ZhNames)
            {
                if (key.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                    return kvp.Value;
            }
            return key;
        }

        // Key-only fallback: GameManager.GetTextBible().GetText(key, GrowthStage.NONE).
        // Used when discovery only yielded a string key (no live Entity to call
        // GetEntityName on, e.g. the prefab-lookup path). GetText returns the
        // current-language text for a text-bible key, or echoes the key if unknown.
        private static MethodInfo? _getTextMethod;
        private static object? _getTextDefaultFlag;
        private static object? _itemEntityManager;
        private static MethodInfo? _getTokenMethod;

        // Second key-only fallback: EntityManager.GetEntityTypeToken(key) -> string[],
        // then GetText on each token, returning the first that localizes. The name token
        // is conventionally first; iterating in order returns it. Used only when neither
        // GetEntityName nor a direct GetText(key) produced a name.
        private static string? TryTokenLocalize(string key)
        {
            try
            {
                var em = _itemEntityManager;
                if (em == null) return null;

                if (_getTokenMethod == null)
                    _getTokenMethod = em.GetType().GetMethod(
                        "GetEntityTypeToken",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                        null,
                        new[] { typeof(string) },
                        null);
                if (_getTokenMethod == null) return null;

                var tokens = _getTokenMethod.Invoke(em, new object[] { key }) as System.Collections.IEnumerable;
                if (tokens == null) return null;

                foreach (var t in tokens)
                {
                    var token = t as string;
                    if (string.IsNullOrEmpty(token)) continue;
                    var localized = TryGameLocalize(token);
                    if (!string.IsNullOrEmpty(localized) && localized != token && localized != key)
                        return localized;
                }
            }
            catch { }
            return null;
        }

        private static string? TryGameLocalize(string key)
        {
            try
            {
                var gm = GameRefs.GetGameManager();
                if (gm == null) return null;
                var gmType = gm.GetType();

                var getTB = gmType.GetMethod("GetTextBible", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (getTB == null) return null;

                var textBible = getTB.Invoke(null, null);
                if (textBible == null) return null;

                if (_getTextMethod == null)
                {
                    foreach (var m in textBible.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        if (m.Name != "GetText") continue;
                        var ps = m.GetParameters();
                        if (ps.Length >= 1 && ps[0].ParameterType == typeof(string))
                        {
                            _getTextMethod = m;
                            // second param is the GrowthStage enum (default NONE == 0)
                            _getTextDefaultFlag = ps.Length >= 2 && ps[1].ParameterType.IsEnum
                                ? Enum.ToObject(ps[1].ParameterType, 0)
                                : null;
                            break;
                        }
                    }
                }
                if (_getTextMethod == null) return null;

                var pc = _getTextMethod.GetParameters().Length;
                var args = pc >= 2 ? new object?[] { key, _getTextDefaultFlag } : new object?[] { key };
                var val = _getTextMethod.Invoke(textBible, args) as string;
                if (!string.IsNullOrEmpty(val) && val != key)
                    return val;
            }
            catch { }
            return null;
        }

        private static object? InvokeStatic(Type type, string methodName)
        {
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            try { return method?.Invoke(null, null); }
            catch (Exception ex) { MelonLogger.Warning($"[Rsrc] {methodName} failed: {ex.Message}"); return null; }
        }

        private static string? GetEntityKey(object entity)
        {
            return GetMemberValue(entity, "myEntityType") as string
                ?? GetMemberValue(entity, "GetEntityType") as string;
        }

        private static string? GetEntityDisplayName(object entity)
        {
            // Entity.GetEntityName() is the game's own virtual localizer — returns the
            // current-language (Chinese) name for every item. displayName field is a
            // last-resort fallback (often empty on prefabs).
            var name = GetMemberValue(entity, "GetEntityName") as string;
            if (!string.IsNullOrEmpty(name))
                return name;
            return GetMemberValue(entity, "displayName") as string;
        }

        private static object? GetMemberValue(object instance, string memberName)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var type = instance.GetType();

            var prop = type.GetProperty(memberName, flags);
            if (prop != null)
            {
                try { return prop.GetValue(instance, null); }
                catch { }
            }

            var field = type.GetField(memberName, flags);
            if (field != null)
            {
                try { return field.GetValue(instance); }
                catch { }
            }

            var method = type.GetMethod(memberName, flags, null, Type.EmptyTypes, null);
            if (method != null)
            {
                try { return method.Invoke(instance, null); }
                catch { }
            }

            return null;
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
                var spawnBatchMethod = gmType.GetMethod(
                    "SpawnEntitiesAtPosition",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new[] { typeof(string), typeof(Vector3), typeof(int), typeof(bool), typeof(float), typeof(bool), typeof(bool) },
                    null);
                if (spawnBatchMethod != null)
                {
                    try { spawnBatchMethod.Invoke(null, new object[] { name, pos, count, true, 1f, false, true }); return; }
                    catch { }
                }

                var itemManager = InvokeStatic(gmType, "GetItemManager");
                var spawnItemMethod = itemManager?.GetType().GetMethod(
                    "SpawnItem",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(string), typeof(Vector3), typeof(Quaternion), typeof(ulong) },
                    null);
                if (spawnItemMethod != null)
                {
                    for (int i = 0; i < count && i < 200; i++)
                    {
                        var p = pos + new Vector3((i % 20) * 0.5f, 0f, (i / 20) * 0.5f);
                        spawnItemMethod.Invoke(itemManager, new object[] { name, p, Quaternion.identity, 0UL });
                    }
                    return;
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
            l.Label($"选择物品 — 槽位 {_selectedSlot + 1}:", 22f);

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
            GUI.Box(sr, string.IsNullOrEmpty(_searchInput) ? "输入搜索…" : _searchInput);
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

            const int vr = 12;
            int mx = Math.Max(0, filtered.Count - vr);
            _scrollOffset = Math.Clamp(_scrollOffset, 0, mx);

            var listY = l.Y + 22f;
            var listRect = new Rect(l.X, listY, l.Width, vr * 22f);
            if (ev != null && listRect.Contains(ev.mousePosition))
            {
                if (ev.type == EventType.ScrollWheel)
                {
                    ev.Use();
                    _scrollOffset = Math.Clamp(_scrollOffset + (ev.delta.y > 0 ? 4 : -4), 0, mx);
                }
            }

            GUI.Label(new Rect(l.X, l.Y, l.Width - 92f, 20f), $"{filtered.Count} 条结果");
            if (ImguiUtil.Button(new Rect(l.X + l.Width - 92f, l.Y, 44f, 20f), "上页") && _scrollOffset > 0)
                _scrollOffset = Math.Max(0, _scrollOffset - vr);
            if (ImguiUtil.Button(new Rect(l.X + l.Width - 46f, l.Y, 44f, 20f), "下页") && _scrollOffset < mx)
                _scrollOffset = Math.Min(mx, _scrollOffset + vr);
            l.Y += 22f;

            var scrollX = l.X + l.Width - 16f;
            var trackRect = new Rect(scrollX, l.Y, 16f, vr * 22f);
            GUI.Box(trackRect, "");
            if (mx > 0)
            {
                var thumbH = Math.Max(22f, trackRect.height * vr / filtered.Count);
                var thumbY = trackRect.y + (trackRect.height - thumbH) * _scrollOffset / mx;
                GUI.Box(new Rect(trackRect.x + 2f, thumbY, trackRect.width - 4f, thumbH), "");
                if (ev != null && ev.type == EventType.MouseDown && ev.button == 0 && trackRect.Contains(ev.mousePosition))
                {
                    ev.Use();
                    if (ev.mousePosition.y < thumbY)
                        _scrollOffset = Math.Max(0, _scrollOffset - vr);
                    else if (ev.mousePosition.y > thumbY + thumbH)
                        _scrollOffset = Math.Min(mx, _scrollOffset + vr);
                }
            }

            for (int i = 0; i < vr && (_scrollOffset + i) < filtered.Count; i++)
            {
                int idx = _scrollOffset + i;
                var key = filtered[idx];
                var display = FormatItemName(key);
                if (ImguiUtil.Button(new Rect(l.X, l.Y + i * 22f, l.Width - 20f, 20f), display))
                { _slots[_selectedSlot] = key; _showPicker = false; }
            }
            l.Y += vr * 22f + 4f;
            if (ImguiUtil.Button(new Rect(l.X, l.Y, 70f, 22f), "顶部")) _scrollOffset = 0;
            if (ImguiUtil.Button(new Rect(l.X + 74f, l.Y, 70f, 22f), "底部")) _scrollOffset = mx;
            if (ImguiUtil.Button(new Rect(l.X + 148f, l.Y, 70f, 22f), "关闭")) _showPicker = false;
            l.Y += 28f;
        }

        private string FormatItemName(string key)
        {
            var display = GetDisplayName(key);
            return string.Equals(display, key, StringComparison.OrdinalIgnoreCase)
                ? key
                : $"{display} ({key})";
        }

        private string GetDisplayName(string key)
        {
            if (_keyToDisplay.TryGetValue(key, out var dn) && !string.IsNullOrEmpty(dn) && dn != key)
                return dn;

            var zh = TryGameLocalize(key) ?? TryTokenLocalize(key);
            if (!string.IsNullOrEmpty(zh) && zh != key)
                return zh;

            if (ZhNames.TryGetValue(key, out var mapped))
                return mapped;

            foreach (var kvp in ZhNames)
            {
                if (key.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                    return kvp.Value;
            }

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
