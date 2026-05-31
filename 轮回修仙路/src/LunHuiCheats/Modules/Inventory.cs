using System.Collections;
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

        public void Register(ModConfig cfg, Harmony harmony)
        {
            Status = AccessTools.TypeByName("FakeInventoryData") != null
                ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() { }
        public void OnUpdate() { }

        private void RebuildRows()
        {
            // Rebuild at most every ~60 frames (lists are large; reflection is costly).
            if (Time.frameCount - _lastBuildFrame < 60) return;
            _lastBuildFrame = Time.frameCount;

            var inv = GameRefs.Inventory;
            if (inv == null) { _model.SetRows(new List<ItemRow>()); return; }

            var rows = new List<ItemRow>();
            foreach (var (field, cat) in Lists)
            {
                if (!ReflectAccessor.TryGet(inv, field, out var listObj) || listObj is not IEnumerable en) continue;
                foreach (var item in en)
                {
                    if (item == null) continue;
                    var name = ReflectAccessor.TryGet(item, "name", out var n) && n != null ? n.ToString() : item.ToString();
                    rows.Add(new ItemRow(name ?? "?", cat, item));
                }
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
            RebuildRows();
            _view.Draw(new Rect(0, 0, 380, 300), _model, OnAdd);
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
                m?.Invoke(inv, new object[] { payload, qty });
            }
            catch (System.Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[Inventory] {method} failed: {ex.Message}");
            }
        }

        public void DisableAll() { }
    }
}
