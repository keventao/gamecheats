# 村庄/人口系统研究

> 研究方法：PowerShell 二进制字符串提取，目标 `Assembly-CSharp.dll`（IL2CPP proxy，4.3 MB）。
> 标注：`(已确认)` = 二进制直接证据；`(推断)` = 间接证据；`(需确认)` = 需 dnSpy 补充。

---

## 新增村民

### 候选方法 1：AddVillager（推荐首选）
- 方法：`AddVillager_Public_Void_Entity_0` — (已确认存在)
- 归属类：**VillageData**（推断）
  - 推断依据：在 IL2CPP 字段名区段中，`AddVillager RemoveVillager excludingVillager AssignRandomVillager SpawnRandomVillager` 聚集出现，同区段包含 `NativeFieldInfoPtr_CreatureManager`、`NativeFieldInfoPtr_TechManager`、`NativeFieldInfoPtr_TurnManager` 等 VillageData 管理器字段引用
  - **需 dnSpy 确认** — 打开 `VillageData` 确认方法签名 `public void AddVillager(Entity entity)`
- 难点：需要一个有效的 `Entity` 参数（已存在的村民实体引用）
- 不适合"从零创建"村民，适合将游戏已有的 settler 转为 villager

### 候选方法 2：SpawnCreature（创建新村民）
- 方法：`SpawnCreature_Public_Creature_SCCoord_Boolean_CreatureIndex_Boolean_Boolean_String_Int32_Entity_Entity_Single_Single_Boolean_0` — (已确认存在)
- 归属类：**CreatureManager**（推断）
  - 推断依据：`SpawnCreature` 在字段名区段中出现，同区段有 `GoToCreature SelectCreature GetCreature SetCreature ScanNearestCreature AddUniqueIndexCreature`，高度符合 `CreatureManager` 语义
  - `CreatureManager_Public_Static_get_CreatureManager_0` — (已确认) 单例存在
- **单例访问**：`CreatureManager.CreatureManager` — (推断标准 IL2CPP 单例模式)
- 难点：参数极多（SCCoord, bool, CreatureIndex, bool, bool, string, int, Entity, Entity, float, float, bool），需要正确的位置坐标和 CreatureIndex 值 — **极难直接调用**

### 候选方法 3：SpawnRandomVillager（可能最简单）
- 方法签名：`SpawnRandomVillager_Public_Creature_0` — (已确认) 无参数，返回 `Creature`
- 归属类：**CreatureManager**（高置信度推断）

#### SpawnRandomVillager 宿主类分析

PowerShell 二进制分析，扫描全部出现位置（pos 2615525 和 3049097），对每处取 ±3000 字节窗口：

**第一处（pos 2615525）— NativeMethodInfoPtr 方法表区段**

`SpawnRandomVillager_Public_Creature_0` 在同一 NativeMethodInfoPtr 连续段中，与以下方法紧邻出现：
- `Save_Public_Void_CreatureManagerData_0` / `Load_Public_Void_CreatureManagerData_0`
- `SpawnCreature_Public_Creature_SCCoord_Boolean_CreatureIndex_..._0`
- `SpawnRandomAnimal_Public_Creature_Vector3_CreatureIndex_0`
- `get_Animals_Public_Static_get_List_1_Creature_0` / `get_Villagers_Public_Static_get_List_1_Creature_0`（均为静态）
- `add_ActiveCreatureChanged_Public_Static_...` / `remove_ActiveCreatureChanged_...`（静态事件）
- `SetActiveCreature_Public_Void_Creature_0`、`OpenCreatureInfo_Public_Void_Creature_0`、`CloseCreatureInfo_Public_Void_Creature_0`
- `get_CreatureManager_Public_Static_get_CreatureManager_0`（单例 getter）
- `GetIndexOfCreature_Public_Int32_Entity_0`、`ScanNearestCreature_...`

上述方法组合（Save/Load CreatureManagerData、SpawnCreature、SpawnRandomAnimal、静态 Villagers/Animals 列表、单例 getter）高度一致地指向 **CreatureManager**。

**第二处（pos 3049097）— 字段名区段**

`SpawnRandomVillager` 与 `AddVillager`、`RemoveVillager`、`AssignRandomVillager`、`TryAppointRandomVillager` 聚集，同区段有 `NativeFieldInfoPtr_CreatureManager`——即 VillageData 持有 CreatureManager 的引用字段，此处是 **VillageData 的字段引用**，不代表方法归属。

#### 候选类 Top-3

| 排名 | 候选类 | 置信度 | 依据 |
|------|--------|--------|------|
| 1 | **CreatureManager** | 高 | NativeMethodInfoPtr 区与 Save/Load CreatureManagerData、SpawnCreature、SpawnRandomAnimal、静态 Animals/Villagers、单例 getter 同表 |
| 2 | VillageData | 低 | 字段名区段相邻，但可能只是 VillageData 持有 CreatureManager 引用后间接调用 |
| 3 | 其他（如 SelectionManager） | 极低 | OpenCreatureInfo/CloseCreatureInfo 同表，但 Save/Load Manager 语义更强 |

**结论**：`[HarmonyPatch(typeof(CreatureManager), "SpawnRandomVillager")]` — (推断，需 dnSpy 确认)

