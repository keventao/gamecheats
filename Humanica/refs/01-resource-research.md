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

### 备选方法
- `AddResource_Public_Void_ResourceIndex_Int32_0`：签名更简单，可能是 `Inventory` 或其他类的方法 — (需确认)
- `CreateResourceInSpecificPack_Public_Void_ResourceIndex_Int32_Boolean_0`：出现在 `AddResourceIntoFreeWarehouse` 旁边 — (已确认存在，用途需确认)
- `Warehouses_Public_get_List_1_Inventory_0`：Warehouses 属性返回 `List<Inventory>`，可以遍历仓库逐个 Add — (已确认存在，归属类需确认)

---

## ResourceIndex 类型

### 类型推断
- 类型：**struct**（int 包装类型）— (推断)
- 推断依据：
  - 二进制中找到 `byref_ResourceIndex` 模式（4 次），说明该类型可以 by-ref 传递，符合 value type
  - 没有找到 `ResourceIndex.Wood`、`ResourceIndex.Stone` 等枚举字段名（排除枚举可能）
  - IL2CPP 将值类型 struct 命名为 `ResourceIndex`，方法签名中大量使用 `ResourceIndex` 作为参数类型

### 资源名称（具体值）
- 木材值：**需 dnSpy 确认** — dnSpy 中搜索 `ResourceIndex` 类型，查看其 `value__` 字段或预定义 static 成员
- 石材值：**需 dnSpy 确认**
- 食物值：**需 dnSpy 确认**
- 金币值：**需 dnSpy 确认**
- 完整资源列表：**需 dnSpy 确认** — 建议在 dnSpy 中搜索 `ResourceTypeData` 类，其中可能有资源名称数据

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
