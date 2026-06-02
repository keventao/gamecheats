using System;
using UnityEngine;
using ClanfolkCheats.Core;
using HarmonyLib;
using MelonLoader;

namespace ClanfolkCheats.Modules
{
    public class StorageCheats : ICheatModule
    {
        public string Name => "存储";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Ok;

        private float _capacityMult = 2f;

        public void Register(HarmonyLib.Harmony harmony)
        {
            try
            {
                var itemType = AccessTools.TypeByName("Il2Cpp.Item");
                if (itemType == null) { SetError("Item type not found"); return; }

                var getMaxCount = AccessTools.Method(itemType, "GetMaxCount");
                if (getMaxCount == null) { SetError("Item.GetMaxCount not found"); return; }

                harmony.Patch(getMaxCount,
                    postfix: new HarmonyMethod(typeof(StorageCheats), nameof(Postfix_GetMaxCount)));

                MelonLogger.Msg("[Stor] Patched Item.GetMaxCount");
            }
            catch (Exception ex) { SetError(ex.Message); }
        }

        public void DrawGui(Layout l)
        {
            l.Label("Storage Controls", 22f);
            l.Space(4);

            l.Label($"Capacity Multiplier: {_capacityMult:F1}x");

            var ev = Event.current;
            if (ev != null && ev.type == EventType.ScrollWheel
                && new UnityEngine.Rect(l.X, l.Y - 20f, l.Width, 22f).Contains(ev.mousePosition))
            {
                ev.Use();
                _capacityMult = Math.Clamp(_capacityMult + (ev.delta.y > 0 ? -0.5f : 0.5f), 1f, 100f);
            }

            l.Space(4);
            if (l.Button("1x", 22f)) _capacityMult = 1f;
            if (l.Button("2x", 22f)) _capacityMult = 2f;
            if (l.Button("5x", 22f)) _capacityMult = 5f;
            if (l.Button("10x", 22f)) _capacityMult = 10f;
        }

        private static void Postfix_GetMaxCount(ref int __result)
        {
            // Find StorageCheats instance to read _capacityMult
            // We use a static field for simplicity
            __result = (int)(__result * _sCapacityMult);
        }

        private static float _sCapacityMult = 1f;

        public void OnUpdate()
        {
            _sCapacityMult = _capacityMult;
        }

        private void SetError(string msg)
        {
            Status = ModuleStatus.Broken;
            MelonLogger.Error($"[Stor] {msg}");
        }
    }
}
