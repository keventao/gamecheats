using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using HumanicaCheats.Core;

namespace HumanicaCheats.Modules
{
    public class VillageCheats : ICheatModule
    {
        public string       Name   => "村庄";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        // ── Toggle 状态 ──────────────────────────────────────────────
        // static：供 Harmony patch postfix 静态方法访问
        internal static bool BuildSpeedX10   = false;
        internal static bool ProductionX10   = false;
        internal static bool SelfPlantedCropGrowthX10 = false;
        internal static int VillagerMoveSpeedMultiplier = 0;

        private static MelonPreferences_Category? _prefs;
        private static MelonPreferences_Entry<bool>? _buildSpeedPref;
        private static MelonPreferences_Entry<bool>? _productionSpeedPref;
        private static MelonPreferences_Entry<bool>? _selfPlantedCropGrowthPref;
        private static MelonPreferences_Entry<int>? _villagerMoveSpeedMultiplierPref;

        // Patch 状态标注（运行时确认）
        private bool _buildPatchOk      = false;
        private bool _productionPatchOk = false;
        private bool _cropGrowthPatchOk = false;
        private bool _villagerMovePatchOk = false;
        private static bool _cropReflectionWarningLogged = false;
        private static bool _villagerMoveReflectionWarningLogged = false;
        private static int _buildSpeedDebugLogsLeft = 8;
        private static int _villagerMoveDebugLogsLeft = 8;

