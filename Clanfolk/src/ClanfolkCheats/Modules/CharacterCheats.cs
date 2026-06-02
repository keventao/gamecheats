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
        private bool _noAging;

        private object? _unitManager;
        private bool _triedInit;

        // cached reflection members (resolved against the runtime Human type once)
        private PropertyInfo? _humanListProp;
        private MethodInfo? _getMoodAttribute;
        private MethodInfo? _setAttributeProgress;

        // read by the Harmony postfix; OnUpdate keeps it in sync with the toggle
        private static float _sSpeedMult = 1f;

        public void Register(HarmonyLib.Harmony harmony)
        {
            // Move speed: postfix Unit.GetMoveSpeed() and scale the final result.
            // This is the value movement actually consumes — writing unitSpeedMult
            // directly had no effect (game recomputes it).
            try
            {
                var unitType = AccessTools.TypeByName("Il2Cpp.Unit");
                var getMoveSpeed = unitType != null ? AccessTools.Method(unitType, "GetMoveSpeed") : null;
                if (getMoveSpeed != null)
                {
                    harmony.Patch(getMoveSpeed,
                        postfix: new HarmonyMethod(typeof(CharacterCheats), nameof(Postfix_GetMoveSpeed)));
                    MelonLogger.Msg("[Char] Patched Unit.GetMoveSpeed");
                }
                else
                {
                    MelonLogger.Warning("[Char] Unit.GetMoveSpeed not found — speed boost disabled");
                }
            }
            catch (Exception ex) { MelonLogger.Error($"[Char] patch GetMoveSpeed: {ex.Message}"); }

            MelonLogger.Msg("[Char] Registered — will init when game world loads.");
        }

        private static void Postfix_GetMoveSpeed(ref float __result)
        {
            __result *= _sSpeedMult;
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
            _speedBoost = l.Toggle(_speedBoost, _speedBoost ? "开" : "关");

            l.Space(4);
            l.Label("停止衰老:");
            _noAging = l.Toggle(_noAging, _noAging ? "开" : "关");
            if (_noAging) l.Label("  开发中: 需要成长/年龄属性。", 18f);
        }

        public void OnUpdate()
        {
            _sSpeedMult = _speedBoost ? SpeedMultiplier : 1f;

            if (!_triedInit)
            {
                _triedInit = true;
                TryInit();
            }

            if (!_moodLock) return;
            if (_unitManager == null) return;

            try
            {
                var humanList = GetHumanList();
                if (humanList == null) return;

                foreach (var unit in humanList)
                {
                    if (unit == null) continue;
                    LockMood(unit);
                }
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
