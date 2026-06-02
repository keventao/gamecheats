using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LunHuiCheats.Core;
using LunHuiCheats.Core.Gui;
using UnityEngine;

namespace LunHuiCheats.Modules
{
    /// <summary>
    /// 背包：枚举 PropsData 全道具库（GMDataBaseSystem.SearchConfAllStatic(DBName.Props=24, typeof PropsData)），
    /// 按 bag_type 分子分类 tab，列表按选中分类过滤。点物品「+数量」= 用该 PropsData 配置建一个
    /// BaseRewardData（拷字段：id→rewardId、bag_type→bagType、prop_type→propType/rewardType、
    /// prop_quality→quality、stack_num、prop_use→propUse、name…），再 AddItem 进背包。
    /// </summary>
    public sealed class Inventory : ICheatModule
    {
        public string Id => "inventory";
        public string Name => "背包 Inventory";
        public string Category => "背包";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        private long _newItemId = 1;
        private int _spawnQty = 1;
        private string _spawnStatus = "";

        private readonly ScrollList _scroll = new();
        private readonly List<PropRow> _allProps = new();
        private readonly List<(int val, string name)> _bagTabs = new();
        private readonly List<int> _filtered = new();
        private int _activeBag = -1;        // -1 = 全部
        private bool _loaded;
        private bool _dumpedSample;
        private float _tcx, _tcy;           // tab 布局游标

        private struct PropRow { public int id; public string disp; public object cfg; public int bag; }

