# 逆向研究清单

> 研究方法：PowerShell 二进制字符串提取（IL2CPP proxy DLL），无 dnSpy GUI。
> 目标文件：`E:\Games\Humanica\MelonLoader\Il2CppAssemblies\Assembly-CSharp.dll`（4.3 MB IL2CPP proxy）

## 资源系统

- [x] `AddResourceIntoFreeWarehouse(ResourceIndex, int, bool)` 方法存在 → refs/01-resource-research.md
- [x] `AddResource_Public_Void_ResourceIndex_Int32_0` 方法存在 → refs/01-resource-research.md
- [~] `AddResource` 所属类：二进制推断为 `VillageData`（间接证据，需 dnSpy 确认）→ refs/01-resource-research.md
- [~] `ResourceIndex` 类型：struct（有 `byref_ResourceIndex` 模式，但无枚举字段名）→ refs/01-resource-research.md
- [ ] `ResourceIndex` 具体值（wood/stone/food/gold 对应整数）：**需 dnSpy 确认** → refs/01-resource-research.md
- [x] `Warehouses_Public_get_List_1_Inventory_0` 存在（Warehouses 属性在某类中）→ refs/01-resource-research.md
- [x] `GodMode_Public_Static_get/set_Boolean_0` 存在，推断在 `InputController` 类 → refs/01-resource-research.md
- [x] `InstantResearchAll_Public_Void_0` 存在，推断在 `TechManager` 类 → refs/01-resource-research.md

## 村庄/人口系统

- [x] `SpawnCreature_Public_Creature_SCCoord_Boolean_CreatureIndex_Boolean_Boolean_String_Int32_Entity_Entity_Single_Single_Boolean_0` 存在 → refs/02-village-research.md
- [x] `AddVillager_Public_Void_Entity_0` 存在 → refs/02-village-research.md
- [x] `SpawnRandomVillager` 字段名存在（推断属于 `VillageData` 或 `CreatureManager`）→ refs/02-village-research.md
- [x] `SetProgressPerTimeStep_Public_Void_Single_0` 存在（建造/生产 patch 候选）→ refs/02-village-research.md
- [x] `GetSumProduceMultiplier_Public_Single_0` 存在（生产倍率 patch 候选）→ refs/02-village-research.md
- [ ] 建造速度 patch 所在类：**需 dnSpy 确认**（候选：`ProductionSlot` / `Construction`）→ refs/02-village-research.md
- [ ] 生产速度 patch 所在类：**需 dnSpy 确认**（候选：`ProductionSlot`）→ refs/02-village-research.md

## 时间系统

- [x] `TimeSystem_Public_Static_get_TimeSystem_0` 存在（单例已确认）→ refs/03-time-research.md
- [x] `SetTimeScale_Public_Void_TimeScaleIndex_0` 存在 → refs/03-time-research.md
- [x] `SetCurrentTimeScale_Public_Void_TimeScaleIndex_0` 存在 → refs/03-time-research.md
- [x] `TimeScaleIndex` 对应字段名 `SetTimeScale0Key`, `SetTimeScale1Key`, `SetTimeScale3Key`, `SetTimeScale6Key` 存在（枚举值 0/1/3/6 已确认）→ refs/03-time-research.md
- [~] `SetTimeScale_Public_Void_Single_0` 也存在（float 版本，推断也在 `TimeSystem`）→ refs/03-time-research.md

## 控制台 / 调试功能

- [x] `AddRecipeProgressesPerDayFromConsole_Public_Void_Int32_0` 存在（命名空间 `Il2CppHumanica.CommandConsole`）→ refs/01-resource-research.md
- [x] `SetDayFromConsole_Public_Void_Int32_0` 存在
- [x] `ConsoleController_Public_Static_get_ConsoleController_0` 单例存在

## 图例

- `[x]` 已确认（二进制直接证据）
- `[~]` 推断（间接证据，置信度中等）
- `[ ]` 需 dnSpy 确认
