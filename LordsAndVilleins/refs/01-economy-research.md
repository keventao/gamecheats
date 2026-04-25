# 01 — Economy 调研

> 调研于 2026-04-22 (ilspycmd 10.0.0.8330)
> 游戏版本:`1.6.15`
> Assembly-CSharp.dll SHA256:`0051905181f064cb0487909da5e4898a7988e97c6064ec5402691a232274eebf`

## 搜词清单(在 dnSpy 里 Ctrl+Shift+K)

`gold`, `money`, `coin`, `treasury`, `economy`, `resource`, `food`, `wood`, `stone`,
`Settlement`, `Manor`, `Estate`, `Village`, `Stockpile`, `Storage`

## 关键类型

- **聚合体类**(持有金币/食物等的顶层对象):
  - 全限定类名:`EconomyManager`（全局命名空间，MonoBehaviour，静态单例 `EconomyManager.instance`）
  - 这是整个 mod 的"游戏世界根"起点，但它是工厂/策略容器，不直接持有资源数量
  - 注意：游戏没有名为 Settlement 的聚合类。资源存储在各家庭（`ActiveOrganization`）的 `Inventory` 里，货币也是资源（`ResourceName.GoldCoins`/`SilverCoins`/`CopperCoins`/`Money`）
  - 玩家仓库总量：`PlayerManager.instance.playerInventory.GetInventory()`（`Inventory` 对象）
  - 时间与 tick 管理中枢：`GameManager`（`GameManager.instance`，MonoBehaviour）

- **资源字段**（资源不是直接字段，而是 Inventory 系统）：
  - 金币：N/A — 无独立字段。通过 `ResourceName.GoldCoins` 在 `Inventory` 中存取
    - 读取：`inventory.GetExistingResourceAmount(ResourceName.GoldCoins)` → `int`
    - 写入：`inventory.AddResource(ResourceName.GoldCoins, amount)`（返回 bool，受容量限制）
  - 食物：N/A — 无独立字段。使用 `ResourceName.Bread`, `ResourceName.Grain`, `ResourceName.SimpleMeal` 等（`ResourceType.Food`）
  - 木材：N/A — 无独立字段。使用 `ResourceName.Wood`（`ResourceType.Material`）
  - 石材：N/A — 无独立字段。使用 `ResourceName.Stone`（`ResourceType.Material`）

> **2026-04-25 实测修正**:
> - 玩家实际钱包用的是 `ResourceName.Money`(显示 120K+),不是 `GoldCoins`(显示 0)。`GoldCoins/SilverCoins/CopperCoins` 是货币系统的"硬币"枚举,但玩家通用钱包字段是 `Money`。EconomyCheats 已改用 `ResourceName.Money`。
> - Food (Grain) 第一个 inventory 拒(`allowedResources` 不含),走到第二个 inventory 接受 — `WalkInventoryViews` 递归是必要的。
> - **Wood / Stone 在玩家个人 inventory 全 reject**(实测 40+ 个 inv,`Inventory.AddResource` 第一行 `if (!allowedResources.Contains(name)) return false`)。Wood/Stone 必须存在 stockpile / lumberyard 类 storage building 的 inventory 里;`PlayerManager.playerInventory.GetInventory()` 走的是 person-carry inventory,不包含 stockpiles。
> - 要支持 Wood/Stone:走 `FindObjectsOfType<Stockpile>()` 找世界级仓库,或 reflect 进 `existingResourceContent[Wood].Gain()` 绕过 allowedResources check。v0.1 已撤,见 ROADMAP "已知限制"。

  > **实际数据结构**：
  > - `Inventory` 类（`Inventory.cs`）内部维护 `private Dictionary<ResourceName, Resource> existingResourceContent`
  > - 主要 API：`GetExistingResourceAmount(ResourceName)` → `int`，`AddResource(ResourceName, int)` → `bool`
  > - 玩家仓库 Inventory：`PlayerManager.instance.playerInventory.GetInventory()`
  > - `ResourceName` 枚举（全局 enum）：`GoldCoins=113`, `SilverCoins=114`, `CopperCoins=115`, `Money=66`, `Bread=28`, `Grain=38`, `Wood=95`, `Stone=82`

