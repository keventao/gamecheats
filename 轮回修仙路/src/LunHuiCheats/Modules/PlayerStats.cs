using HarmonyLib;
using LunHuiCheats.Core;
using LunHuiCheats.Core.Gui;
using UnityEngine;

namespace LunHuiCheats.Modules
{
    /// <summary>
    /// Read/write player battle stats on DataLib.UnitData. Each numeric stat has a
    /// "lock" toggle that re-applies the edited value every frame.
    /// </summary>
    public sealed class PlayerStats : ICheatModule
    {
        public string Id => "player";
        public string Name => "角色属性 PlayerStats";
        public string Category => "角色";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        private long _phys, _spell;
        private float _moveSpeed;
        private int _flySpeed;
        private bool _lockPhys, _lockSpell, _lockMove, _lockFly;
        private bool _synced;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            Status = AccessTools.TypeByName("DataLib.UnitData") != null
                ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() { _synced = false; }

        public void OnUpdate()
        {
            var unit = GameRefs.UnitData;
            if (unit == null) return;
            if (_lockPhys)  ReflectAccessor.SetInt64(unit, "curPhysicalAttacks", _phys);
            if (_lockSpell) ReflectAccessor.SetInt64(unit, "curSpellAttacks", _spell);
            if (_lockMove)  ReflectAccessor.SetSingle(unit, "MoveSpeed", _moveSpeed);
            if (_lockFly)   ReflectAccessor.SetInt32(unit, "bigWorldFlySpeed", _flySpeed);
        }

        public void DrawGui()
        {
            var c = new LayoutCursor(0, 0, 360);
            var unit = GameRefs.UnitData;
            if (unit == null) { GuiWidgets.Label(c.Line(), "未找到 UnitData（进入游戏世界后生效）"); return; }

            if (!_synced)
            {
                _phys      = ReflectAccessor.GetInt64(unit, "curPhysicalAttacks");
                _spell     = ReflectAccessor.GetInt64(unit, "curSpellAttacks");
                _moveSpeed = ReflectAccessor.GetSingle(unit, "MoveSpeed");
                _flySpeed  = ReflectAccessor.GetInt32(unit, "bigWorldFlySpeed");
                _synced = true;
            }

            // physical attack
            var l1 = c.Line();
            GuiWidgets.Label(new Rect(l1.x, l1.y, 70, l1.height), "物攻");
            _phys = GuiWidgets.Int64Field(new Rect(l1.x + 72, l1.y, 120, l1.height), "player.phys", _phys);
            _lockPhys = GuiWidgets.Toggle(new Rect(l1.x + 200, l1.y, 90, l1.height), _lockPhys, "锁定");

            // spell attack
            var l2 = c.Line();
            GuiWidgets.Label(new Rect(l2.x, l2.y, 70, l2.height), "法攻");
            _spell = GuiWidgets.Int64Field(new Rect(l2.x + 72, l2.y, 120, l2.height), "player.spell", _spell);
            _lockSpell = GuiWidgets.Toggle(new Rect(l2.x + 200, l2.y, 90, l2.height), _lockSpell, "锁定");

            // move speed
            var l3 = c.Line();
            GuiWidgets.Label(new Rect(l3.x, l3.y, 70, l3.height), $"移速 {_moveSpeed:0.0}");
            _moveSpeed = GuiWidgets.Slider(new Rect(l3.x + 72, l3.y + 6, 120, l3.height), _moveSpeed, 0f, 50f);
            _lockMove = GuiWidgets.Toggle(new Rect(l3.x + 200, l3.y, 90, l3.height), _lockMove, "锁定");

            // fly speed
            var l4 = c.Line();
            GuiWidgets.Label(new Rect(l4.x, l4.y, 70, l4.height), "飞行速度");
            _flySpeed = (int)GuiWidgets.Int64Field(new Rect(l4.x + 72, l4.y, 120, l4.height), "player.fly", _flySpeed);
            _lockFly = GuiWidgets.Toggle(new Rect(l4.x + 200, l4.y, 90, l4.height), _lockFly, "锁定");

            if (GuiWidgets.Button(c.Line(new Rect(0,0,120,24).height), "立即写入一次"))
            {
                ReflectAccessor.SetInt64(unit, "curPhysicalAttacks", _phys);
                ReflectAccessor.SetInt64(unit, "curSpellAttacks", _spell);
                ReflectAccessor.SetSingle(unit, "MoveSpeed", _moveSpeed);
                ReflectAccessor.SetInt32(unit, "bigWorldFlySpeed", _flySpeed);
            }
        }

        public void DisableAll()
        {
            _lockPhys = _lockSpell = _lockMove = _lockFly = false;
        }
    }
}
