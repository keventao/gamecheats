using System;
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

        // Patch 状态标注（运行时确认）
        private bool _buildPatchOk      = false;
        private bool _productionPatchOk = false;

        public void Register(HarmonyLib.Harmony harmony)
        {
            // ── 建造速度 patch ───────────────────────────────────────
            // 目标：Il2CppGameCore.Features.ResourceManagement.ConstructionProduction
            //        .CalculateProgressPerTimeStep — 私有方法，返回 float
            // 二进制反射确认存在 (2026-05-01)；用 AccessTools.TypeByName 运行时绑定
            // （避免编译期依赖完整命名空间路径）
            try
            {
                var buildType = AccessTools.TypeByName(
                    "Il2CppGameCore.Features.ResourceManagement.ConstructionProduction");
                if (buildType == null) throw new Exception("ConstructionProduction type not found");
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

            // ── 生产速度 patch ───────────────────────────────────────
            // 目标：Il2CppGameCore.Features.Buffs.BuffController
            //        .GetSumProduceMultiplier — 公开无参方法，返回 float
            // 二进制反射确认存在 (2026-05-01)；用 AccessTools.TypeByName 运行时绑定
            try
            {
                var buffType = AccessTools.TypeByName(
                    "Il2CppGameCore.Features.Buffs.BuffController");
                if (buffType == null) throw new Exception("BuffController type not found");
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

            // 全部 patch 失败 → Broken (UI 显示 (!) 警告)
            // 部分失败 → Ok (toggle 部分可用)
            Status = (_buildPatchOk || _productionPatchOk) ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void DrawGui()
        {
            if (!GameRefs.IsReady) { GUILayout.Label("等待游戏加载…"); return; }

            // ── 人口 ─────────────────────────────────────────────────
            GUILayout.Label("人口");
            if (GUILayout.Button("立即添加 1 名村民"))
                TrySpawnVillager();

            GUILayout.Space(8);

            // ── 建造速度 ─────────────────────────────────────────────
            if (!_buildPatchOk)
                GUILayout.Label("[!] 建造 patch 未绑定 — 查看 MelonLoader 控制台");
            BuildSpeedX10 = GUILayout.Toggle(BuildSpeedX10, "建造速度 ×10");

            // ── 生产速度 ─────────────────────────────────────────────
            if (!_productionPatchOk)
                GUILayout.Label("[!] 生产 patch 未绑定 — 查看 MelonLoader 控制台");
            ProductionX10 = GUILayout.Toggle(ProductionX10, "生产速度 ×10");
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
            if (BuildSpeedX10) __result *= 10f;
        }

        // ── Harmony Postfix: 生产倍率 ────────────────────────────────
        // GetSumProduceMultiplier 返回当前生产速度倍率(基础值通常为 1.0)。
        // ×10 → 所有工坊生产速度 ×10。
        private static void ProdSpeed_Postfix(ref float __result)
        {
            if (ProductionX10) __result *= 10f;
        }
    }
}
