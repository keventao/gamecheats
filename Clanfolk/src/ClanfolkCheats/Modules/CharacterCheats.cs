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

        private bool _healthLock;
        private bool _moodLock;
        private bool _noAging;

        private object? _unitManager;
        private bool _triedInit;

        public void Register(HarmonyLib.Harmony harmony)
        {
            MelonLogger.Msg("[Char] Registered — will init when game world loads.");
        }

        public void DrawGui(Layout l)
        {
            l.Label("角色控制", 22f);
            if (_unitManager == null) { l.Label("等待游戏世界加载…"); return; }
            l.Space(4);

            l.Label("生命锁满:");
            _healthLock = l.Toggle(_healthLock, _healthLock ? "开" : "关");

            l.Space(4);
            l.Label("心情锁满:");
            _moodLock = l.Toggle(_moodLock, _moodLock ? "开" : "关");
            if (_moodLock) l.Label("  开发中: 需要心情属性字段名。", 18f);

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

            if (!_healthLock && !_moodLock) return;
            if (_unitManager == null) return;

            try
            {
                var humanListField = AccessTools.Field(_unitManager.GetType(), "humanList");
                if (humanListField == null) return;
                var humanList = humanListField.GetValue(_unitManager) as IList;
                if (humanList == null) return;

                foreach (var unit in humanList)
                {
                    if (unit == null) continue;

                    if (_healthLock)
                    {
                        var attrsField = AccessTools.Field(unit.GetType(), "myEntityAttributes");
                        if (attrsField != null)
                        {
                            var attrs = attrsField.GetValue(unit);
                            if (attrs != null)
                            {
                                var healthField = AccessTools.Field(attrs.GetType(), "myHealth");
                                if (healthField != null)
                                {
                                    var health = healthField.GetValue(attrs);
                                    if (health != null)
                                    {
                                        var setHP = AccessTools.Method(health.GetType(), "SetHealthPercent");
                                        setHP?.Invoke(health, new object[] { 1f });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
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
