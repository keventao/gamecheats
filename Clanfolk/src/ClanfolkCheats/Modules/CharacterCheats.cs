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

        private const float SpeedMultiplier = 5f;

        private bool _moodLock;
        private bool _sleepLock;
        private bool _speedBoost;
        private bool _noAging;

        private object? _unitManager;
        private bool _triedInit;

        // cached reflection members (resolved against the runtime Human type once)
        private PropertyInfo? _humanListProp;
        private MethodInfo? _getSleepAttribute;
        private MethodInfo? _setSleepProgress;

        // read by the Harmony postfixes; OnUpdate keeps them in sync with the toggles
        private static float _sSpeedMult = 1f;
        private static bool _sMoodLock;

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

            // Mood: the GetCurrentValue() reader patch never fired (game reads the native
            // field directly, not via the method), and value-writes from OnUpdate got pulled
            // back. So intercept the WRITE path instead: prefix the two methods that mutate
            // an attribute's stored value, filtered to AttributeMood —
            //   SetCurrentValue(int val)   -> force val to max
            //   ChangeCurrentValue(int chg)-> block decreases (chg<0 => 0)
            // Combined with the per-tick MaxCurrentValue kick in the lock loop, mood rises to
            // max and the cap pull-back (which goes through these setters) can't lower it.
            try
            {
                _moodType = AccessTools.TypeByName("Il2Cpp.AttributeMood");
                var baseType = AccessTools.TypeByName("Il2Cpp.AttributeGeneric");
                var setCur = baseType != null ? AccessTools.Method(baseType, "SetCurrentValue", new[] { typeof(int) }) : null;
                var changeCur = baseType != null ? AccessTools.Method(baseType, "ChangeCurrentValue", new[] { typeof(int) }) : null;
                _moodGetMaxBase = _moodType != null ? AccessTools.Method(_moodType, "GetMaxValueBase") : null;
                if (_moodType != null && setCur != null && changeCur != null)
                {
                    harmony.Patch(setCur, prefix: new HarmonyMethod(typeof(CharacterCheats), nameof(Prefix_MoodSetCurrentValue)));
                    harmony.Patch(changeCur, prefix: new HarmonyMethod(typeof(CharacterCheats), nameof(Prefix_MoodChangeCurrentValue)));
                    MelonLogger.Msg($"[Char] Patched mood SetCurrentValue + ChangeCurrentValue (maxBase={_moodGetMaxBase != null})");
                }
                else
                {
                    MelonLogger.Warning($"[Char] mood patch deps missing (type={_moodType != null} set={setCur != null} change={changeCur != null})");
                }
            }
            catch (Exception ex) { MelonLogger.Error($"[Char] patch mood setters: {ex.Message}"); }

            MelonLogger.Msg("[Char] Registered — will init when game world loads.");
        }

        private static System.Type? _moodType;
        private static MethodInfo? _moodGetMaxBase;
        private static int _moodMaxCache = -1;
        [ThreadStatic] private static bool _inMoodMax;
        private static bool _moodSetDiag;
        private static bool _moodChgDiag;

        private static int MoodMax(object inst)
        {
            if (_moodMaxCache > 0 || _inMoodMax) return _moodMaxCache;
            _inMoodMax = true;
            try { if (_moodGetMaxBase?.Invoke(inst, null) is int mb && mb > 0) _moodMaxCache = mb; }
            catch { }
            finally { _inMoodMax = false; }
            return _moodMaxCache;
        }

        // Force every write to a locked mood instance up to its max.
        private static void Prefix_MoodSetCurrentValue(object __instance, ref int val)
        {
            if (!_sMoodLock || __instance == null || _moodType == null || !_moodType.IsInstanceOfType(__instance)) return;
            int max = MoodMax(__instance);
            if (!_moodSetDiag) { _moodSetDiag = true; MelonLogger.Msg($"[Char] MoodSet fired: val={val} max={max}"); }
            if (max > 0) val = max;
        }

        // Block any decrease to a locked mood instance.
        private static void Prefix_MoodChangeCurrentValue(object __instance, ref int change)
        {
            if (!_sMoodLock || __instance == null || _moodType == null || !_moodType.IsInstanceOfType(__instance)) return;
            if (!_moodChgDiag) { _moodChgDiag = true; MelonLogger.Msg($"[Char] MoodChange fired: change={change}"); }
            if (change < 0) change = 0;
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
            l.Label("睡眠锁满(不用睡):");
            _sleepLock = l.Toggle(_sleepLock, _sleepLock ? "开" : "关");

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
            _sMoodLock = _moodLock;   // read by the mood setter prefixes

            if (!_triedInit)
            {
                _triedInit = true;
                TryInit();
            }

            if (!_moodLock && !_sleepLock) return;
            if (_unitManager == null) return;

            try
            {
                EnsureUnitEntityManager();

                // humanList is an Il2CppSystem List<ulong> of entity IDs — NOT a
                // System.Collections.IList (old `as IList` always returned null, so the
                // lock loop never ran), and NOT a list of Unit objects (elements are IDs
                // that must be resolved via EntityManager.GetEntity(id)).
                var rawList = GetHumanListRaw();
                if (rawList == null || _unitEntityManager == null || _emGetEntity == null) return;

                var lt = rawList.GetType();
                _humanListCount ??= lt.GetProperty("Count");
                _humanListItem ??= lt.GetProperty("Item");
                if (_humanListCount == null || _humanListItem == null)
                {
                    if (!_lockDiagLogged) { _lockDiagLogged = true; MelonLogger.Warning($"[Char] humanList not indexable: {lt.FullName}"); }
                    return;
                }

                int n = _humanListCount.GetValue(rawList) is int c ? c : 0;
                if (!_lockDiagLogged)
                {
                    _lockDiagLogged = true;
                    MelonLogger.Msg($"[Char] Lock loop: {n} humans, EM={_unitEntityManager.GetType().Name}");
                }

                var idArg = new object[1];
                for (int i = 0; i < n; i++)
                {
                    var id = _humanListItem.GetValue(rawList, new object[] { i });
                    if (id == null) continue;
                    idArg[0] = id;
                    var unit = _emGetEntity.Invoke(_unitEntityManager, idArg);
                    if (unit == null) continue;
                    if (_moodLock) LockMood(unit);
                    if (_sleepLock) LockSleep(unit);
                }
            }
            catch (Exception ex)
            {
                if (!_lockDiagLogged) { _lockDiagLogged = true; MelonLogger.Warning($"[Char] Lock loop: {ex.Message}"); }
            }
        }

        private PropertyInfo? _humanListCount;
        private PropertyInfo? _humanListItem;
        private bool _lockDiagLogged;
        private object? _unitEntityManager;
        private MethodInfo? _emGetEntity;

        // Resolve the Unit-class EntityManager once: GameManager.GetEntityManager(EntityClass.Unit),
        // then cache its GetEntity(ulong)->Entity to turn humanList IDs into live units.
        private void EnsureUnitEntityManager()
        {
            if (_unitEntityManager != null) return;
            var gm = GameRefs.GetGameManager();
            if (gm == null) return;

            var gmType = gm.GetType();
            var ecType = gmType.Assembly.GetType("Il2Cpp.EntityClass") ?? GameRefs.ResolveType("EntityClass");
            if (ecType == null || !ecType.IsEnum) return;

            var unitEC = Enum.Parse(ecType, "Unit");
            var getEM = gmType.GetMethod("GetEntityManager", new[] { ecType });
            _unitEntityManager = getEM?.Invoke(gm, new[] { unitEC });
            if (_unitEntityManager != null)
                _emGetEntity = AccessTools.Method(_unitEntityManager.GetType(), "GetEntity", new[] { typeof(ulong) });
        }

        private object? GetHumanListRaw()
        {
            if (_humanListProp == null)
                _humanListProp = AccessTools.Property(_unitManager!.GetType(), "humanList");
            if (_humanListProp != null)
                return _humanListProp.GetValue(_unitManager);

            var f = AccessTools.Field(_unitManager!.GetType(), "humanList");
            return f?.GetValue(_unitManager);
        }

        private MethodInfo? _getMoodAttribute;
        private MethodInfo? _moodMaxCurrent;

        // Per-tick kick: set mood current=max. The SetCurrentValue prefix then keeps every
        // subsequent write pinned at max. Per-unit resolve, cache only non-null (same
        // poisoning fix as sleep).
        private void LockMood(object unit)
        {
            if (_getMoodAttribute == null)
            {
                var m = AccessTools.Method(unit.GetType(), "GetMoodAttribute");
                if (m == null) return;
                _getMoodAttribute = m;
            }
            var mood = _getMoodAttribute.Invoke(unit, null);
            if (mood == null) return;

            if (_moodMaxCurrent == null) _moodMaxCurrent = AccessTools.Method(mood.GetType(), "MaxCurrentValue");
            _moodMaxCurrent?.Invoke(mood, null);
        }

        private bool _sleepDiagLogged;
        private MethodInfo? _sleepMaxCurrent;
        private MethodInfo? _getCurVal;
        private MethodInfo? _getMaxVal;

        // Keep rest maxed so units never get tired / never sleep. AttributeSleep value is
        // high=rested, low=sleepy (ShouldSleep/IsSleepy fire when below threshold), so we
        // push it to max every tick. MaxCurrentValue() sets current=max directly — more
        // reliable than SetAttributeProgress(1f)'s float→value mapping. Logs once so the
        // game log shows whether the attribute actually resolved.
        private void LockSleep(object unit)
        {
            // Resolve per unit and only cache a non-null result. The humanList can yield an
            // entity whose proxy type lacks GetSleepAttribute (caching that null poisoned
            // every later unit and disabled sleep entirely). Skip such units, try the rest.
            if (_getSleepAttribute == null)
            {
                var m = AccessTools.Method(unit.GetType(), "GetSleepAttribute");
                if (m == null) return;
                _getSleepAttribute = m;
            }
            var sleep = _getSleepAttribute.Invoke(unit, null);
            if (sleep == null) return;

            var st = sleep.GetType();
            if (_sleepMaxCurrent == null) _sleepMaxCurrent = AccessTools.Method(st, "MaxCurrentValue");
            if (_setSleepProgress == null) _setSleepProgress = AccessTools.Method(st, "SetAttributeProgress");

            if (!_sleepDiagLogged)
            {
                _sleepDiagLogged = true;
                if (_getCurVal == null) _getCurVal = AccessTools.Method(st, "GetCurrentValue");
                if (_getMaxVal == null) _getMaxVal = AccessTools.Method(st, "GetMaxValue");
                var cv = _getCurVal?.Invoke(sleep, null);
                var mv = _getMaxVal?.Invoke(sleep, null);
                MelonLogger.Msg($"[Char] LockSleep bound: cur={cv} max={mv} maxFn={_sleepMaxCurrent != null} setProg={_setSleepProgress != null}");
            }

            if (_sleepMaxCurrent != null)
                _sleepMaxCurrent.Invoke(sleep, null);
            else
                _setSleepProgress?.Invoke(sleep, new object[] { 1f });
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