        public void Register(HarmonyLib.Harmony harmony)
        {
            _prefs = MelonPreferences.CreateCategory("HumanicaCheats");
            _buildSpeedPref = _prefs.CreateEntry("village_build_speed_x10", false);
            _productionSpeedPref = _prefs.CreateEntry("village_production_speed_x10", false);
            _selfPlantedCropGrowthPref = _prefs.CreateEntry("village_self_planted_crop_growth_x10", false);
            _villagerMoveSpeedMultiplierPref = _prefs.CreateEntry("village_move_speed_multiplier", 0);
            BuildSpeedX10 = _buildSpeedPref.Value;
            ProductionX10 = _productionSpeedPref.Value;
            SelfPlantedCropGrowthX10 = _selfPlantedCropGrowthPref.Value;
            VillagerMoveSpeedMultiplier = NormalizeVillagerMoveSpeedMultiplier(_villagerMoveSpeedMultiplierPref.Value);
            if (_villagerMoveSpeedMultiplierPref.Value != VillagerMoveSpeedMultiplier)
            {
                _villagerMoveSpeedMultiplierPref.Value = VillagerMoveSpeedMultiplier;
                _prefs.SaveToFile(false);
            }

            // ── 建造速度 patch ───────────────────────────────────────
            // 目标：Il2CppGameCore.Features.ResourceManagement.ConstructionProduction
            //        .CalculateProgressPerTimeStep — 私有方法，返回 float
            // 二进制反射确认存在 (2026-05-01)；用 AccessTools.TypeByName 运行时绑定
            // （避免编译期依赖完整命名空间路径）
            try
            {
                var buildType = typeof(Il2CppGameCore.Features.ResourceManagement.ConstructionProduction);
                var buildMethod = AccessTools.Method(buildType, "CalculateProgressPerTimeStep");
                if (buildMethod == null) throw new Exception("CalculateProgressPerTimeStep not found");
                harmony.Patch(buildMethod,
                    postfix: new HarmonyMethod(typeof(VillageCheats), nameof(BuildTime_Postfix)));
                _buildPatchOk = true;
                MelonLogger.Msg("[VillageCheats] 建造速度 patch OK (ConstructionProduction.CalculateProgressPerTimeStep)");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[VillageCheats] 建造速度 patch 失败（toggle 运行时无效）: {ex.Message}");
            }

            try
            {
                var progressGetter = FindConstructionProgressGetter();
                if (progressGetter == null) throw new Exception("ConstructionComponent.get_ProgressPerTimeStep not found");
                harmony.Patch(progressGetter,
                    postfix: new HarmonyMethod(typeof(VillageCheats), nameof(BuildTime_Postfix)));
                _buildPatchOk = true;
                MelonLogger.Msg($"[VillageCheats] 建造进度 getter patch OK ({progressGetter.DeclaringType?.FullName}.{progressGetter.Name})");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[VillageCheats] 建造进度 getter patch 失败: {ex.Message}");
            }

            try
            {
                var progressSetter = FindConstructionProgressSetter();
                if (progressSetter == null) throw new Exception("ConstructionComponent.SetProgressPerTimeStep not found");
                harmony.Patch(progressSetter,
                    prefix: new HarmonyMethod(typeof(VillageCheats), nameof(BuildProgressSetter_Prefix)));
                _buildPatchOk = true;
                MelonLogger.Msg($"[VillageCheats] 建造进度 setter patch OK ({progressSetter.DeclaringType?.FullName}.{progressSetter.Name})");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[VillageCheats] 建造进度 setter patch 失败: {ex.Message}");
            }

            // ── 生产速度 patch ───────────────────────────────────────
            // 目标：Il2CppGameCore.Features.Buffs.BuffController
            //        .GetSumProduceMultiplier — 公开无参方法，返回 float
            // 二进制反射确认存在 (2026-05-01)；用 AccessTools.TypeByName 运行时绑定
            try
            {
                var buffType = typeof(Il2CppGameCore.Features.Buffs.BuffController);
                var prodMethod = AccessTools.Method(buffType, "GetSumProduceMultiplier");
                if (prodMethod == null) throw new Exception("GetSumProduceMultiplier not found");
                harmony.Patch(prodMethod,
                    postfix: new HarmonyMethod(typeof(VillageCheats), nameof(ProdSpeed_Postfix)));
                _productionPatchOk = true;
                MelonLogger.Msg("[VillageCheats] 生产速度 patch OK (BuffController.GetSumProduceMultiplier)");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[VillageCheats] 生产速度 patch 失败（toggle 运行时无效）: {ex.Message}");
            }

            try
            {
                var cropMethod = FindCropGrowthMethod();
                if (cropMethod == null) throw new Exception("crop growth method not found");
                harmony.Patch(cropMethod,
                    postfix: new HarmonyMethod(typeof(VillageCheats), nameof(SelfPlantedCropGrowth_Postfix)));
                _cropGrowthPatchOk = true;
                MelonLogger.Msg($"[VillageCheats] 自种作物生长 patch OK ({cropMethod.DeclaringType?.FullName}.{cropMethod.Name})");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[VillageCheats] 自种作物生长 patch 失败（toggle 运行时无效）: {ex.Message}");
            }

            try
            {
                var moveMethod = FindVillagerMoveSpeedMethod();
                if (moveMethod == null) throw new Exception("villager move speed method not found");
                harmony.Patch(moveMethod,
                    postfix: new HarmonyMethod(typeof(VillageCheats), nameof(VillagerMoveSpeed_Postfix)));
                _villagerMovePatchOk = true;
                MelonLogger.Msg($"[VillageCheats] 己方村民移动速度 patch OK ({moveMethod.DeclaringType?.FullName}.{moveMethod.Name})");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[VillageCheats] 己方村民移动速度 patch 失败（toggle 运行时无效）: {ex.Message}");
            }

            // 全部 patch 失败 → Broken (UI 显示 (!) 警告)
            // 部分失败 → Ok (toggle 部分可用)
            Status = (_buildPatchOk || _productionPatchOk || _cropGrowthPatchOk || _villagerMovePatchOk) ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void DrawGui(Layout l)
        {
            if (!GameRefs.IsReady) { l.Label("等待游戏加载…"); return; }

            // ── 人口 ─────────────────────────────────────────────────
            l.Label("人口");
            if (l.Button("立即添加 1 名村民", 26f))
                TrySpawnVillager();

            l.Space(8);

            // ── 建造速度 ─────────────────────────────────────────────
            if (!_buildPatchOk)
                l.Label("[!] 建造 patch 未绑定 — 查看 MelonLoader 控制台");
            bool previousBuildSpeed = BuildSpeedX10;
            BuildSpeedX10 = l.Toggle(BuildSpeedX10, "建造速度 ×10");
            if (BuildSpeedX10 != previousBuildSpeed)
            {
                SaveVillagePrefs();
            }

            // ── 生产速度 ─────────────────────────────────────────────
            if (!_productionPatchOk)
                l.Label("[!] 生产 patch 未绑定 — 查看 MelonLoader 控制台");
            bool previousProductionSpeed = ProductionX10;
            ProductionX10 = l.Toggle(ProductionX10, "生产速度 ×10");
            if (ProductionX10 != previousProductionSpeed)
            {
                SaveVillagePrefs();
            }

            // ── 自己种植作物生长 ─────────────────────────────────────
            if (!_cropGrowthPatchOk)
                l.Label("[!] 作物生长 patch 未绑定 — 查看 MelonLoader 控制台");
            bool previousCropGrowth = SelfPlantedCropGrowthX10;
            SelfPlantedCropGrowthX10 = l.Toggle(SelfPlantedCropGrowthX10, "自己种植作物生长 ×10");
            if (SelfPlantedCropGrowthX10 != previousCropGrowth)
            {
                SaveVillagePrefs();
            }

            if (!_villagerMovePatchOk)
                l.Label("[!] 村民移动 patch 未绑定 — 查看 MelonLoader 控制台");
            DrawVillagerMoveSpeedRow(l);
        }

        private static void DrawVillagerMoveSpeedRow(Layout l)
        {
            const float h = 24f;
            const float gap = 6f;
            float labelW = Math.Min(150f, l.Width * 0.46f);
            float buttonW = (l.Width - labelW - gap * 2f) / 2f;
            float y = l.Y;

            GUI.Label(new Rect(l.X, y + 2f, labelW, h), "己方村民移动速度");
            if (ImguiUtil.Button(new Rect(l.X + labelW + gap, y, buttonW, h), "2倍", VillagerMoveSpeedMultiplier == 2))
            {
                VillagerMoveSpeedMultiplier = VillagerMoveSpeedMultiplier == 2 ? 0 : 2;
                SaveVillagePrefs();
                MelonLogger.Msg($"[VillageCheats] 己方村民移动速度倍率 = x{(VillagerMoveSpeedMultiplier == 0 ? 1 : VillagerMoveSpeedMultiplier)}");
            }

            if (ImguiUtil.Button(new Rect(l.X + labelW + gap + buttonW + gap, y, buttonW, h), "5倍", VillagerMoveSpeedMultiplier == 5))
            {
                VillagerMoveSpeedMultiplier = VillagerMoveSpeedMultiplier == 5 ? 0 : 5;
                SaveVillagePrefs();
                MelonLogger.Msg($"[VillageCheats] 己方村民移动速度倍率 = x{(VillagerMoveSpeedMultiplier == 0 ? 1 : VillagerMoveSpeedMultiplier)}");
            }

            l.Y += h + 4f;
        }

        // ── 添加村民 ─────────────────────────────────────────────────
        // 服务定位器 Il2Cpp.S.CreatureManager 取单例，调用无参 SpawnRandomVillager()。
        // 类型：Il2CppGameCore.EntityProviders.CreatureManager（二进制反射确认）。
        // Il2Cpp.S.CreatureManager 已通过与 Il2Cpp.S.VillageData 相同的 get_CreatureManager
        // 静态 getter 确认存在（在同一 NativeMethodInfoPtr 表）。
        private static void TrySpawnVillager()
        {
            try
            {
                var cm = Il2Cpp.S.CreatureManager;
                if (cm == null)
                {
                    MelonLogger.Warning("[VillageCheats] Il2Cpp.S.CreatureManager 为 null — 游戏世界未加载?");
                    return;
                }
                cm.SpawnRandomVillager();
                MelonLogger.Msg("[VillageCheats] SpawnRandomVillager 已调用");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[VillageCheats] TrySpawnVillager 失败: {ex.Message}");
            }
        }

        // ── Harmony Postfix: 建造进度 ────────────────────────────────
        // 假设 CalculateProgressPerTimeStep 返回"每步进度速率"(非剩余工期)。
        // 若假设成立: __result *= 10f → 进度速率 ×10 → 工期缩短至 1/10。
        // TODO: 游戏内需确认 — 启用 toggle 后建筑完成时间是否变短(而非变长)。
        // 若变长说明返回的是工期而非进度,改为 __result /= 10f。
        private static void BuildTime_Postfix(ref float __result)
        {
            if (!BuildSpeedX10)
            {
                return;
            }

            var original = __result;
            __result *= 10f;
            LogBuildSpeedHit("calc", original, __result);
        }

        private static void BuildProgressSetter_Prefix(ref float __0)
        {
            if (!BuildSpeedX10)
            {
                return;
            }

            var original = __0;
            __0 *= 10f;
            LogBuildSpeedHit("set", original, __0);
        }

        private static void LogBuildSpeedHit(string source, float original, float modified)
        {
            if (_buildSpeedDebugLogsLeft <= 0)
            {
                return;
            }

            _buildSpeedDebugLogsLeft--;
            MelonLogger.Msg($"[VillageCheats] BuildSpeed hit {source}, {original:0.###}->{modified:0.###}");
        }

        // ── Harmony Postfix: 生产倍率 ────────────────────────────────
        // GetSumProduceMultiplier 返回当前生产速度倍率(基础值通常为 1.0)。
        // ×10 → 所有工坊生产速度 ×10。
        private static void ProdSpeed_Postfix(ref float __result)
        {
            if (ProductionX10) __result *= 10f;
        }

        // ── Harmony Postfix: 自己种植作物生长 ────────────────────────
        private static void SelfPlantedCropGrowth_Postfix(object __instance, ref float __result)
        {
            if (!SelfPlantedCropGrowthX10)
            {
                return;
            }

            __result = CropGrowthPolicy.ApplyMultiplier(
                __result,
                enabled: true,
                hasPlantGrowingTrigger: TryHasPlantGrowingTrigger(__instance),
                isPlayerPlanted: TryIsPlayerPlanted(__instance));
        }

        // ── Harmony Postfix: 己方村民移动速度 ─────────────────────────
        private static void VillagerMoveSpeed_Postfix(object __instance, ref float __result)
        {
            var multiplier = VillagerMoveSpeedMultiplier;
            if (multiplier != 2 && multiplier != 5)
            {
                return;
            }

            var isPlayerVillager = TryIsPlayerVillager(__instance);
            var original = __result;
            __result = VillagerMoveSpeedPolicy.ApplyMultiplier(__result, multiplier, isPlayerVillager);

            if (_villagerMoveDebugLogsLeft > 0)
            {
                _villagerMoveDebugLogsLeft--;
                MelonLogger.Msg($"[VillageCheats] MoveSpeed hit mult=x{multiplier}, own={isPlayerVillager}, {original:0.###}->{__result:0.###}, type={__instance.GetType().FullName}");
            }
        }

        private static void SaveVillagePrefs()
        {
            if (_prefs == null
                || _buildSpeedPref == null
                || _productionSpeedPref == null
                || _selfPlantedCropGrowthPref == null
                || _villagerMoveSpeedMultiplierPref == null)
            {
                return;
            }

            _buildSpeedPref.Value = BuildSpeedX10;
            _productionSpeedPref.Value = ProductionX10;
            _selfPlantedCropGrowthPref.Value = SelfPlantedCropGrowthX10;
            _villagerMoveSpeedMultiplierPref.Value = NormalizeVillagerMoveSpeedMultiplier(VillagerMoveSpeedMultiplier);
            _prefs.SaveToFile(false);
        }

        private static int NormalizeVillagerMoveSpeedMultiplier(int multiplier)
        {
            return multiplier == 2 || multiplier == 5 ? multiplier : 0;
        }

        private static MethodInfo? FindCropGrowthMethod()
        {
            var types = new[]
            {
                typeof(Il2CppGameCore.Features.ResourceManagement.ResourceDeposit)
            };
            var methodNames = new[]
            {
                "RecoverySpeed",
                "AdditionalRecoverySpeed",
                "GetGrowProgress"
            };

            foreach (var type in types)
            {
                foreach (var methodName in methodNames)
                {
                    var method = AccessTools.Method(type, methodName);
                    if (method != null && method.ReturnType == typeof(float))
                    {
                        return method;
                    }
                }
            }

            return null;
        }

        private static MethodInfo? FindVillagerMoveSpeedMethod()
        {
            var type = typeof(Il2CppGameCore.Features.Movement.MoveController);
            var method = AccessTools.Method(type, "CalculateMoveSpeed");
            return method != null && method.ReturnType == typeof(float) ? method : null;
        }

        private static MethodInfo? FindConstructionProgressGetter()
        {
            foreach (var type in SafeGameTypes())
            {
                if (type.Name != "ConstructionComponent")
                {
                    continue;
                }

                var method = AccessTools.Method(type, "get_ProgressPerTimeStep");
                if (method != null && method.ReturnType == typeof(float))
                {
                    return method;
                }
            }

            return null;
        }

        private static MethodInfo? FindConstructionProgressSetter()
        {
            foreach (var type in SafeGameTypes())
            {
                if (type.Name != "ConstructionComponent")
                {
                    continue;
                }

                var method = AccessTools.Method(type, "SetProgressPerTimeStep");
                if (method == null || method.ReturnType != typeof(void))
                {
                    continue;
                }

                var parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(float))
                {
                    return method;
                }
            }

            return null;
        }

