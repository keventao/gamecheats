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
            l.Label("Character Controls", 22f);
            if (_unitManager == null) { l.Label("Waiting for game world..."); return; }
            l.Space(4);

            l.Label("Health Lock (keep full HP):");
            _healthLock = l.Toggle(_healthLock, _healthLock ? "ON" : "OFF");

            l.Space(4);
            l.Label("Mood Lock (keep max mood):");
            _moodLock = l.Toggle(_moodLock, _moodLock ? "ON" : "OFF");
            if (_moodLock) l.Label("  WIP: needs mood attribute field name.", 18f);

            l.Space(4);
            l.Label("No Aging:");
            _noAging = l.Toggle(_noAging, _noAging ? "ON" : "OFF");
            if (_noAging) l.Label("  WIP: needs growth/age attribute.", 18f);
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
