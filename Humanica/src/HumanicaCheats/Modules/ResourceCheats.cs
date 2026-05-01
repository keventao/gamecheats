using System;
using System.Reflection;
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

        // ── ResourceIndex 值 ────────────────────────────────────────
        // TODO: 占位,需游戏内 trial-and-error 确认 (refs/01-resource-research.md: 需 dnSpy 确认)
        // 在游戏加载后用 +100 各按一次,对照仓库变化来判断 idx 是否正确。
        // 如果全部无效,查看 MelonLoader 控制台的 [ResourceCheats] 错误行调整。
        private static readonly int WOOD_IDX  = 0; // TODO: 占位,待确认
        private static readonly int STONE_IDX = 1; // TODO: 占位,待确认
        private static readonly int FOOD_IDX  = 2; // TODO: 占位,待确认
        private static readonly int GOLD_IDX  = 3; // TODO: 占位,待确认

        // ── 反射缓存 ──────────────────────────────────────────────
        // VillageData 是游戏根单例,通过 Il2Cpp.S.VillageData 访问 (已通过 Task 2 确认)
        // AddResourceIntoFreeWarehouse(ResourceIndex, int, bool) 归属 VillageData (refs/01-resource-research.md: 推断,需 dnSpy 确认)
        private static MethodInfo? _addMethod;
        private static Type?       _resourceIndexType;

        private static bool EnsureMethod()
        {
            if (_addMethod != null) return true;

            // 类名: VillageData (推断,refs/01-resource-research.md)
            var villageDataType = AccessTools.TypeByName("VillageData");
            if (villageDataType == null)
            {
                MelonLogger.Error("[ResourceCheats] 找不到 VillageData 类 — 确认 IL2CPP proxy 命名空间或查看 refs/01-resource-research.md");
                return false;
            }

            // ResourceIndex 类型 (refs/01-resource-research.md: struct,需 dnSpy 确认)
            _resourceIndexType = AccessTools.TypeByName("ResourceIndex");
            if (_resourceIndexType == null)
            {
                MelonLogger.Error("[ResourceCheats] 找不到 ResourceIndex 类型 — 需 dnSpy 确认命名空间");
                return false;
            }

            // 方法名: AddResourceIntoFreeWarehouse (已确认存在,refs/01-resource-research.md)
            _addMethod = AccessTools.Method(villageDataType, "AddResourceIntoFreeWarehouse");
            if (_addMethod == null)
            {
                MelonLogger.Error("[ResourceCheats] 找不到 AddResourceIntoFreeWarehouse — 确认方法名是否带 IL2CPP 后缀");
                return false;
            }

            MelonLogger.Msg("[ResourceCheats] 方法绑定成功");
            return true;
        }

        // 构造 ResourceIndex struct 并调用 AddResourceIntoFreeWarehouse
        // 实例方法通过 Il2Cpp.S.VillageData 获取 (Task 2 已验证此访问路径)
        private static void AddRes(int idxValue, int amount)
        {
            if (!EnsureMethod()) return;
            try
            {
                var villageData = Il2Cpp.S.VillageData;
                if (villageData == null)
                {
                    MelonLogger.Warning("[ResourceCheats] Il2Cpp.S.VillageData 为 null — 游戏世界未加载?");
                    return;
                }

                // 构造 ResourceIndex struct 实例,设置 value__ 字段
                // ResourceIndex 是 int 包装 struct (推断,refs/01-resource-research.md)
                var idx = Activator.CreateInstance(_resourceIndexType!);
                var valueField = _resourceIndexType!.GetField("value__");
                if (valueField == null)
                {
                    // 备选字段名 (IL2CPP proxy 可能用不同命名)
                    valueField = _resourceIndexType.GetField("Value")
                              ?? _resourceIndexType.GetField("_value")
                              ?? _resourceIndexType.GetField("m_value");
                }
                if (valueField != null)
                {
                    valueField.SetValue(idx, idxValue);
                }
                else
                {
                    MelonLogger.Warning($"[ResourceCheats] ResourceIndex 无 value__ 字段 — 尝试直接传 int");
                    // 兜底:若 ResourceIndex 实际上是枚举或 int,直接传 idxValue
                    _addMethod!.Invoke(villageData, new object[] { idxValue, amount, false });
                    return;
                }

                // AddResourceIntoFreeWarehouse(ResourceIndex idx, int amount, bool createIfNeeded)
                // bool 参数 false = 只向现有仓库添加 (推断,refs/01-resource-research.md)
                _addMethod!.Invoke(villageData, new object[] { idx, amount, false });
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[ResourceCheats] AddRes(idx={idxValue}, amount={amount}) 失败: {ex.Message}");
            }
        }

        public void Register(HarmonyLib.Harmony harmony)
        {
            // 方法发现延迟到首次使用时,避免初始化顺序问题
            Status = ModuleStatus.Ok;
        }

        public void DrawGui()
        {
            if (!GameRefs.IsReady) { GUILayout.Label("等待游戏加载…"); return; }

            // ── 用户警告 ────────────────────────────────────────────────
            // ResourceIndex 值为占位 (0/1/2/3),可能与实际资源不符
            // 若点击后无效,查看 MelonLoader 控制台的 [ResourceCheats] 错误行,
            // 并在 refs/01-resource-research.md 记录 dnSpy 确认的实际值。
            GUILayout.Label("[!] ResourceIndex 值为占位(0/1/2/3),需游戏内验证");
            GUILayout.Space(4);

            // ── 木材 ────────────────────────────────────────────────────
            GUILayout.Label("木材");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+100"))  AddRes(WOOD_IDX,  100);
            if (GUILayout.Button("+1000")) AddRes(WOOD_IDX, 1000);
            _lockWood = GUILayout.Toggle(_lockWood, "锁定 ≥500");
            GUILayout.EndHorizontal();

            // ── 石材 ────────────────────────────────────────────────────
            GUILayout.Label("石材");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+100"))  AddRes(STONE_IDX,  100);
            if (GUILayout.Button("+1000")) AddRes(STONE_IDX, 1000);
            _lockStone = GUILayout.Toggle(_lockStone, "锁定 ≥500");
            GUILayout.EndHorizontal();

            // ── 食物 ────────────────────────────────────────────────────
            GUILayout.Label("食物");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+100"))  AddRes(FOOD_IDX,  100);
            if (GUILayout.Button("+1000")) AddRes(FOOD_IDX, 1000);
            _lockFood = GUILayout.Toggle(_lockFood, "锁定 ≥500");
            GUILayout.EndHorizontal();

            // ── 金币 ────────────────────────────────────────────────────
            GUILayout.Label("金币");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+100"))  AddRes(GOLD_IDX,  100);
            if (GUILayout.Button("+1000")) AddRes(GOLD_IDX, 1000);
            _lockGold = GUILayout.Toggle(_lockGold, "锁定 ≥500");
            GUILayout.EndHorizontal();
        }

        // 每帧锁定 — 由 Plugin.OnUpdate 驱动
        public void OnUpdate()
        {
            if (!GameRefs.IsReady) return;
            if (_lockWood)  AddRes(WOOD_IDX,  LockMin);
            if (_lockStone) AddRes(STONE_IDX, LockMin);
            if (_lockFood)  AddRes(FOOD_IDX,  LockMin);
            if (_lockGold)  AddRes(GOLD_IDX,  LockMin);
        }
    }
}
