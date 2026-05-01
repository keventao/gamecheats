# 资源系统研究

> 研究方法：PowerShell 二进制字符串提取，目标 `Assembly-CSharp.dll`（IL2CPP proxy，4.3 MB）。
> 无 dnSpy GUI，标注说明：`(已确认)` = 二进制直接证据；`(推断)` = 间接证据；`(需确认)` = 需 dnSpy 补充。

---

## AddResource 方法

### 方法存在性
- `AddResource_Public_Void_ResourceIndex_Int32_0` — (已确认) 在 NativeMethodInfoPtr 表中直接找到
- `AddResourceIntoFreeWarehouse_Public_Void_ResourceIndex_Int32_Boolean_0` — (已确认) 同上
- `RemoveResource_Public_Void_ResourceIndex_Int32_0` — (已确认)
- `SpendResource_Public_Void_ResourceQuery_Boolean_0` — (已确认)

### 方法所在类
- 类名：**VillageData** — (推断)
  - 推断依据：`AddResource` 和 `AddResourceIntoFreeWarehouse` 在二进制字段名区段中与以下 VillageData 特有字段/方法紧邻：
    `SetPause`, `RaycastFromMouse`, `SetStaticMouse`, `SetObjectsFollowingMouse`, `SpawnCreature`, `DOPause`, `SetSmallInfoByMouse`
  - 这些方法名在 IL2CPP 字段名区段（非 NativeMethodInfoPtr 前缀的区段）中聚集出现，高度符合 `VillageData` 作为游戏世界根单例的特征
  - **需 dnSpy 确认**：在 dnSpy 中打开 `VillageData` 类，展开方法列表，确认 `AddResourceIntoFreeWarehouse(ResourceIndex, int, bool)` 归属于该类

### 单例访问
- `VillageData_Public_Static_get_VillageData_0` — (已确认) 标准 IL2CPP 单例 getter 模式
- 访问方式：`VillageData.VillageData` → 返回 `VillageData` 实例
- 推荐调用：`VillageData.VillageData.AddResourceIntoFreeWarehouse(idx, amount, false)` — (推断)

### 推荐调用方式（决策树）

```
1. 优先 AddResourceIntoFreeWarehouse(ResourceIndex, int, bool)
   理由：名称含 "FreeWarehouse"，说明会自动寻找空闲仓库槽，
   不需要调用方指定 Inventory；签名 bool 参数推断为 "createIfNeeded"，
   传 false 即向现有仓库添加，安全可预测。(推断，需 dnSpy 确认)

2. 如失败，试 AddResource(ResourceIndex, int, InventoryType)
   理由：需要调用方指定 InventoryType，灵活但需要额外枚举值；
   与 RemoveResource 签名对称，说明是正规资源操作接口。(已确认存在)

3. 兜底：AddResource(int) — 控制台命令版本
   理由：接受裸 int，可能绕过 ResourceIndex struct 构造问题；
   但语义不明确（单参数 int 可能是 ResourceType 而非 ResourceIndex）。
   仅在前两者均失败时使用。(已确认存在，所在类需确认)
```

### 备选方法
- `AddResource_Public_Void_ResourceIndex_Int32_0`：签名更简单，可能是 `Inventory` 或其他类的方法 — (需确认)
- `CreateResourceInSpecificPack_Public_Void_ResourceIndex_Int32_Boolean_0`：出现在 `AddResourceIntoFreeWarehouse` 旁边 — (已确认存在，用途需确认)
- `Warehouses_Public_get_List_1_Inventory_0`：Warehouses 属性返回 `List<Inventory>`，可以遍历仓库逐个 Add — (已确认存在，归属类需确认)

---

## ResourceIndex 类型

### 类型(2026-05-02 运行时确认)
- 类型:**enum**(int 底层),非 struct。原 v0.1.0 推断错误,实测 `typeof(Il2Cpp.ResourceIndex).IsEnum == true`
- 共 **143** 个成员(含 `___DEPRECATED` 后缀的废弃项)
- C# 直接 cast int → enum 即可:`var idx = (Il2Cpp.ResourceIndex)42;`

### 资源名称(完整列表 — 启动 dump)
启动时 `Plugin.OnInitializeMelon` 调 `ResourceCheats.DumpResourceIndex()` 把全部 143 项
枚举名 + int 值写到 `MelonLoader/Latest.log` 的 `[ResourceIndex.dump]` 节。

**已游戏内验证的关键 idx 映射(2026-05-02):**
- `STICKS = 1` — 树枝
- `COBBLESTONES = 2` — 鹅卵石
- `LOG = 3` — 原木
- `WILD_BERRIES = 4` — 野莓
- `APPLE = 5`
- `RAW_MEAT = 6`
- `RAW_PELT = 7`
- `BREAD = 32`
- `TECHNOLOGY_KNOWLEDGE = 105` — 科技知识(原 v0.1.0 当"金币"用是错的)

**完整 EN→中文翻译表:**`src/HumanicaCheats/Core/ResourceI18n.cs`,覆盖 ~100 项常用资源,
缺失项 fallback 到 enum 名。

### 重要参数:`AddResourceIntoFreeWarehouse` 第三参数
**v0.1.1 游戏内确认:必须传 `true`**(原 v0.1.0 推断 `false` 错误)。
- `false` = 只往现有仓库塞,容量不够静默丢弃。LOG 仓库大没事,
  COBBLESTONES / RAW_PELT 等容量小的会被截到 ~10。累积过多还会让游戏 AI 死循环卡死。
- `true` = 容量不够时自动开新仓库槽,正常工作。