## 周期性方法(Postfix 钩子)

- 全限定签名：`GameManager.Update()` — 每 Unity 帧调用，内部在秒/分/时/日/季/年边界分发 tick 事件
  - `onDayTick`（`TickEvent`）：每游戏日触发 — 最适合经济 hook
  - `onHourTick`：每游戏小时触发
  - `onSeasonTick`：每季节触发
- 调用频率：`Update()` 每帧；`onDayTick` 每游戏日（realTimeMultiplier=128 缩放）
- 适合 Postfix：是 — patch `GameManager.Update()` 或订阅 `GameManager.instance.onDayTick.AddListener(...)`
- 游戏速度公式：`Time.deltaTime * 128f * gameSpeedMultiplierBySpeedLvl / gameSpeedDividerBySetting`

## 已排除的备选

- `EconomyManager.Awake/Start`：空 body，无 tick 逻辑
- `EconomyManager` 无名为 UpdateEconomy/Tick/OnDayEnd 的方法
- 无名为 Settlement/Manor/Estate/Village/Colony 的顶层聚合类
- 资源不在任何 Manager 字段中，而是分布于各 `ActiveOrganization.inanimateInventoryCollection`

## 反编译片段(关键代码,直接粘贴)

```csharp
// GameManager.Update() — 时间推进与 tick 分发（GameManager.cs）
public void Update()
{
    if (LoadingManager.instance == null || LoadingManager.instance.IsLoading()) return;
    secondsDelta += GetGameTimeSecondsDelta();
    // ... 推进 worldTimeInSeconds ...
    if (worldTimeInDays2 > worldTimeInDays)
    {
        onDayTick.Invoke(WorldTime.GetDayOfSeasonIndex(worldTimeInSeconds), worldTimeInDays2 - worldTimeInDays);
    }
}

// GameManager.GetGameTimeSecondsDelta()
public float GetGameTimeSecondsDelta()
{
    return Time.deltaTime * (float)WorldTime.realTimeMultiplier * gameSpeedMultiplierBySpeedLvl / gameSpeedDividerBySetting;
}

// Inventory.GetExistingResourceAmount(ResourceName resource)
public int GetExistingResourceAmount(ResourceName resource)
{
    if (!existingResourceContent.ContainsKey(resource)) return 0;
    return existingResourceContent[resource].GetAmount();
}

// Inventory.AddResource(ResourceName name, int amount)  [受 maxCapacity 限制]
public bool AddResource(ResourceName name, int amount)
{
    if (!allowedResources.Contains(name)) return false;
    if (totalWeight + EconomyManager.instance.GetResourceDefinition(name).weightPerUnit * amount > inventoryDefinition.maxCapacity) return false;
    existingResourceContent[name].Gain(amount);
    // ...
    return true;
}

// EconomyManager — 货币资源名列表
public List<ResourceName> GetCurrencyResourceNames()
{
    return new List<ResourceName> { ResourceName.GoldCoins, ResourceName.SilverCoins, ResourceName.CopperCoins };
}
```

## Module code 替换检查表

填完上面之后,Phase 5 EconomyCheats.cs 里需要把这些占位替换:

- `SettlementTypeFqn` ← `EconomyManager`（单例入口），实际资源操作目标是 `PlayerManager.instance.playerInventory.GetInventory()`
- `EconTickMethod` ← `GameManager.Update`（或订阅 `GameManager.instance.onDayTick`）
- `GOLD_FIELD_NAME` ← N/A — 使用 `ResourceName.GoldCoins` 通过 `Inventory.AddResource()` API
- `FOOD_FIELD_NAME` ← N/A — 使用 `ResourceName.Grain`（或 `ResourceName.Bread`）
- `WOOD_FIELD_NAME` ← N/A — 使用 `ResourceName.Wood`
- `STONE_FIELD_NAME` ← N/A — 使用 `ResourceName.Stone`
- 实际添加金币：`PlayerManager.instance.playerInventory.GetInventory().AddResource(ResourceName.GoldCoins, amount)`