### 候选方法 4：HomelessSettlers 机制
- `SelectHomelessSettlers_Private_List_1_Entity_IReadOnlyList_1_Entity_Int32_0` — (已确认存在)
- `PopulationStep_Private_Void_Entity_0` — (已确认存在)
- 方向：游戏通过 "homeless settler" 流程自然新增村民，可能通过操控此流程更容易

### 推荐实现策略
1. 优先尝试 `SpawnRandomVillager`（若参数少）— 需 dnSpy 确认签名
2. 备选 `SpawnCreature` + 传入 `VillageData.VillageData` 中心坐标 — 参数复杂
3. 兜底：调高 `VillagerSpawnChanceRatio` — (已确认) `GetVillagerSpawnChanceRatio_Public_Single_0` 存在

---

## 建造速度 patch 点

### 候选方法 1：SetProgressPerTimeStep（推荐）
- 方法：`SetProgressPerTimeStep_Public_Void_Single_0` — (已确认存在)
- 字段名区段证据：`get_ProgressPerTimeStep CalculateProgressPerTimeStep SetProgressPerTimeStep NativeFieldInfoPtr__progressPerTimeStep` 聚集，同区段有 `get_ExecutedRecipeStep`, `TryGetNextRecipeStep`, `get_NeedStartNextRecipeStep`
- 归属类：**推断 `ProductionSlot` 或 construction 组件** — (需确认)
  - **需 dnSpy 确认**：搜索 `SetProgressPerTimeStep`，确认所在类和方法签名
- Patch 策略：对 `CalculateProgressPerTimeStep()` 做 Postfix，将 `__result` 乘以倍率
- 注意：该方法也用于生产（非纯建造），patch 后可能同时加速生产

### 候选方法 2：SetConstructionProcess
- 方法：`SetConstructionProcess_Public_Void_Single_0` — (已确认存在)
- 字段名区段证据：`SetConstructionProcess constructionProcess ChangeTurnProcess` 聚集，同区段有 `SetInteractionProcess`
- 归属类：**需 dnSpy 确认**（候选：主游戏循环类或建筑实体类）
- Patch 策略：Postfix 放大 `__result`，或 Prefix 在调用前修改参数

### 候选方法 3：SetSpeedMultiplierParameter
- 方法：`SetSpeedMultiplierParameter_Public_Void_Single_0` — (已确认存在，出现在 SetProgressPerTimeStep 附近)
- 含义：推断为设置速度倍率参数，可能直接控制建造/生产速度
- **需 dnSpy 确认** 归属类和语义

---

## 生产速度 patch 点

### 候选方法 1：GetSumProduceMultiplier（推荐）
- 方法：`GetSumProduceMultiplier_Public_Single_0` — (已确认存在)
- 字段名区段证据：`get_ProduceMultiplier GetSumProduceMultiplier NativeFieldInfoPtr_produceMultiplier` 聚集，同区段有 `_speedMultiplier`, `_currentInventorySpeedMultiplier`, `_fullInventorySpeedMultiplier`
- 归属类：**推断生产/工坊相关组件** — (需确认)
- Patch 策略：Postfix 将返回值乘以 10，所有生产都加速

### 候选方法 2：GetProductionSpeedModifiers
- `GetProductionSpeedModifiers_Public_Static_Void_ProductionSlot_AttributeModifierContainer_byref_Single_byref_Single_0` — (已确认存在)
- 静态方法，可能是全局生产速度计算入口
- Patch 策略：Postfix 修改 byref 输出参数

### 推荐实现策略
- **建造速度**：Patch `SetProgressPerTimeStep`（需确认类名）
- **生产速度**：Patch `GetSumProduceMultiplier`（需确认类名）
- 两者可能在同一类上，待 dnSpy 确认

---

## 人口相关统计

- `Population_Public_get_Int32_0` — (已确认存在) 获取当前人口数量
- `AnimalPopulation_Public_get_Int32_0` — (已确认存在) 动物数量
- `GetVillagerSpawnChanceRatio_Public_Single_0` — (已确认存在) 村民自然增长概率倍率
- `VillagerSpawnChanceRatio_Public_Single_0` — (已确认存在，settet 版本)

---

## 总结

| 功能 | 方法 | 场景 | 确认状态 |
|------|------|------|---------|
| 新增村民（无参，自动生成） | `SpawnRandomVillager()` — 归属 CreatureManager（推断） | 创建全新 Creature 实体；无需已有 Entity | 签名已确认（无参返回 Creature），归属类待 dnSpy 确认 |
| 新增村民（需已有 Entity） | `AddVillager(Entity)` — 归属 VillageData（推断） | 将已存在的 settler Entity 注册为村民；不创建新实体 | 需 dnSpy 确认签名和归属类 |
| 新增村民（完整） | `SpawnCreature(SCCoord, bool, CreatureIndex, ...)` — CreatureManager | 创建指定类型/位置 Creature；参数极复杂 | 已确认存在，直接调用难度高 |
| 建造速度 | `SetProgressPerTimeStep(float)` | — | 需确认归属类 |
| 生产速度 | `GetSumProduceMultiplier()` | — | 需确认归属类 |
