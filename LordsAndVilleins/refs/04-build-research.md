# 04 — Build 调研

> 调研于 2026-04-22 (ilspycmd 10.0.0.8330)

## 搜词清单

`Build`, `Blueprint`, `Construction`, `Building`, `Structure`,
`Recipe`, `Craft`, `Material`, `Cost`, `Consume`

## 关键类型

- **建造系统类**:`Blueprint`（全局命名空间，非 MonoBehaviour）
  - Blueprint 是放置在地图上的"待建"对象，持有自己的 `Inventory`（用于存放建材）
  - 建造完成回调：`Blueprint.BuildBlueprint()`（无参，在建造完成时被调用）
  - 建材存储：`Blueprint.inventory`（`private Inventory`，通过 `GetInventory()` 暴露）
  - AI 建造活动：`ACBuildBlueprint`（继承 `BuildBlueprint`，继承 `AIActivity`）

- **材料消耗方法**:`Blueprint.BuildBlueprint()` — 建造完成，触发 `buildableObjectAsset.ConstructObject()`
  - 完整签名：`public void BuildBlueprint()`
  - 返回类型：`void`
  - 参数：无
  - 说明：该方法在建造完成时被 `BuildBlueprint.ExecuteActivitySteps()` 的末尾调用（通过 `blueprint.FinishedBuildProgress()` → inventory 自动检查 → `BuildBlueprint()` 触发）

- **资源消耗流程**：
  1. NPC 从家族仓库取出材料，通过 `ResourceTicket` 机制预留
  2. 执行 `AIActivity.GetExecutionCost()` 获取建材列表
  3. 完成时 `inventory.SpendResources(ticket)` 从 Blueprint inventory 扣除材料
  4. `Blueprint.BuildBlueprint()` 最终被调用，执行实际建造
  - **资源"消耗"的最终钩点**：`Blueprint.BuildBlueprint()`（void，Prefix 返回 false 可跳过实际建造，但保留 Inventory 扣费）
  - **跳过材料检查的钩点**：`BuildBlueprint.HasResourcesForBlueprint(...)` → `bool`

- **HasResourcesForBlueprint 签名**（`BuildBlueprint` 抽象类中，protected 方法）：
  ```csharp
  protected bool HasResourcesForBlueprint(
      IInventoryView familyInventory,
      IInventoryView npcInventory,
      IInventoryView allNPCInventory,
      Dictionary<ResourceName, int> contractedAvailableResources,
      Dictionary<ResourceName, int> deductedResources,
      List<ResourceCostTupleSO> blueprintCost,
      Dictionary<ResourceName, int> taxedResourceDict)
  ```
  返回 `bool`。Prefix 将 `__result = true; return false;` 可跳过材料检查（NPC 永远认为"有足够材料"）。

## Prefix 短路细节

| 原方法返回 | Prefix 签名应该是 |
|---|---|
| `bool` | `static bool Prefix(ref bool __result) { __result = true; return false; }` |
| `void` | `static bool Prefix() { return false; }` |

**实际签名**（最有效的钩点）：
- **建材检查 Prefix**（推荐）：patch `BuildBlueprint.HasResourcesForBlueprint`
  ```csharp
  static bool Prefix(ref bool __result)
  {
      __result = true;
      return false;
  }
  ```
  效果：NPC 始终认为有足够材料，直接开始建造（但仍然需要物理递送材料到 blueprint）

- **建材放入 Prefix**（更激进）：patch `Blueprint.FinishedBuildProgress` 或 `BuildBlueprint()`
  - `Blueprint.BuildBlueprint()` 是 void，Prefix 可跳过整个建造流程（不常用）

- **更简单方案**：patch `Blueprint.OnInventoryChange()` 内的 `haveAllResources` 检查，或直接通过 `Blueprint.inventory.AddResource(...)` 注入材料

## 全建筑解锁(可选,本计划默认不做)

- `GameManager.unlockedStructures`（`HashSet<StructureName>`）：存放已解锁建筑类型
- `GameManager.unlockedWalls`（`HashSet<WallName>`）：已解锁墙壁
- `GameManager.unlockedFloors`（`HashSet<FloorName>`）：已解锁地板
- 解锁方式：直接 `GameManager.instance.unlockedStructures.Add(structureName)`（仅在 Campaign 模式有意义；Standard 模式默认全解锁）

## 反编译片段

```csharp
// BuildBlueprint.cs（抽象类）— 材料检查方法
protected bool HasResourcesForBlueprint(
    IInventoryView familyInventory,
    IInventoryView npcInventory,
    IInventoryView allNPCInventory,
    Dictionary<ResourceName, int> contractedAvailableResources,
    Dictionary<ResourceName, int> deductedResources,
    List<ResourceCostTupleSO> blueprintCost,
    Dictionary<ResourceName, int> taxedResourceDict)
{
    foreach (ResourceCostTupleSO item in blueprintCost)
    {
        ResourceName resourceName = item.first.resourceName;
        // ... 检查各 inventory 是否有足够资源 ...
        if (familyInventory.GetFreeResourceAmount(resourceName) + freeResourceAmount + value3 - value - num2 < item.second)
            return false;
    }
    return true;
}

// Blueprint.cs — 建造完成回调
public void BuildBlueprint()
{
    if (hasBeenBuilt)
    {
        Logger.LogAllError("Blueprint was tried to be built twice", logException: true);
        return;
    }
    hasBeenBuilt = true;
    Remove();
    buildableObjectAsset.ConstructObject(posInGrid, rotation, visualVariationID, fromGeneration: false);
}

// BuildBlueprint.ExecuteActivitySteps() — 建造流程（AI 侧）
protected override IEnumerator ExecuteActivitySteps()
{
    Blueprint blueprint = activityExecutionTarget as Blueprint;
    // ... 等待位置合法 ...
    blueprint.StartProgress();
    // ... 等待建造时间 ...
    if (!executionInterrupted)
    {
        executedSuccessfuly = true;
        blueprint.FinishedBuildProgress();  // 触发 inventory 检查 → BuildBlueprint()
    }
}

// Blueprint.OnInventoryChange() — inventory 变化时检查是否可建
private void OnInventoryChange()
{
    if (haveAllResources && finishedBuildProgress)
    {
        BuildBlueprint();  // 当材料齐全且进度完成时触发
        return;
    }
    haveAllResources = inventory.HasFreeAmountOfResources(GetResourceCost());
}
```

