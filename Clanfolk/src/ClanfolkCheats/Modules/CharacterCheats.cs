using System;
using System.Collections;
using System.Reflection;
using ClanfolkCheats.Core;
using HarmonyLib;
using MelonLoader;

namespace ClanfolkCheats.Modules
{
    public class CharacterCheats : ICheatModule
    {
        public string Name => "角色";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        private const float SpeedMultiplier = 3f;

        private bool _moodLock;
        private bool _speedBoost;
        private bool _speedNeedsReset;
        private bool _noAging;

        private object? _unitManager;
        private bool _triedInit;

        // cached reflection members (resolved against the runtime Human type once)
        private PropertyInfo? _humanListProp;
        private MethodInfo? _getMoodAttribute;
        private MethodInfo? _setAttributeProgress;
        private PropertyInfo? _unitSpeedMultProp;

        public void Register(HarmonyLib.Harmony harmony)
        {
            MelonLogger.Msg("[Char] Registered — will init when game world loads.");
        }

        public void DrawGui(Layout l)
        {
            l.Label("角色控制", 22f);
            if (_unitManager == null) { l.Label("等待游戏世界加载…"); return; }
            l.Space(4);

            l.Label("心情锁满:");
            _moodLock = l.Toggle(_moodLock, _moodLock ? "开" : "关");

            l.Space(4);
            l.Label($"移动速度 {SpeedMultiplier:0}倍:");
            var prev = _speedBoost;
            _speedBoost = l.Toggle(_speedBoost, _speedBoost ? "开" : "关");
            if (prev && !_speedBoost) _speedNeedsReset = true;   // toggled off — restore on next tick

            l.Space(4);
            l.Label("停止衰老:");
            _noAging = l.Toggle(_noAging, _noAging ? "开" : "关");
            if (_noAging) l.Label("  开发中: 需要成长/年龄属性。", 18f);
        }

        public void OnUpdate()
        {
            if (!_triedInit)
            {
                _triedInit = true;
                TryInit();
            }

            if (!_moodLock && !_speedBoost && !_speedNeedsReset) return;
            if (_unitManager == null) return;

            try
            {
                var humanList = GetHumanList();
                if (humanList == null) return;

                foreach (var unit in humanList)
                {
                    if (unit == null) continue;
                    if (_moodLock) LockMood(unit);
                    if (_speedBoost) SetSpeed(unit, SpeedMultiplier);
                    else if (_speedNeedsReset) SetSpeed(unit, 1f);
                }

                // one restore pass is enough; stop fighting the game afterwards
                if (_speedNeedsReset && !_speedBoost) _speedNeedsReset = false;
            }
            catch { }
        }

        private IList? GetHumanList()
        {
            if (_humanListProp == null)
                _humanListProp = AccessTools.Property(_unitManager!.GetType(), "humanList");
            if (_humanListProp != null)
                return _humanListProp.GetValue(_unitManager) as IList;

            // fallback: some IL2Cpp proxies expose it as a field
            var f = AccessTools.Field(_unitManager!.GetType(), "humanList");
            return f?.GetValue(_unitManager) as IList;
        }

        // unit.GetMoodAttribute().SetAttributeProgress(1f)  — AttributeMood : AttributeGeneric
        private void LockMood(object unit)
        {
            if (_getMoodAttribute == null)
                _getMoodAttribute = AccessTools.Method(unit.GetType(), "GetMoodAttribute");
            var mood = _getMoodAttribute?.Invoke(unit, null);
            if (mood == null) return;

            if (_setAttributeProgress == null)
                _setAttributeProgress = AccessTools.Method(mood.GetType(), "SetAttributeProgress");
            _setAttributeProgress?.Invoke(mood, new object[] { 1f });
        }

        // Unit.unitSpeedMult is a runtime movement multiplier (exposed as a property
        // by Il2CppInterop). Writing it each tick keeps the boost applied.
        private void SetSpeed(object unit, float mult)
        {
            if (_unitSpeedMultProp == null)
                _unitSpeedMultProp = AccessTools.Property(unit.GetType(), "unitSpeedMult");
            _unitSpeedMultProp?.SetValue(unit, mult);
        }

        private void TryInit()
        {
            var gm = GameRefs.GetGameManager();
            if (gm == null) { _triedInit = false; return; }

            try
            {
                var getUM = AccessTools.Method(gm.GetType(), "GetUnitManager");
                if (getUM != null)
                    _unitManager = getUM.Invoke(gm, null);

                if (_unitManager != null)
                {
                    Status = ModuleStatus.Ok;
                    MelonLogger.Msg("[Char] OK — UnitManager found");
                }
            }
            catch (Exception ex) { MelonLogger.Warning($"[Char] {ex.Message}"); _triedInit = false; }
        }
    }
}