        public void Register(ModConfig cfg, Harmony harmony)
        {
            Status = AccessTools.TypeByName("FakeInventoryData") != null ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() { _loaded = false; _allProps.Clear(); _bagTabs.Clear(); _filtered.Clear(); _activeBag = -1; }
        public void OnUpdate() { }

        public void DrawGui()
        {
            var inv = GameRefs.Inventory;
            if (inv == null) { GUI.Label(new Rect(0, 0, 400, 20), "未找到 FakeInventoryData（进入游戏世界后生效）"); return; }
            if (!_loaded) { _loaded = true; EnumerateProps(); }

            // 子分类 tab（按 bag_type），自动换行。
            _tcx = 0; _tcy = 0;
            if (Tab(58, "全部", _activeBag < 0)) { _activeBag = -1; RebuildFilter(); }
            foreach (var bt in _bagTabs)
                if (Tab(64, BagLabel(bt.name), _activeBag == bt.val)) { _activeBag = bt.val; RebuildFilter(); }
            float y = _tcy + 28;

            GUI.Label(new Rect(0, y, 32, 24), "数量");
            _spawnQty = (int)GuiWidgets.Int64Field(new Rect(34, y, 56, 24), "inv.qty", _spawnQty);
            GUI.Label(new Rect(98, y, 20, 24), "ID");
            _newItemId = GuiWidgets.Int64Field(new Rect(120, y, 70, 24), "inv.newid", _newItemId);
            if (GUI.Button(new Rect(194, y, 54, 24), "生成")) SpawnById((int)_newItemId, _spawnQty);
            if (GUI.Button(new Rect(412, y, 58, 24), "刷新")) EnumerateProps();
            if (_spawnStatus.Length > 0) GUI.Label(new Rect(0, y + 26, 470, 20), _spawnStatus);

            _scroll.Draw(new Rect(0, y + 50, 470, 300), _filtered.Count, 26, DrawRow);
        }

        private bool Tab(float bw, string label, bool active)
        {
            if (_tcx + bw > 470) { _tcx = 0; _tcy += 26; }
            var rc = new Rect(_tcx, _tcy, bw - 2, 24);
            _tcx += bw;
            var prev = GUI.color;
            if (active) GUI.color = new Color(0.3f, 0.6f, 1f);
            bool clicked = GUI.Button(rc, label);
            GUI.color = prev;
            return clicked;
        }

        // Map BagType enum member names to short Chinese labels (fallback = raw name).
        private static string BagLabel(string name) => name switch
        {
            "Danyao" => "丹药", "Equip" => "装备", "Material" => "材料", "UseItem" => "消耗",
            "Pet" => "宠物", "FlyTalisman" => "符箓", "SeedMaterial" => "种子", "Coin" => "货币",
            "Spirite" => "魂魄", "AchievementMethod" => "功法", "HeartAchievementMethod" => "心法",
            "CreateMaterial" => "制材", "FixedTool" => "工具", "EDUProp" => "培养", "None" => "其他",
            _ => name,
        };

        private void DrawRow(int i, Rect rr)
        {
            if (i < 0 || i >= _filtered.Count) return;
            var d = _allProps[_filtered[i]];
            GUI.Label(new Rect(rr.x, rr.y, rr.width - 70, rr.height), $"id{d.id}  {d.disp}");
            if (GUI.Button(new Rect(rr.x + rr.width - 66, rr.y, 64, rr.height - 2), $"+{_spawnQty}"))
                SpawnByConfig(d.id, _spawnQty, d.cfg);
        }

        private void RebuildFilter()
        {
            _filtered.Clear();
            for (int i = 0; i < _allProps.Count; i++)
                if (_activeBag < 0 || _allProps[i].bag == _activeBag) _filtered.Add(i);
        }

        // Enumerate the whole 道具 library via the non-generic GM overload:
        // static List<Object> SearchConfAllStatic(DBName.Props=24, Il2CppSystem.Type=PropsData).
        private void EnumerateProps()
        {
            var log = Plugin.LogSrc;
            _allProps.Clear(); _bagTabs.Clear(); _filtered.Clear();
            var gm = AccessTools.TypeByName("DataLib.GMDataBaseSystem");
            var dbEnum = AccessTools.TypeByName("DataLib.GMDataBaseSystem+DBName") ?? AccessTools.TypeByName("DBName");
            var cfgT = AccessTools.TypeByName("Configuration.PropsData");
            if (gm == null || dbEnum == null || cfgT == null)
            { _spawnStatus = "缺 GM/PropsData（见日志）"; log?.LogWarning($"[Props] gm={gm != null} db={dbEnum != null} cfg={cfgT != null}"); return; }

            MethodInfo mi = null;
            foreach (var m in gm.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                if (m.Name == "SearchConfAllStatic" && !m.IsGenericMethodDefinition && m.GetParameters().Length == 2) { mi = m; break; }
            if (mi == null) { _spawnStatus = "SearchConfAllStatic 未找到"; return; }

            try
            {
                object dbVal = Enum.ToObject(dbEnum, 24);
                var listObj = mi.Invoke(null, new object[] { dbVal, Il2CppInterop.Runtime.Il2CppType.From(cfgT) });
                if (listObj == null) { _spawnStatus = "返回 null"; return; }
                var lt = listObj.GetType();
                int cnt = Convert.ToInt32(lt.GetProperty("Count").GetValue(listObj));
                var itemM = lt.GetMethod("get_Item") ?? lt.GetMethod("Item");
                var strProps = new List<PropertyInfo>();
                foreach (var p in cfgT.GetProperties())
                    if (p.PropertyType == typeof(string)) strProps.Add(p);

                var bagSeen = new Dictionary<int, string>();
                for (int i = 0; i < cnt; i++)
                {
                    object raw = itemM?.Invoke(listObj, new object[] { i });
                    if (raw == null) continue;
                    var typed = CastIl2Cpp(raw, cfgT);
                    int id = ReflectAccessor.GetInt32(typed, "id");
                    int bag = 0; string bagName = "?";
                    if (ReflectAccessor.TryGet(typed, "bag_type", out var bv) && bv != null)
                    { try { bag = Convert.ToInt32(bv); } catch { } bagName = bv.ToString(); }
                    if (!bagSeen.ContainsKey(bag)) bagSeen[bag] = bagName;
                    string disp = "";
                    foreach (var sp in strProps)
                    { try { var v = sp.GetValue(typed) as string; if (!string.IsNullOrEmpty(v)) disp += v + " "; } catch { } }
                    if (disp.Length > 64) disp = disp.Substring(0, 64);
                    _allProps.Add(new PropRow { id = id, disp = disp, cfg = typed, bag = bag });
                }
                foreach (var kv in bagSeen) _bagTabs.Add((kv.Key, kv.Value));
                _bagTabs.Sort((a, b) => a.val.CompareTo(b.val));
                _activeBag = -1;
                RebuildFilter();
                _spawnStatus = $"道具 {_allProps.Count} 条 · {_bagTabs.Count} 类";
                log?.LogInfo($"[Props] {_allProps.Count} items bags=[{string.Join(",", _bagTabs.ConvertAll(t => $"{t.val}:{t.name}"))}]");
            }
            catch (Exception ex) { _spawnStatus = $"枚举失败: {ex.GetType().Name}（见日志）"; log?.LogWarning($"[Props] enumerate failed: {ex.GetType().Name}: {ex.Message}"); }
        }

        private void SpawnById(int id, int qty)
        {
            object cfg = null;
            foreach (var r in _allProps) if (r.id == id) { cfg = r.cfg; break; }
            SpawnByConfig(id, qty, cfg);
        }

        // Build a BaseRewardData from the PropsData config and AddItem it into the bag.
        private void SpawnByConfig(int id, int qty, object cfg)
        {
            var inv = GameRefs.Inventory;
            if (inv == null) return;
            var brt = AccessTools.TypeByName("BaseRewardData");
            if (brt == null) { _spawnStatus = "BaseRewardData 缺"; return; }

            object reward = TryActivator(brt);
            if (reward == null) { var src = FirstExistingItem(); if (src != null) ReflectAccessor.TryInvoke(src, "Clone", out reward); }
            if (reward == null) { _spawnStatus = "建 reward 失败"; return; }

            ReflectAccessor.TrySet(reward, "rewardId", id);
            ReflectAccessor.TrySet(reward, "rewardNum", qty);
            if (cfg != null) CopyConfigToReward(cfg, reward);
            ReflectAccessor.TrySet(reward, "iconName", id.ToString());

            if (!_dumpedSample) { _dumpedSample = true; DumpReward("REAL", FirstExistingItem()); DumpReward("MINE", reward); }

            string nm = ReflectAccessor.TryGet(reward, "name", out var n) && n != null ? n.ToString() : "?";
            bool added = ReflectAccessor.TryInvoke(inv, "AddItem", out _, reward, qty);
            _spawnStatus = $"生成 id={id} {nm} AddItem={(added ? "✓" : "✗")}";
            Plugin.LogSrc?.LogInfo($"[Inventory] spawn id={id} name={nm} bagType={ReflectAccessor.GetInt32(reward, "bagType")} rewardType={ReflectAccessor.GetInt32(reward, "rewardType")} added={added} cfg={cfg != null}");
        }

        // Copy PropsData(config, snake_case) fields into BaseRewardData(reward, camelCase).
        private static void CopyConfigToReward(object cfg, object reward)
        {
            CopyVal(cfg, "bag_type", reward, "bagType");
            CopyVal(cfg, "prop_type", reward, "propType");
            CopyVal(cfg, "prop_quality", reward, "quality");
            CopyVal(cfg, "stack_num", reward, "stack_num");
            CopyVal(cfg, "DuraMax", reward, "duraMax");
            CopyVal(cfg, "name", reward, "name");
            CopyVal(cfg, "prop_use", reward, "propUse");
            CopyVal(cfg, "prop_info", reward, "description");
            CopyVal(cfg, "info", reward, "description");
            CopyVal(cfg, "description", reward, "description");
            // rewardType (RewardType enum) from prop_type (PropType enum) by member name.
            CopyEnumByName(cfg, "prop_type", reward, "rewardType");
        }

        private static void CopyVal(object src, string srcName, object dst, string dstName)
        {
            if (ReflectAccessor.TryGet(src, srcName, out var v) && v != null)
                ReflectAccessor.TrySet(dst, dstName, v);
        }

        private static void CopyEnumByName(object src, string srcName, object dst, string dstName)
        {
            try
            {
                if (!ReflectAccessor.TryGet(src, srcName, out var v) || v == null) return;
                var name = v.ToString();
                var dp = dst.GetType().GetProperty(dstName);
                if (dp == null || !dp.CanWrite || !dp.PropertyType.IsEnum) return;
                foreach (var en in Enum.GetNames(dp.PropertyType))
                    if (en == name) { dp.SetValue(dst, Enum.Parse(dp.PropertyType, name)); return; }
            }
            catch { }
        }

        // Re-wrap an Il2CppSystem.Object as a concrete Il2CppInterop wrapper via its Pointer ctor.
        private static object CastIl2Cpp(object il2cppObj, Type wrapperType)
        {
            try
            {
                var pp = il2cppObj.GetType().GetProperty("Pointer");
                if (pp == null) return il2cppObj;
                var ptr = pp.GetValue(il2cppObj);
                return Activator.CreateInstance(wrapperType, ptr) ?? il2cppObj;
            }
            catch { return il2cppObj; }
        }

        private static object TryActivator(Type brt)
        {
            try { return Activator.CreateInstance(brt); }
            catch (Exception ex) { Plugin.LogSrc?.LogWarning($"[Inventory] Activator failed: {ex.Message}"); return null; }
        }

        // First non-null item from a few bag lists (Clone fallback + REAL-item dump reference).
        private object FirstExistingItem()
        {
            var inv = GameRefs.Inventory;
            if (inv == null) return null;
            foreach (var field in new[] { "AllDanYao", "AllEquips", "AllMaterials", "AllUseItem" })
            {
                if (!ReflectAccessor.TryGet(inv, field, out var listObj) || listObj == null) continue;
                try
                {
                    var lt = listObj.GetType();
                    var en = lt.GetMethod("GetEnumerator")?.Invoke(listObj, null);
                    if (en == null) continue;
                    var et = en.GetType();
                    var moveNext = et.GetMethod("MoveNext");
                    var currentProp = et.GetProperty("Current");
                    if (moveNext == null || currentProp == null) continue;
                    if ((bool)moveNext.Invoke(en, null))
                    {
                        var item = currentProp.GetValue(en);
                        if (item != null) return item;
                    }
                }
                catch { }
            }
            return null;
        }

        private static void DumpReward(string tag, object r)
        {
            if (r == null) { Plugin.LogSrc?.LogInfo($"[SpawnDump] {tag}=null"); return; }
            string s = "";
            foreach (var p in r.GetType().GetProperties())
            {
                try
                {
                    if (!p.CanRead) continue;
                    var pt = p.PropertyType;
                    if (pt == typeof(string) || pt == typeof(int) || pt == typeof(long) || pt == typeof(short) || pt.IsEnum)
                        s += $"{p.Name}={p.GetValue(r)}, ";
                }
                catch { }
            }
            Plugin.LogSrc?.LogInfo($"[SpawnDump] {tag}: {s}");
        }

        public void DisableAll() { }
    }
}