        private static IEnumerable<Type> SafeGameTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = assembly.GetName().Name;
                if (name != "Assembly-CSharp")
                {
                    continue;
                }

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
                }

                foreach (var type in types)
                {
                    yield return type;
                }
            }
        }

        private static bool TryHasPlantGrowingTrigger(object? instance)
        {
            try
            {
                return TryGetBoolMember(instance, "HasPlantGrowingTrigger")
                    || TryGetBoolMember(instance, "PlantGrowingStartTrigger")
                    || TryGetBoolMember(instance, "hasPlantGrowingTrigger")
                    || TryGetBoolMember(instance, "_hasPlantGrowingTrigger");
            }
            catch (Exception ex)
            {
                LogCropReflectionWarningOnce(ex);
                return false;
            }
        }

        private static bool TryIsPlayerPlanted(object? instance)
        {
            try
            {
                var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
                return TryFindPlantedFlag(instance, visited, 0);
            }
            catch (Exception ex)
            {
                LogCropReflectionWarningOnce(ex);
                return false;
            }
        }

        private static bool TryIsPlayerVillager(object? instance)
        {
            if (instance == null)
            {
                return false;
            }

            try
            {
                var villagers = TryGetPlayerVillagers();
                var generalAIs = TryGetPlayerGeneralAIs();
                if (generalAIs != null)
                {
                    var aiVisited = new HashSet<object>(ReferenceEqualityComparer.Instance);
                    if (TryFindObjectInCollection(instance, generalAIs, aiVisited, 0))
                    {
                        return true;
                    }
                }

                if (villagers != null)
                {
                    var villagerVisited = new HashSet<object>(ReferenceEqualityComparer.Instance);
                    if (TryFindObjectInCollection(instance, villagers, villagerVisited, 0))
                    {
                        return true;
                    }

                    if (TryFindVillagerByMoveController(instance, villagers))
                    {
                        return true;
                    }
                }

                if (TryHasMoveControllerGeneralAI(instance))
                {
                    return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogVillagerMoveReflectionWarningOnce(ex);
                return false;
            }
        }

        private static object? TryGetPlayerVillagers()
        {
            var creatureManager = Il2Cpp.S.CreatureManager;
            if (creatureManager == null)
            {
                return null;
            }

            return GetMemberValue(creatureManager.GetType(), creatureManager, "Villagers");
        }

        private static object? TryGetPlayerGeneralAIs()
        {
            var creatureManager = Il2Cpp.S.CreatureManager;
            if (creatureManager == null)
            {
                return null;
            }

            return GetMemberValue(creatureManager.GetType(), creatureManager, "GeneralAIs")
                ?? GetMemberValue(creatureManager.GetType(), creatureManager, "get_GeneralAIs");
        }

        private static bool TryFindVillagerByMoveController(object moveController, object villagers)
        {
            foreach (var villager in EnumerateCollectionItems(villagers))
            {
                if (villager == null)
                {
                    continue;
                }

                var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
                if (TryFindMoveControllerReference(villager, moveController, visited, 0))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryHasMoveControllerGeneralAI(object moveController)
        {
            var type = moveController.GetType();
            foreach (var memberName in VillagerMoveGeneralAIMemberNames)
            {
                if (GetMemberValue(type, moveController, memberName) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryFindMoveControllerReference(object? instance, object moveController, HashSet<object> visited, int depth)
        {
            if (instance == null || depth > 4 || !visited.Add(instance))
            {
                return false;
            }

            if (IsSameObject(instance, moveController))
            {
                return true;
            }

            var type = instance.GetType();
            foreach (var memberName in VillagerMoveGraphMemberNames)
            {
                var value = GetMemberValue(type, instance, memberName);
                if (value != null && TryFindMoveControllerReference(value, moveController, visited, depth + 1))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryFindObjectInCollection(object? instance, object collection, HashSet<object> visited, int depth)
        {
            if (instance == null || depth > 3 || !visited.Add(instance))
            {
                return false;
            }

            if (IsObjectInCollection(collection, instance))
            {
                return true;
            }

            var type = instance.GetType();
            foreach (var memberName in VillagerMoveOwnerMemberNames)
            {
                var value = GetMemberValue(type, instance, memberName);
                if (value != null && TryFindObjectInCollection(value, collection, visited, depth + 1))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsObjectInCollection(object collection, object target)
        {
            foreach (var item in EnumerateCollectionItems(collection))
            {
                if (IsSameObject(item, target))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<object?> EnumerateCollectionItems(object collection)
        {
            if (collection is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    yield return item;
                }

                yield break;
            }

            var countValue = GetMemberValue(collection.GetType(), collection, "Count");
            if (countValue == null)
            {
                yield break;
            }

            var count = Convert.ToInt32(countValue);
            for (var i = 0; i < count; i++)
            {
                yield return GetIndexedMemberValue(collection, i);
            }
        }

        private static object? GetIndexedMemberValue(object instance, int index)
        {
            var type = instance.GetType();
            var property = type.GetProperty("Item", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, new[] { typeof(int) }, null);
            if (property != null)
            {
                return property.GetValue(instance, new object[] { index });
            }

            var method = type.GetMethod("get_Item", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(int) }, null);
            return method?.Invoke(instance, new object[] { index });
        }

        private static bool IsSameObject(object? left, object right)
        {
            return left != null && (ReferenceEquals(left, right) || left.Equals(right));
        }

        private static bool TryFindPlantedFlag(object? instance, HashSet<object> visited, int depth)
        {
            if (instance == null || depth > 2 || !visited.Add(instance))
            {
                return false;
            }

            if (TryGetBoolMember(instance, "Planted") || TryGetBoolMember(instance, "planted"))
            {
                return true;
            }

            var type = instance.GetType();
            foreach (var memberName in CropOwnerMemberNames)
            {
                var value = GetMemberValue(type, instance, memberName);
                if (value != null && TryFindPlantedFlag(value, visited, depth + 1))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetBoolMember(object? instance, string name)
        {
            if (instance == null)
            {
                return false;
            }

            var type = instance.GetType();
            var value = GetMemberValue(type, instance, name);
            return value is bool b && b;
        }

        private static object? GetMemberValue(Type type, object instance, string name)
        {
            var method = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            if (method != null)
            {
                return method.Invoke(instance, null);
            }

            var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null && property.GetIndexParameters().Length == 0)
            {
                return property.GetValue(instance, null);
            }

            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(instance);
        }

        private static void LogCropReflectionWarningOnce(Exception ex)
        {
            if (_cropReflectionWarningLogged)
            {
                return;
            }

            _cropReflectionWarningLogged = true;
            MelonLogger.Warning($"[VillageCheats] 自种作物检测反射失败，作物倍率将安全跳过: {ex.Message}");
        }

        private static void LogVillagerMoveReflectionWarningOnce(Exception ex)
        {
            if (_villagerMoveReflectionWarningLogged)
            {
                return;
            }

            _villagerMoveReflectionWarningLogged = true;
            MelonLogger.Warning($"[VillageCheats] 己方村民检测反射失败，移动倍率将安全跳过: {ex.Message}");
        }

        private static readonly string[] CropOwnerMemberNames =
        {
            "nature",
            "_nature",
            "owner",
            "_owner",
            "entity",
            "_entity",
            "deposit",
            "_deposit",
            "resourceDeposit",
            "_resourceDeposit"
        };

        private static readonly string[] VillagerMoveOwnerMemberNames =
        {
            "Creature",
            "get_Creature",
            "creature",
            "_creature",
            "Owner",
            "get_Owner",
            "owner",
            "_owner",
            "Entity",
            "get_Entity",
            "entity",
            "_entity",
            "GeneralAI",
            "get_GeneralAI",
            "get__generalAI",
            "generalAI",
            "_generalAI",
            "_generalAiInstance",
            "generalAiInstance",
            "get__generalAiInstance",
            "MoveController",
            "get_MoveController",
            "get__moveController",
            "moveController",
            "_moveController",
            "Actor",
            "actor",
            "_actor",
            "Unit",
            "unit",
            "_unit",
            "Parent",
            "parent",
            "_parent"
        };

        private static readonly string[] VillagerMoveGeneralAIMemberNames =
        {
            "GeneralAI",
            "get_GeneralAI",
            "get__generalAI",
            "generalAI",
            "_generalAI",
            "_generalAiInstance",
            "generalAiInstance",
            "get__generalAiInstance"
        };

        private static readonly string[] VillagerMoveGraphMemberNames =
        {
            "MoveController",
            "get_MoveController",
            "moveController",
            "_moveController",
            "GeneralAI",
            "get_GeneralAI",
            "get__generalAI",
            "generalAI",
            "_generalAI",
            "_generalAiInstance",
            "generalAiInstance",
            "get__generalAiInstance",
            "Creature",
            "get_Creature",
            "creature",
            "_creature",
            "Entity",
            "get_Entity",
            "entity",
            "_entity",
            "Owner",
            "get_Owner",
            "owner",
            "_owner",
            "MoveController",
            "get_MoveController",
            "get__moveController",
            "moveController",
            "_moveController"
        };

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

            public new bool Equals(object? x, object? y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
