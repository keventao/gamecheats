using HarmonyLib;
using LunHuiCheats.Core;
using LunHuiCheats.Core.Gui;
using UnityEngine;

namespace LunHuiCheats.Modules
{
    /// <summary>Locks the player's curHp to maxHp every frame. Target: DataLib.UnitData.</summary>
    public sealed class GodMode : ICheatModule
    {
        public string Id => "godmode";
        public string Name => "无敌 GodMode";
        public string Category => "战斗";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        private bool _enabled;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            Status = AccessTools.TypeByName("DataLib.UnitData") != null
                ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() { }

        public void OnUpdate()
        {
            if (!_enabled) return;
            var unit = GameRefs.UnitData;
            if (unit == null) return;
            var max = ReflectAccessor.GetInt64(unit, "maxHp");
            if (max > 0) ReflectAccessor.SetInt64(unit, "curHp", max);
        }

        public void DrawGui()
        {
            var c = new LayoutCursor(0, 0, 360);
            _enabled = GuiWidgets.Toggle(c.Line(), _enabled, "锁定满血 (curHp = maxHp)");
            var unit = GameRefs.UnitData;
            GuiWidgets.Label(c.Line(), unit == null
                ? "未找到 UnitData（进入游戏世界后生效）"
                : $"curHp={ReflectAccessor.GetInt64(unit, "curHp")}  maxHp={ReflectAccessor.GetInt64(unit, "maxHp")}");
        }

        public void DisableAll() => _enabled = false;
    }
}
