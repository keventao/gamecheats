using HarmonyLib;
using LunHuiCheats.Core;
using LunHuiCheats.Core.Gui;
using UnityEngine;

namespace LunHuiCheats.Modules
{
    /// <summary>
    /// Player battle stats on DataLib.UnitData (increment model): each stat shows its current
    /// live value + a delta box + "+N" button that sets value = current + delta. 物攻/法攻/移速/飞速.
    /// </summary>
    public sealed class PlayerStats : ICheatModule
    {
        public string Id => "player";
        public string Name => "角色属性 PlayerStats";
        public string Category => "角色";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        private long _physDelta = 1000, _spellDelta = 1000;
        private int _moveDelta = 5, _flyDelta = 5;
        private string _status = "";

        public void Register(ModConfig cfg, Harmony harmony)
        {
            Status = AccessTools.TypeByName("DataLib.UnitData") != null
                ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() { _status = ""; }
        public void OnUpdate() { }   // increment-on-button, nothing locked per frame

        public void DrawGui()
        {
            var c = new LayoutCursor(0, 0, 360);
            var unit = GameRefs.UnitData;
            if (unit == null) { GuiWidgets.Label(c.Line(), "未找到 UnitData（进入游戏世界后生效）"); return; }

            // Read live each frame so "当前值" refreshes after every +N apply.
            long curPhys  = ReflectAccessor.GetInt64(unit, "curPhysicalAttacks");
            long curSpell = ReflectAccessor.GetInt64(unit, "curSpellAttacks");
            float curMove = ReflectAccessor.GetSingle(unit, "MoveSpeed");
            int curFly    = ReflectAccessor.GetInt32(unit, "bigWorldFlySpeed");

            // 物攻
            var r1 = c.Line();
            GuiWidgets.Label(c.Slice(ref r1, 150), $"当前物攻 {curPhys}");
            _physDelta = GuiWidgets.Int64Field(c.Slice(ref r1, 60), "player.physDelta", _physDelta);
            if (GuiWidgets.Button(c.Slice(ref r1, 110), $"物攻 +{_physDelta}"))
            {
                bool ok = ReflectAccessor.TrySet(unit, "curPhysicalAttacks", curPhys + _physDelta);
                _status = $"物攻 {curPhys} → {curPhys + _physDelta} {(ok ? "✓" : "✗")}";
            }

            // 法攻
            var r2 = c.Line();
            GuiWidgets.Label(c.Slice(ref r2, 150), $"当前法攻 {curSpell}");
            _spellDelta = GuiWidgets.Int64Field(c.Slice(ref r2, 60), "player.spellDelta", _spellDelta);
            if (GuiWidgets.Button(c.Slice(ref r2, 110), $"法攻 +{_spellDelta}"))
            {
                bool ok = ReflectAccessor.TrySet(unit, "curSpellAttacks", curSpell + _spellDelta);
                _status = $"法攻 {curSpell} → {curSpell + _spellDelta} {(ok ? "✓" : "✗")}";
            }

            // 移速（Single，整数增量）
            var r3 = c.Line();
            GuiWidgets.Label(c.Slice(ref r3, 150), $"当前移速 {curMove:0.0}");
            _moveDelta = (int)GuiWidgets.Int64Field(c.Slice(ref r3, 60), "player.moveDelta", _moveDelta);
            if (GuiWidgets.Button(c.Slice(ref r3, 110), $"移速 +{_moveDelta}"))
            {
                bool ok = ReflectAccessor.TrySet(unit, "MoveSpeed", curMove + _moveDelta);
                _status = $"移速 {curMove:0.0} → {curMove + _moveDelta:0.0} {(ok ? "✓" : "✗")}";
            }

            // 飞行速度
            var r4 = c.Line();
            GuiWidgets.Label(c.Slice(ref r4, 150), $"当前飞速 {curFly}");
            _flyDelta = (int)GuiWidgets.Int64Field(c.Slice(ref r4, 60), "player.flyDelta", _flyDelta);
            if (GuiWidgets.Button(c.Slice(ref r4, 110), $"飞速 +{_flyDelta}"))
            {
                bool ok = ReflectAccessor.TrySet(unit, "bigWorldFlySpeed", curFly + _flyDelta);
                _status = $"飞速 {curFly} → {curFly + _flyDelta} {(ok ? "✓" : "✗")}";
            }

            if (_status.Length > 0) GuiWidgets.Label(c.Line(), _status);
        }

        public void DisableAll() { }
    }
}
