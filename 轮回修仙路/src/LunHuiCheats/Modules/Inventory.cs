using System.Collections.Generic;
using HarmonyLib;
using LunHuiCheats.Core;
using LunHuiCheats.Core.Gui;
using UnityEngine;

namespace LunHuiCheats.Modules
{
    /// <summary>
    /// Inventory cheat. Phase 1: browse items the FakeInventoryData already holds
    /// (its All* category lists) and add quantity via AddItem(existing, qty).
    /// </summary>
    public sealed class Inventory : ICheatModule
    {
        public string Id => "inventory";
        public string Name => "背包 Inventory";
        public string Category => "背包";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        // Inventory list field name -> display category.
        private static readonly (string field, string cat)[] Lists =
        {
            ("AllDanYao", "丹药"), ("AllEquips", "装备"), ("AllMaterials", "材料"),
            ("AllUseItem", "消耗"), ("AllPets", "宠物"), ("AllFlyTalisman", "飞行符"),
            ("AllSeedMaterials", "种子"), ("AllCoins", "货币"),
        };

        private readonly ItemBrowserModel _model = new();
        private readonly ItemBrowserView _view = new();
        private int _lastBuildFrame = -1000;
        private long _newItemId = 1;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            Status = AccessTools.TypeByName("FakeInventoryData") != null
                ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() { }
        public void OnUpdate() { }

        private void RebuildRows()
        {
            if (Time.frameCount - _lastBuildFrame < 60) return;
            _lastBuildFrame = Time.frameCount;

            var inv = GameRefs.Inventory;
            if (inv == null) { _model.SetRows(new List<ItemRow>()); return; }

            var rows = new List<ItemRow>();
            foreach (var (field, cat) in Lists)
            {
                if (!ReflectAccessor.TryGet(inv, field, out var listObj) || listObj == null)
                {
                    Plugin.LogSrc?.LogWarning($"[Inventory] Field '{field}' missing on FakeInventoryData.");
                    continue;
                }
                Plugin.LogSrc?.LogInfo($"[Inventory] Field '{field}' found, type={listObj.GetType().Name}");
                var lt = listObj.GetType();
                var getEnum = lt.GetMethod("GetEnumerator");
                if (getEnum == null)
                {
                    Plugin.LogSrc?.LogWarning($"[Inventory] GetEnumerator not found on {lt.FullName}.");
                    continue;
                }
                var enumerator = getEnum.Invoke(listObj, null);
                if (enumerator == null) { Plugin.LogSrc?.LogWarning($"[Inventory] enum null for {field}"); continue; }
                var et = enumerator.GetType();
                var moveNext = et.GetMethod("MoveNext");
                var currentProp = et.GetProperty("Current");
                if (moveNext == null || currentProp == null) { Plugin.LogSrc?.LogWarning($"[Inventory] MoveNext/Current missing on {et.Name}"); continue; }
                int count = 0;
                try
                {
                    while ((bool)moveNext.Invoke(enumerator, null))
                    {
                        var item = currentProp.GetValue(enumerator);
                        if (item != null)
                        {
                            count++;
                            var name = ReflectAccessor.TryGet(item, "name", out var n) && n != null ? n.ToString() : item.ToString();
                            rows.Add(new ItemRow(name ?? "?", cat, item));
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Plugin.LogSrc?.LogWarning($"[Inventory] enum {field} failed: {ex.Message}");
                    break;
                }
                Plugin.LogSrc?.LogInfo($"[Inventory] Loaded {count} items from {field} ({cat}).");
            }
            _model.SetRows(rows);
        }

        public void DrawGui()
        {
            var inv = GameRefs.Inventory;
            if (inv == null)
            {
                GUI.Label(new Rect(0, 0, 360, 20), "未找到 FakeInventoryData（进入游戏世界后生效）");
                return;
            }
            GUI.Label(new Rect(0, 0, 60, 24), "物品ID");
            _newItemId = GuiWidgets.Int64Field(new Rect(62, 0, 100, 24), "inv.newid", _newItemId);
            if (GUI.Button(new Rect(166, 0, 90, 24), "尝试添加"))
                TryAddById(_newItemId, 1);
            RebuildRows();
            _view.Draw(new Rect(0, 28, 380, 272), _model, OnAdd);
        }

        private void OnAdd(ItemRow row, long qty)
        {
            var inv = GameRefs.Inventory;
            if (inv == null) return;
            // AddCoin(CoinData,int) for coins, AddItem(BaseRewardData,int) otherwise.
            var method = row.Category == "货币" ? "AddCoin" : "AddItem";
            CallAdd(inv, method, row.Payload, (int)qty);
        }

        private static void CallAdd(object inv, string method, object payload, int qty)
        {
            try
            {
                var m = AccessTools.Method(inv.GetType(), method, new[] { payload.GetType(), typeof(int) });
                if (m == null) { Plugin.LogSrc?.LogWarning($"[Inventory] method {method}({payload.GetType().Name},int) not found."); return; }
                m.Invoke(inv, new object[] { payload, qty });
            }
            catch (System.Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[Inventory] {method} failed: {ex.Message}");
            }
        }

        private void TryAddById(long id, int qty)
        {
            var inv = GameRefs.Inventory;
            if (inv == null) return;
            var brt = AccessTools.TypeByName("BaseRewardData");
            if (brt == null) { Plugin.LogSrc?.LogWarning("[Inventory] BaseRewardData type not found."); return; }
            try
            {
                var reward = System.Activator.CreateInstance(brt);
                if (reward == null) return;
                // Common id field names on reward data; set whichever exists.
                ReflectAccessor.TrySet(reward, "id", id);
                ReflectAccessor.TrySet(reward, "itemId", id);
                CallAdd(inv, "AddItem", reward, qty);
                Plugin.LogSrc?.LogInfo($"[Inventory] TryAddById({id}) invoked AddItem.");
            }
            catch (System.Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[Inventory] construct BaseRewardData failed: {ex.Message}");
            }
        }

        public void DisableAll() { }
    }
}
