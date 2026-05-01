using System;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using HumanicaCheats.Core;

namespace HumanicaCheats.Modules
{
    public class ResourceCheats : ICheatModule
    {
        public string       Name   => "资源";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        // ── 锁定状态 ──────────────────────────────────────────────
        private bool _lockWood, _lockStone, _lockFood, _lockGold;
        private const int LockMin = 500;

        // ── ResourceIndex 强类型静态成员 ────────────────────────────
        // Il2Cpp.ResourceIndex 是 int 包装 struct，通过反射确认 value__ 字段存在。
        // 静态成员名通过 Assembly-CSharp.dll 反射枚举确认 (2026-05-01)。
        // 当前值为推断（LOG=木材，STONE_BRICKS=石砖，BREAD=食物，TODO 金币需游戏内验证）。
        // TODO: 游戏内按各资源 +100，对照仓库变化确认 idx 正确性。
        private static readonly Il2Cpp.ResourceIndex WOOD_IDX  = Il2Cpp.ResourceIndex.LOG;         // TODO: 占位,待确认
        private static readonly Il2Cpp.ResourceIndex STONE_IDX = Il2Cpp.ResourceIndex.STONE_BRICKS; // TODO: 占位,待确认
        private static readonly Il2Cpp.ResourceIndex FOOD_IDX  = Il2Cpp.ResourceIndex.BREAD;        // TODO: 占位,待确认
        private static readonly Il2Cpp.ResourceIndex GOLD_IDX  = Il2Cpp.ResourceIndex.TECHNOLOGY_KNOWLEDGE; // TODO: 占位,待确认

        public void Register(HarmonyLib.Harmony harmony)
        {
            // 强类型直接调用，无需 patch 或延迟绑定
            Status = ModuleStatus.Ok;
        }

        // 强类型直接调用 AddResourceIntoFreeWarehouse
        // Il2Cpp.S.VillageData 路径已通过 Task 2 验证
        private static void AddRes(Il2Cpp.ResourceIndex idx, int amount)
        {
            try
            {
                var vd = Il2Cpp.S.VillageData;
                if (vd == null)
                {
                    MelonLogger.Warning("[ResourceCheats] Il2Cpp.S.VillageData 为 null — 游戏世界未加载?");
                    return;
                }
                // AddResourceIntoFreeWarehouse(ResourceIndex idx, int amount, bool createIfNeeded)
                // bool false = 只向现有仓库添加，不新建仓库槽 (推断,需游戏内确认)
                vd.AddResourceIntoFreeWarehouse(idx, amount, false);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[ResourceCheats] AddRes(amount={amount}) 失败: {ex.Message}");
            }
        }

        // 锁定方案 A: 读当前量，不足才补差值
        // GetResourceAmount(ResourceIndex) — 单参数重载已通过二进制扫描确认存在
        private static int GetCurrentAmount(Il2Cpp.ResourceIndex idx)
        {
            try
            {
                var vd = Il2Cpp.S.VillageData;
                if (vd == null) return -1;
                return vd.GetResourceAmount(idx);
            }
            catch
            {
                return -1;
            }
        }

        public void DrawGui()
        {
            if (!GameRefs.IsReady) { GUILayout.Label("等待游戏加载…"); return; }

            // ── 用户警告 ────────────────────────────────────────────────
            // ResourceIndex 静态成员为占位推断（LOG/STONE_BRICKS/BREAD/TECHNOLOGY_KNOWLEDGE），
            // 若点击后仓库无变化，查看 MelonLoader 控制台 [ResourceCheats] 错误行，
            // 并在 refs/01-resource-research.md 记录 dnSpy 确认的实际成员名。
            GUILayout.Label("[!] ResourceIndex 值为占位推断,需游戏内验证");
            GUILayout.Space(4);

            // ── 木材 ────────────────────────────────────────────────────
            GUILayout.Label("木材 (LOG — 占位)");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+100"))  AddRes(WOOD_IDX,  100);
            if (GUILayout.Button("+1000")) AddRes(WOOD_IDX, 1000);
            _lockWood = GUILayout.Toggle(_lockWood, "锁定 ≥500");
            GUILayout.EndHorizontal();

            // ── 石材 ────────────────────────────────────────────────────
            GUILayout.Label("石材 (STONE_BRICKS — 占位)");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+100"))  AddRes(STONE_IDX,  100);
            if (GUILayout.Button("+1000")) AddRes(STONE_IDX, 1000);
            _lockStone = GUILayout.Toggle(_lockStone, "锁定 ≥500");
            GUILayout.EndHorizontal();

            // ── 食物 ────────────────────────────────────────────────────
            GUILayout.Label("食物 (BREAD — 占位)");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+100"))  AddRes(FOOD_IDX,  100);
            if (GUILayout.Button("+1000")) AddRes(FOOD_IDX, 1000);
            _lockFood = GUILayout.Toggle(_lockFood, "锁定 ≥500");
            GUILayout.EndHorizontal();

            // ── 金币 ────────────────────────────────────────────────────
            GUILayout.Label("金币 (TECHNOLOGY_KNOWLEDGE — 占位)");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+100"))  AddRes(GOLD_IDX,  100);
            if (GUILayout.Button("+1000")) AddRes(GOLD_IDX, 1000);
            _lockGold = GUILayout.Toggle(_lockGold, "锁定 ≥500");
            GUILayout.EndHorizontal();
        }

        // 每帧锁定方案 A — 读当前量，不足 LockMin 才补差值，避免每帧无限叠加
        public void OnUpdate()
        {
            if (!GameRefs.IsReady) return;
            if (_lockWood)
            {
                int cur = GetCurrentAmount(WOOD_IDX);
                if (cur >= 0 && cur < LockMin) AddRes(WOOD_IDX, LockMin - cur);
            }
            if (_lockStone)
            {
                int cur = GetCurrentAmount(STONE_IDX);
                if (cur >= 0 && cur < LockMin) AddRes(STONE_IDX, LockMin - cur);
            }
            if (_lockFood)
            {
                int cur = GetCurrentAmount(FOOD_IDX);
                if (cur >= 0 && cur < LockMin) AddRes(FOOD_IDX, LockMin - cur);
            }
            if (_lockGold)
            {
                int cur = GetCurrentAmount(GOLD_IDX);
                if (cur >= 0 && cur < LockMin) AddRes(GOLD_IDX, LockMin - cur);
            }
        }
    }
}
