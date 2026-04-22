using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using LordsAndVilleinsCheats.Core;
using LordsAndVilleinsCheats.Util;
using UnityEngine;

namespace LordsAndVilleinsCheats.Modules
{
    public class EconomyCheats : ICheatModule
    {
        public string Id   => "Economy";
        public string Name => "Economy";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        internal static ConfigEntry<bool> LockGold   = null!;
        internal static ConfigEntry<int>  GoldValue  = null!;
        internal static ConfigEntry<bool> LockFood   = null!;
        internal static ConfigEntry<int>  FoodValue  = null!;
        internal static ConfigEntry<bool> LockWood   = null!;
        internal static ConfigEntry<int>  WoodValue  = null!;
        internal static ConfigEntry<bool> LockStone  = null!;
        internal static ConfigEntry<int>  StoneValue = null!;
        internal static ModConfig SharedCfg = null!;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            SharedCfg  = cfg;
            LockGold   = cfg.File.Bind("Economy", "LockGold",   false, "Top up player GoldCoins to GoldValue every game tick.");
            GoldValue  = cfg.File.Bind("Economy", "GoldValue",  99999, "Target player GoldCoins.");
            LockFood   = cfg.File.Bind("Economy", "LockFood",   false, "Top up player Grain to FoodValue every game tick.");
            FoodValue  = cfg.File.Bind("Economy", "FoodValue",  9999,  "Target player Grain.");
            LockWood   = cfg.File.Bind("Economy", "LockWood",   false, "Top up player Wood to WoodValue every game tick.");
            WoodValue  = cfg.File.Bind("Economy", "WoodValue",  9999,  "Target player Wood.");
            LockStone  = cfg.File.Bind("Economy", "LockStone",  false, "Top up player Stone to StoneValue every game tick.");
            StoneValue = cfg.File.Bind("Economy", "StoneValue", 9999,  "Target player Stone.");

            var ok = HarmonyHelpers.SafeRun("EconomyCheats.PatchAll", () =>
            {
                var update = AccessTools.Method(typeof(GameManager), "Update")
                          ?? throw new System.Exception("GameManager.Update not found");
                harmony.Patch(update,
                    postfix: new HarmonyMethod(typeof(EconomyCheats), nameof(OnGameTick_Postfix)));
            });
            Status = ok ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() {}

        public void DrawGui()
        {
            DrawLockedField("Gold (GoldCoins)",  LockGold,  GoldValue);
            DrawLockedField("Food (Grain)",      LockFood,  FoodValue);
            DrawLockedField("Wood",              LockWood,  WoodValue);
            DrawLockedField("Stone",             LockStone, StoneValue);

            GUILayout.Space(8);
            GUILayout.Label("Quick add (one-shot):");
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+1000 Gold"))  AddResource(ResourceName.GoldCoins, 1000);
                if (GUILayout.Button("+1000 Food"))  AddResource(ResourceName.Grain,     1000);
                if (GUILayout.Button("+1000 Wood"))  AddResource(ResourceName.Wood,      1000);
                if (GUILayout.Button("+1000 Stone")) AddResource(ResourceName.Stone,     1000);
            }
        }

        public void DisableAll()
        {
            LockGold.Value = LockFood.Value = LockWood.Value = LockStone.Value = false;
        }

        // ----------- helpers -----------

        /// <summary>
        /// Enumerates the concrete Inventory objects backing the player's warehouse.
        /// playerInventory.GetInventory() returns an InventoryProxy whose internal
        /// inventoryViews list contains the actual Inventory instances.
        /// </summary>
        private static IEnumerable<Inventory> GetPlayerInventories()
        {
            var pm = PlayerManager.instance;
            if (pm == null) yield break;

            IInventoryView? proxy = null;
            try { proxy = pm.playerInventory?.GetInventory(); } catch { yield break; }
            if (proxy == null) yield break;

            // InventoryProxy holds a private List<IInventoryView> inventoryViews.
            // We walk that list recursively and yield any Inventory instances.
            foreach (var inv in WalkInventoryViews(proxy))
                yield return inv;
        }

        private static IEnumerable<Inventory> WalkInventoryViews(IInventoryView view)
        {
            if (view is Inventory directInv)
            {
                yield return directInv;
                yield break;
            }
            // Try to access the internal list via Traverse (works for InventoryProxy).
            var listTraverse = Traverse.Create(view).Field("inventoryViews");
            var list = listTraverse.GetValue() as System.Collections.IEnumerable;
            if (list != null)
            {
                foreach (var item in list)
                {
                    if (item is IInventoryView child)
                        foreach (var inv in WalkInventoryViews(child))
                            yield return inv;
                }
            }
        }

        private static void AddResource(ResourceName name, int amount)
        {
            HarmonyHelpers.SafeRun($"AddResource({name},{amount})", () =>
            {
                int remaining = amount;
                foreach (var inv in GetPlayerInventories())
                {
                    if (remaining <= 0) break;
                    // AddResource returns true if at least some was added.
                    if (inv.AddResource(name, remaining))
                        remaining = 0; // AddResource adds full amount or nothing (capacity check)
                }
            });
        }

        private static void TopUp(ResourceName name, int target)
        {
            // Read current total from the proxy view (has GetExistingResourceAmount).
            var pm = PlayerManager.instance;
            if (pm == null) return;
            IInventoryView? proxy = null;
            try { proxy = pm.playerInventory?.GetInventory(); } catch { return; }
            if (proxy == null) return;

            var current = proxy.GetExistingResourceAmount(name);
            var diff = target - current;
            if (diff <= 0) return;

            // Distribute the top-up across underlying inventories.
            int remaining = diff;
            foreach (var inv in GetPlayerInventories())
            {
                if (remaining <= 0) break;
                inv.AddResource(name, remaining);
                remaining = 0;
            }
        }

        private static void DrawLockedField(string label, ConfigEntry<bool> toggle, ConfigEntry<int> value)
        {
            using (new GUILayout.HorizontalScope())
            {
                toggle.Value = GUILayout.Toggle(toggle.Value, $"Lock {label}", GUILayout.Width(180));
                var s = GUILayout.TextField(value.Value.ToString(), GUILayout.Width(100));
                if (int.TryParse(s, out var n)) value.Value = n;
            }
        }

        // ----------- patch body -----------

        public static void OnGameTick_Postfix()
        {
            if (SharedCfg.GlobalDisableAll.Value) return;
            if (!LockGold.Value && !LockFood.Value && !LockWood.Value && !LockStone.Value) return;

            HarmonyHelpers.SafeRun("OnGameTick_Postfix", () =>
            {
                if (LockGold.Value)  TopUp(ResourceName.GoldCoins, GoldValue.Value);
                if (LockFood.Value)  TopUp(ResourceName.Grain,     FoodValue.Value);
                if (LockWood.Value)  TopUp(ResourceName.Wood,      WoodValue.Value);
                if (LockStone.Value) TopUp(ResourceName.Stone,     StoneValue.Value);
            });
        }
    }
}
