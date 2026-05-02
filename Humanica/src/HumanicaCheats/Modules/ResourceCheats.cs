using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using HumanicaCheats.Core;

namespace HumanicaCheats.Modules
{
    public class ResourceCheats : ICheatModule
    {
        public string       Name   => "资源";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        // ── 5 个用户可配置槽 ───────────────────────────────────────────
        private const int SlotCount = 5;
        private const int LockMin = 50;
        private static readonly int[] SupportedCapacityMultipliers = { 1, 5, 10, 50 };

        internal static int WarehouseCapacityMultiplier { get; private set; } = 1;

        // 默认槽:STICKS / LOG / COBBLESTONES / RAW_PELT / BREAD
        private static readonly int[] DefaultSlotIdx = { 1, 3, 2, 7, 32 };

        // 持久化:MelonPreferences,自动写入 UserData/MelonPreferences.cfg
        private MelonPreferences_Category _prefs = null!;
        private MelonPreferences_Entry<int>[] _slotIdxPref = null!;
        private MelonPreferences_Entry<int> _capacityMultiplierPref = null!;
        private bool _warehouseCapacityPatchBound;
        private bool[] _lockEnabled = new bool[SlotCount];

        // 资源选择器状态:-1 = 关,0..N = 给某个槽选资源
        private int _pickerForSlot = -1;
        private float _pickerScroll;
        // 搜索框:自实现(GUI.TextField 在此 IL2CPP 绑定下不可信),靠 Event.current.character
        // 写值 → IME 输入完成时 character 是最终字符,中英文都吃。
        private string _searchText = "";
        private bool _searchFocused = true;
        private Rect _searchFieldRect;

        // 全部 ResourceIndex(去掉 NONE / ___DEPRECATED),按名字排序
        // 启动时反射枚举一次,后面只读
        private static List<(string Name, int Idx)> _allResources = null!;

        public void Register(HarmonyLib.Harmony harmony)
        {
            if (_allResources == null) _allResources = LoadAllResources();

            _prefs = MelonPreferences.CreateCategory("HumanicaCheats.Resources");
            _slotIdxPref = new MelonPreferences_Entry<int>[SlotCount];
            for (int i = 0; i < SlotCount; i++)
            {
                _slotIdxPref[i] = _prefs.CreateEntry($"slot{i}_idx", DefaultSlotIdx[i],
                    description: $"Resource slot {i} ResourceIndex int value");
            }

            _capacityMultiplierPref = _prefs.CreateEntry("warehouse_capacity_multiplier", 1,
                description: "Warehouse capacity multiplier. Supported values: 1, 5, 10, 50.");
            WarehouseCapacityMultiplier = NormalizeCapacityMultiplier(_capacityMultiplierPref.Value);
            if (_capacityMultiplierPref.Value != WarehouseCapacityMultiplier)
            {
                _capacityMultiplierPref.Value = WarehouseCapacityMultiplier;
                _prefs.SaveToFile(false);
            }

            _warehouseCapacityPatchBound = WarehouseCapacityPatch.Register(harmony);

            Status = ModuleStatus.Ok;
        }

        private static int NormalizeCapacityMultiplier(int value)
        {
            for (int i = 0; i < SupportedCapacityMultipliers.Length; i++)
            {
                if (SupportedCapacityMultipliers[i] == value) return value;
            }
            return 1;
        }

        private static List<(string, int)> LoadAllResources()
        {
            var list = new List<(string, int)>();
            try
            {
                var t = typeof(Il2Cpp.ResourceIndex);
                foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    if (f.Name == "NONE" || f.Name.Contains("DEPRECATED")) continue;
                    try
                    {
                        var v = f.GetValue(null);
                        if (v is Il2Cpp.ResourceIndex ri)
                            list.Add((f.Name, IndexInt(ri)));
                    }
                    catch { /* 单项失败跳过 */ }
                }
                list.Sort((a, b) => string.Compare(a.Item1, b.Item1, StringComparison.Ordinal));
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[ResourceCheats] LoadAllResources 失败: {ex.Message}");
            }
            return list;
        }