## Module code 替换检查表

Phase 8 BuildCheats.cs 需要:
- `MATERIAL_CONSUMPTION_TYPE` ← `BuildBlueprint`（抽象基类，全局命名空间）
- `MATERIAL_CONSUMPTION_METHOD` ← `HasResourcesForBlueprint`（protected bool，参数7个）
- `OnConsume_Prefix` 签名：
  ```csharp
  static bool Prefix(ref bool __result)
  {
      __result = true;
      return false;
  }
  ```
- 注意：`BuildBlueprint` 是抽象类，patch 应针对具体子类或使用 `HarmonyPatch(typeof(BuildBlueprint), "HasResourcesForBlueprint")` 并加 `[HarmonyTargetMethods]` 覆盖所有子类
- 备选：直接给 Blueprint.inventory 注入材料，绕过整个材料检查

## FastBuild(1-tick 建造)— 2026-05-22 ilspycmd 验证

源:`Assembly-CSharp.dll`(Lords and Villeins 1.6.15, CrossOver Steam bottle)。

### 已验证签名

| 符号 | 签名 / 行号 |
|---|---|
| `Blueprint.StartProgress` | `public void StartProgress()` — Blueprint:1105;副作用:`isInProgress = true`、视觉/摩擦更新,不触发计时。 |
| `Blueprint.FinishedBuildProgress` | `public void FinishedBuildProgress()` — Blueprint:1188;body 全文:`finishedBuildProgress = true;` |
| `Blueprint.haveAllResources` | `private bool` — Blueprint:34 |
| `Blueprint.finishedBuildProgress` | `private bool` — Blueprint:36 |
| `Blueprint.OnInventoryChange` | `private void OnInventoryChange()` — Blueprint:84;`OnInventoryContentChange` 监听器(Blueprint:269 添加,545 移除);开头分支 `if (haveAllResources && finishedBuildProgress) { BuildBlueprint(); return; }` 在 Blueprint:126。 |
| `AIActivity.CalculateTargetTime` | `public float CalculateTargetTime(float targetTimeBase)` — AIActivity:270 |
| `AIActivity.ExecuteForTime` | `public IEnumerator ExecuteForTime(float targetTimeInSeconds, bool storeTimeInTarget, SFXData executingSFXOverride = null, bool showBar = true)` — AIActivity:812 |

### `BuildBlueprint.ExecuteActivitySteps` 关键片段(BuildBlueprint.cs:340-365)

```csharp
waitTimePassed = 0f;
blueprint.StartProgress();
targetTime = CalculateTargetTime(asset.GetConstructTime());   // ← FastBuild patch 点
if (PlayerManager.instance.settlementDebuffModule.HasDebuff(SettlementDebuffType.MotivatedBuilders))
    targetTime *= 0.5f;
// ... 类型分支 ...
yield return ExecuteForTime(targetTime, storeTimeInTarget: true);
if (!executionInterrupted)
{
    executedSuccessfuly = true;
    blueprint.FinishedBuildProgress();
    PlaySoundByMaterial(...);
}
```

### Patch 策略

**目标**:`AIActivity.CalculateTargetTime` postfix,`__instance is BuildBlueprint` 限定。

理由:
1. `CalculateTargetTime` 是 `AIActivity` 基类公共方法,签名稳定。
2. `__instance is BuildBlueprint` 把作用域限制在建造活动内 —— 不影响伐木/烹饪/狩猎等其他 AI 工作的计时。
3. 把 `__result` 设 0f 时,`ExecuteForTime(0,...)` 立刻返回,后续 `FinishedBuildProgress` 即时执行;材料消耗仍由 NPC AI 流程在 `BuildBlueprint()` 实际触发前完成(`inventory.SpendResources` 走原路径触发 `OnInventoryChange` → `BuildBlueprint()`)。
4. 零反射写 private 字段,零 transpiler,零 coroutine 改写。

不选的方案:
- `Blueprint.StartProgress` postfix 直接调 `BuildBlueprint()`:跳过材料消耗,违反"材料仍正常扣"。
- `Blueprint.FinishedBuildProgress` 提前 + 触发 `OnInventoryChange`:需要反射读 `haveAllResources` 或显式重入,复杂且与 listener 模型相冲。
- transpiler 重写 `BuildBlueprint.ExecuteActivitySteps`(IEnumerator):IL 改 coroutine 风险高。
- `IBuildableObjectAsset.GetConstructTime` 全局归零:可能影响存档/序列化语义,作用面比 CalculateTargetTime 大。