### 相关类型
- `ResourceType`：(已确认存在) `GetResourceAmount_Public_Int32_ResourceType_0`，可能是枚举
- `ResourceTypeData`：(已确认存在) `GetResourceType_Public_Static_ResourceTypeData_ResourceType_0`
- `ResourceQuery`：(已确认存在) 多个方法以此为参数

### 构造方法
如果 `ResourceIndex` 是 int 包装 struct，在 Il2CppInterop proxy 中的用法通常为：
```csharp
// 方式 A：直接 cast（若 proxy 暴露 implicit 转换）
var idx = (ResourceIndex)3;

// 方式 B：构造（若有 _ctor(int) 方法）
var idx = new ResourceIndex();
idx.value__ = 3;
```
具体用法 **需 dnSpy 确认**。

### 运行时探测策略（Task 4 回退方案）

如果 dnSpy 在开发机不可用或无法确认具体 int 值，使用以下方案之一：

**方案 A：`GetResourceIndices` 反查（推荐）**

二进制已确认存在 `GetResourceIndices_Public_Static_Il2CppStructArray_1_ResourceIndex_ResourceType_0`，签名推断为：
```
ResourceIndex[] ResourceTypeData.GetResourceIndices(ResourceType type)
```
游戏内 `ResourceType` 是另一个枚举，其成员名（`Wood`/`Stone`/`Food` 等）可能有字符串表示。
调用流程：先拿到 `ResourceType` 枚举值 → 调用 `GetResourceIndices(type)` → 取第 0 个结果作为该类型的主 `ResourceIndex`。
- 置信度：高（方法已确认存在）；**需 dnSpy 确认 ResourceType 枚举成员名和所在类**

**方案 B：`GetResourceAmount` 暴力探测**

在 Mod 初始化时对 idx = 0..50 依次调用 `VillageData.VillageData.GetResourceAmount(new ResourceIndex { value__ = i })` 并记录非零值，对照游戏内实际仓库数量推断对应关系。
```csharp
// 伪代码（加入 MelonLogger 打印）
for (int i = 0; i <= 50; i++) {
    var idx = new ResourceIndex(); idx.value__ = i;
    var amt = VillageData.VillageData.GetResourceAmount(idx, InventoryType.Warehouse);
    if (amt > 0) MelonLogger.Msg($"ResourceIndex[{i}] = {amt}");
}
```
- 前提：`GetResourceAmount_Public_Int32_ResourceIndex_InventoryType_0` — (已确认存在)
- 置信度：高（只需游戏内运行一次即可映射）；**需确认 InventoryType 枚举值**

**方案 C：控制台命令 `AddResource(int)` 直接绕过**

二进制发现 `AddResource_Public_Void_Int32_0`（取纯 int 参数，与 `SetResource_Public_Void_Int32_0` 相邻），位置在 ConsoleController 相关命令区（pos 2476535）。该版本可能接受原始 int 索引，无需构造 `ResourceIndex` struct。
- 置信度：中（推断为控制台命令入口）；**需 dnSpy 确认所在类和参数语义**

---

## GodMode

### 所在类
- 类名：**InputController** — (推断)
- 推断依据：
  - `set_GodMode_Public_Static_set_Void_Boolean_0` 和 `set_IsInputFieldSelected_Public_Static_set_Void_Boolean_0`、`set_MapEditMode_Public_Static_set_Void_Boolean_0` 在 NativeMethodInfoPtr 表中紧邻出现
  - 同区段还有 `SetCheatHotKeysFlag_Public_Void_Boolean_0`、`set_DialogIsActive`、`set_PhotoModeIsActive` 等 UI/输入相关静态属性
  - `InputController_Public_Static_get_InputController_0` 单例存在 — (已确认)
  - 字段名区段中 `get_GodMode set_GodMode NativeFieldInfoPtr_godMode` 与 `EnterBuildMode SetBuildMode ExitBuildMode` 紧邻，符合 InputController 控制游戏模式的语义
- **需 dnSpy 确认**：打开 `InputController` 类，确认 `GodMode` 是其静态 bool 属性

### 访问方式
- 单例：`InputController.InputController` — (已确认单例模式)
- 开启：`InputController.GodMode = true` — (推断) 静态属性，无需实例

### 推断的效果
- 基于命名推断：GodMode 开启后可能禁用受伤/死亡、无限资源或其他无敌效果
- 实际效果 **需游戏内验证**

---

## TechManager

### 单例
- `TechManager_Public_Static_get_TechManager_0` — (已确认)
- 访问方式：`TechManager.TechManager`

### 解锁方法
- `InstantResearchAll_Public_Void_0` — (已确认存在)
  - 出现在 NativeMethodInfoPtr 方法表和字段名区段
  - 字段名区段：`OnClickResearch TryToResearch IsAvailableForResearch InstantResearch` 聚集，推断归属 `TechManager`
  - **需 dnSpy 确认**：打开 `TechManager` 类，确认 `InstantResearchAll()` 方法归属和签名

- `InstantResearch_Public_Void_0` — (已确认存在，推断单个科技解锁)

- `ApproveAll_Public_Void_0` — (已确认存在，可能用于审批所有年度计划，非科技解锁)

### 调用方式（推断）
```csharp
TechManager.TechManager.InstantResearchAll();
```

### 结论
游戏**有独立科技研究系统**，`InstantResearchAll` 方法存在。UnlockCheats 模块有实现基础。

---

## 控制台命令（可直接利用）

以下方法已通过控制台接口暴露，可以通过 `ConsoleController` 调用：
- `AddRecipeProgressesPerDayFromConsole_Public_Void_Int32_0` — (已确认)，在命名空间 `Il2CppHumanica.CommandConsole` 下
- `SetDayFromConsole_Public_Void_Int32_0` — (已确认)
- 访问：`ConsoleController_Public_Static_get_ConsoleController_0` — (已确认) 单例存在