        // ── ResourceIndex 反射读 int ────────────────────────────────
        private static int IndexInt(Il2Cpp.ResourceIndex idx)
        {
            try { return Convert.ToInt32(idx); } catch { }
            try
            {
                var vf = typeof(Il2Cpp.ResourceIndex).GetField("value__",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (vf != null) return Convert.ToInt32(vf.GetValue(idx));
            }
            catch { }
            return -1;
        }

        private static string FindName(int idx)
        {
            if (_allResources != null)
                foreach (var item in _allResources)
                    if (item.Item2 == idx) return item.Item1;
            return $"<idx {idx}>";
        }

        // 启动 dump:由 Plugin.OnInitializeMelon 调用,把全部 ResourceIndex 列到日志
        public static void DumpResourceIndex()
        {
            try
            {
                var t = typeof(Il2Cpp.ResourceIndex);
                MelonLogger.Msg($"[ResourceIndex.dump] type={t.FullName} isEnum={t.IsEnum} isValueType={t.IsValueType}");

                if (t.IsEnum)
                {
                    var values = Enum.GetValues(t);
                    MelonLogger.Msg($"[ResourceIndex.dump] enum values: {values.Length}");
                    foreach (var v in values.Cast<object>().OrderBy(o => o.ToString()))
                        MelonLogger.Msg($"  {v} = {Convert.ToInt32(v)}");
                    return;
                }

                var fields = t.GetFields(BindingFlags.Public | BindingFlags.Static);
                MelonLogger.Msg($"[ResourceIndex.dump] static fields: {fields.Length}");
                foreach (var f in fields.OrderBy(f => f.Name))
                {
                    try
                    {
                        var v = f.GetValue(null);
                        if (v is Il2Cpp.ResourceIndex ri)
                            MelonLogger.Msg($"  field {f.Name} = {IndexInt(ri)}");
                        else
                            MelonLogger.Msg($"  field {f.Name} = {v}");
                    }
                    catch (Exception ex) { MelonLogger.Msg($"  field {f.Name} = <err: {ex.Message}>"); }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[ResourceIndex.dump] failed: {ex}");
            }
        }

        // ── AddRes ────────────────────────────────────────────────
        // 第三参数 createIfNeeded=true:仓库容量不够时自动开新仓库槽,
        // 否则容量小的资源(如 cobblestones / raw pelt)会被截到 +10 还可能卡死。
        private static void AddRes(string label, int idxInt, int amount)
        {
            try
            {
                var vd = Il2Cpp.S.VillageData;
                if (vd == null)
                {
                    MelonLogger.Warning($"[ResourceCheats] AddRes({label} idx={idxInt} amt={amount}) — VillageData null");
                    return;
                }
                var idx = (Il2Cpp.ResourceIndex)idxInt;
                vd.AddResourceIntoFreeWarehouse(idx, amount, true);
                MelonLogger.Msg($"[ResourceCheats] AddRes({label} idx={idxInt} amt={amount}) OK");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[ResourceCheats] AddRes({label} idx={idxInt} amt={amount}) 失败: {ex.Message}");
            }
        }

        private static int GetCurrentAmount(int idxInt)
        {
            try
            {
                var vd = Il2Cpp.S.VillageData;
                if (vd == null) return -1;
                var idx = (Il2Cpp.ResourceIndex)idxInt;
                return vd.GetResourceAmount(idx);
            }
            catch { return -1; }
        }

        // ── DrawGui ──────────────────────────────────────────────
        public void DrawGui(Layout l)
        {
            if (!GameRefs.IsReady) { l.Label("等待游戏加载…"); return; }
            if (_allResources == null || _allResources.Count == 0)
            {
                l.Label("ResourceIndex 列表未加载,看 MelonLoader 启动日志。");
                return;
            }

            if (_pickerForSlot >= 0)
            {
                DrawPicker(l);
                return;
            }

            DrawWarehouseCapacity(l);
            DrawSlots(l);
        }

        private void DrawWarehouseCapacity(Layout l)
        {
            const float btnW = 50f;
            const float btnH = 26f;
            const float gap = 6f;

            GUI.Label(new Rect(l.X, l.Y, 80f, btnH), "仓库容量:");
            float x = l.X + 84f;
            for (int i = 0; i < SupportedCapacityMultipliers.Length; i++)
            {
                int multiplier = SupportedCapacityMultipliers[i];
                var r = new Rect(x + i * (btnW + gap), l.Y, btnW, btnH);
                if (ImguiUtil.Button(r, $"x{multiplier}", WarehouseCapacityMultiplier == multiplier))
                {
                    WarehouseCapacityMultiplier = multiplier;
                    _capacityMultiplierPref.Value = multiplier;
                    _prefs.SaveToFile(false);
                    MelonLogger.Msg($"[ResourceCheats] Warehouse capacity multiplier set to x{multiplier}");
                }
            }
            l.Y += btnH + 4f;

            if (!_warehouseCapacityPatchBound)
            {
                l.Label("[!] 仓库容量 patch 未绑定 — 查看 MelonLoader 控制台");
            }
            l.Space(4);
        }

        private void DrawSlots(Layout l)
        {
            l.Label($"5 个槽,点资源名换成 {_allResources.Count} 种之一(选择跨会话保存):");
            l.Space(4);
            for (int i = 0; i < SlotCount; i++) DrawSlot(l, i);
        }

        private void DrawSlot(Layout l, int slot)
        {
            const float btnH = 26f;
            int curIdx = _slotIdxPref[slot].Value;
            string name = FindName(curIdx);

            var rName  = new Rect(l.X,        l.Y, 200, btnH);
            var rPlus5 = new Rect(l.X + 204,  l.Y, 50,  btnH);
            var rP50   = new Rect(l.X + 258,  l.Y, 50,  btnH);
            var rLock  = new Rect(l.X + 312,  l.Y, 130, btnH);

            if (ImguiUtil.Button(rName, $"{ResourceI18n.Display(name)} ({curIdx})"))
            {
                _pickerForSlot = slot;
                _pickerScroll = 0;
                _searchText = "";
                _searchFocused = true;
            }
            if (ImguiUtil.Button(rPlus5, "+5"))  AddRes(name, curIdx, 5);
            if (ImguiUtil.Button(rP50,   "+50")) AddRes(name, curIdx, 50);
            _lockEnabled[slot] = ImguiUtil.Toggle(rLock, _lockEnabled[slot], $"锁定≥{LockMin}");

            l.Y += btnH + 4f;
        }

        private void DrawPicker(Layout l)
        {
            const float titleH = 24f;
            const float searchH = 26f;
            const float itemH = 22f;

            // ── 标题栏 ──
            var titleRect = new Rect(l.X,                  l.Y, l.Width - 64f, titleH);
            var closeRect = new Rect(l.X + l.Width - 60f,  l.Y, 60f,           titleH);
            GUI.Label(titleRect, $"为槽 {_pickerForSlot} 选资源(滚轮滚动 / 输入过滤):");
            if (ImguiUtil.Button(closeRect, "关闭"))
            {
                _pickerForSlot = -1;
                return;
            }
            l.Y += titleH + 4f;

            // ── 搜索框 + 清空按钮 ──
            _searchFieldRect = new Rect(l.X, l.Y, l.Width - 80f, searchH);
            var clearRect    = new Rect(l.X + l.Width - 76f, l.Y, 76f, searchH);

            var prevColor = GUI.color;
            GUI.color = _searchFocused ? Color.cyan : Color.white;
            // 简单光标:focused 时尾部加 |
            string display = string.IsNullOrEmpty(_searchText)
                ? (_searchFocused ? "|" : "搜索 (中/英)…")
                : _searchText + (_searchFocused ? "|" : "");
            GUI.Box(_searchFieldRect, " " + display);
            GUI.color = prevColor;

            if (ImguiUtil.Button(clearRect, "清空"))
            {
                _searchText = "";
                _searchFocused = true;
                _pickerScroll = 0;
            }

            HandleSearchInput();
            l.Y += searchH + 6f;

            // ── 过滤 ──
            List<(string Name, int Idx)> filtered;
            if (string.IsNullOrEmpty(_searchText))
            {
                filtered = _allResources;
            }
            else
            {
                var q = _searchText.ToLowerInvariant();
                filtered = _allResources.Where(r =>
                    r.Name.ToLowerInvariant().Contains(q)
                    || (ResourceI18n.Zh.TryGetValue(r.Name, out var zh) && zh.Contains(_searchText))
                ).ToList();
            }

            GUI.Label(new Rect(l.X, l.Y, l.Width, 18f),
                $"  匹配 {filtered.Count} 项 / 共 {_allResources.Count}");
            l.Y += 20f;

            // ── 列表 + 滚轮 ──
            float listY = l.Y;
            float listH = 360f;
            var listRect = new Rect(l.X, listY, l.Width, listH);

            var ev = Event.current;
            if (ev != null && ev.type == EventType.ScrollWheel && listRect.Contains(ev.mousePosition))
            {
                _pickerScroll += ev.delta.y * itemH;
                ev.Use();
            }
            float maxScroll = Mathf.Max(0f, filtered.Count * itemH - listH);
            _pickerScroll = Mathf.Clamp(_pickerScroll, 0f, maxScroll);

            int firstVisible = Mathf.FloorToInt(_pickerScroll / itemH);
            int lastVisible  = Mathf.Min(filtered.Count - 1,
                                         Mathf.CeilToInt((_pickerScroll + listH) / itemH));

            for (int i = firstVisible; i <= lastVisible; i++)
            {
                float yLocal = i * itemH - _pickerScroll;
                if (yLocal < 0 || yLocal + itemH > listH) continue;

                var item = filtered[i];
                var r = new Rect(l.X, listY + yLocal, l.Width, itemH);
                if (ImguiUtil.Button(r, $"{ResourceI18n.Display(item.Name)}  (idx={item.Idx})"))
                {
                    _slotIdxPref[_pickerForSlot].Value = item.Idx;
                    _prefs.SaveToFile(false);
                    _pickerForSlot = -1;
                    return;
                }
            }
        }

        // ── 自实现文本输入 ────────────────────────────────────────
        // GUI.TextField 在此 IL2CPP 绑定下 string 返回值不可信(参考 GUI.Button bool 同样
        // 不回传)。手 hold 一个 string 状态,Event.current.character 写值。
        // IME 输入(中文拼音) Unity 在最终 commit 时 raise KeyDown,character 是最终字符,
        // 因此中英文都能搜。
        private void HandleSearchInput()
        {
            var ev = Event.current;
            if (ev == null) return;

            // 点击 search 框聚焦,点别处失焦
            if (ev.type == EventType.MouseDown && ev.button == 0)
            {
                _searchFocused = _searchFieldRect.Contains(ev.mousePosition);
            }

            if (!_searchFocused) return;

            if (ev.type == EventType.KeyDown)
            {
                if (ev.keyCode == KeyCode.Backspace)
                {
                    if (_searchText.Length > 0)
                    {
                        _searchText = _searchText.Substring(0, _searchText.Length - 1);
                        _pickerScroll = 0;
                    }
                    ev.Use();
                }
                else if (ev.keyCode == KeyCode.Escape)
                {
                    _searchText = "";
                    _searchFocused = false;
                    _pickerScroll = 0;
                    ev.Use();
                }
                else if (ev.character != 0 && !char.IsControl(ev.character))
                {
                    _searchText += ev.character;
                    _pickerScroll = 0;
                    ev.Use();
                }
            }
        }

        // ── 锁定:每帧补差值 ──
        public void OnUpdate()
        {
            if (!GameRefs.IsReady) return;
            for (int i = 0; i < SlotCount; i++)
            {
                if (!_lockEnabled[i]) continue;
                int idx = _slotIdxPref[i].Value;
                int cur = GetCurrentAmount(idx);
                if (cur >= 0 && cur < LockMin) AddRes(FindName(idx), idx, LockMin - cur);
            }
        }
    }
}
