using System.Collections.Generic;
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

        public void Register(ModConfig cfg, Harmony harmony)
        {
            // The patch is the attach hook for CheatsRunner — it runs every game tick
            // and ensures our runner is bound to the live GameManager GameObject.
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
            GUILayout.Label($"Money: {ReadCurrentAmount(ResourceName.Money)}");
            GUILayout.Label($"Food (Grain): {ReadCurrentAmount(ResourceName.Grain)}");

            GUILayout.Space(8);
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+10000 Money")) AddResource(ResourceName.Money, 10000);
                if (GUILayout.Button("+1000 Food"))  AddResource(ResourceName.Grain, 1000);
            }
        }

        public void DisableAll() {}

        // ----------- helpers -----------

        private static IEnumerable<Inventory> GetPlayerInventories()
        {
            var pm = PlayerManager.instance;
            if (pm == null) yield break;

            IInventoryView? proxy = null;
            try { proxy = pm.playerInventory?.GetInventory(); } catch { yield break; }
            if (proxy == null) yield break;

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
            var list = Traverse.Create(view).Field("inventoryViews").GetValue() as System.Collections.IEnumerable;
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
                foreach (var inv in GetPlayerInventories())
                {
                    if (inv.AddResource(name, amount)) return;
                }
            });
        }

        private static string ReadCurrentAmount(ResourceName name)
        {
            try
            {
                var pm = PlayerManager.instance;
                if (pm == null) return "(no PM)";
                IInventoryView? proxy = pm.playerInventory?.GetInventory();
                if (proxy == null) return "(no proxy)";
                return proxy.GetExistingResourceAmount(name).ToString();
            }
            catch (System.Exception ex)
            {
                return $"(err: {ex.GetType().Name})";
            }
        }

        // ----------- patch body -----------

        public static void OnGameTick_Postfix(GameManager __instance)
        {
            // Every tick: ensure CheatsRunner is bound to the live GameManager host.
            // Plugin.AttachRunnerTo no-ops once attached; cheap on the hot path.
            if (__instance != null) Plugin.AttachRunnerTo(__instance);
        }
    }
}
