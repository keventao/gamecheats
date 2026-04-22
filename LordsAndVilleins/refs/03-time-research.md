# 03 — Time 调研

> 调研于 2026-04-22 (ilspycmd 10.0.0.8330)

## 搜词清单

`Time`, `Clock`, `Calendar`, `Season`, `Day`, `Tick`, `Speed`,
`GameTime`, `TimeManager`, `WorldTime`, `Pause`

## 关键类型

- **时间管理类**:`GameManager`（全局命名空间，MonoBehaviour，`GameManager.instance`）
  - `Update()` 方法负责推进游戏时间和分发所有 tick 事件
  - 辅助静态工具类：`WorldTime`（静态类，无实例，提供时间单位转换）

- **MonoBehaviour 还是普通类?**:`GameManager` 是 MonoBehaviour（影响 patch Update 方法）

## 速度字段

- 速度档位字段名：`private int gameSpeedLvl`（访问性：private，通过 `GetGameSpeedLvl()` 读，`SetGameSpeedLvl(int)` 写）
- 实际乘数字段名：`private float gameSpeedMultiplierBySpeedLvl`（访问性：private）
- 类型：档位 int（0–5）+ 对应乘数 float
- 档位 → 乘数对应关系（在 `SetGameSpeedLvl()` 中）：
  - `0` → `0f`（暂停）
  - `1` → `1f`（正常速度）
  - `2` → `2f`（×2）
  - `3` → `4f`（×4）
  - `4` → `8f`（×8）
  - `5` → `32f`（×32，最大快进）
- 默认 1x 对应的值：`gameSpeedLvl=1`，`gameSpeedMultiplierBySpeedLvl=1f`
- 暂停对应的值：`gameSpeedLvl=0`，`gameSpeedMultiplierBySpeedLvl=0f`
- 游戏内最大快进档对应的值：`gameSpeedLvl=5`，`gameSpeedMultiplierBySpeedLvl=32f`

> **速度公式**（`GetGameTimeSecondsDelta()`）：
> `Time.deltaTime * (float)WorldTime.realTimeMultiplier * gameSpeedMultiplierBySpeedLvl / gameSpeedDividerBySetting`
> 其中 `WorldTime.realTimeMultiplier = 128u`（静态常量）
> `gameSpeedDividerBySetting`：游戏难度/设置中的额外减速因子（正常为 1.0f）

> **OverrideSpeed 策略**：直接设置 `gameSpeedMultiplierBySpeedLvl` 字段（需用 AccessTools），或调用 `SetGameSpeedLvl(int)`。
> 推荐方案：patch `GameManager.Update()` 前修改 `gameSpeedMultiplierBySpeedLvl`，或使用 `AccessTools.Field(typeof(GameManager), "gameSpeedMultiplierBySpeedLvl").SetValue(...)`

## 季节/日期字段

- 当前世界时间：`private uint worldTimeInSeconds`（GameManager，private，通过 `GetWorldTime()` 读）
- 季节：通过 `WorldTime.GetSeasonOfYearAsName(worldTimeInSeconds)` → `SeasonName` enum
  - `SeasonName`：`Spring=2`, `Summer=4`, `Autumn=8`, `Winter=16`（按位枚举）
- 日期（季内第几天）：`WorldTime.GetDayOfSeasonIndex(worldTimeInSeconds)` → `uint` (0-based)
- 年份：`WorldTime.GetWorldYearNumber(worldTimeInSeconds)` → `uint`（含基准年 bigBangYear=1300）
- 季长（天）：`WorldTime.daysInSeason = 15u`（静态可修改）

## 周期方法(Postfix 钩子)

- `GameManager.Update()` 存在：是（MonoBehaviour.Update，每帧调用）
- tick 事件列表（均为 `TickEvent`，委托 `void(uint, uint)`）：
  - `onSecondsTick`, `onMinuteTick`, `onHourTick`, `onDayTick`, `onSeasonTick`, `onYearTick`
  - `onEarlyDayTick`, `onLateDayTick`, `onMorningStartTick`, `onEveningStartTick`
- 推荐 patch 点：`GameManager.Update()`（Prefix/Postfix 均可），用 AccessTools 读写 `gameSpeedMultiplierBySpeedLvl`

## 反编译片段

```csharp
// GameManager.cs — 速度相关字段
private float gameSpeedDividerBySetting = 1f;
private float gameSpeedMultiplierBySpeedLvl = 1f;
private int lastGameSpeedLvl = 1;
private int gameSpeedLvl = 1;

// SetGameSpeedLvl — 档位与乘数映射
public void SetGameSpeedLvl(int speedLvl)
{
    gameSpeedLvl = speedLvl;
    switch (speedLvl)
    {
        case 0: gameSpeedMultiplierBySpeedLvl = 0f; break;   // 暂停
        case 1: gameSpeedMultiplierBySpeedLvl = 1f; break;   // 正常
        case 2: gameSpeedMultiplierBySpeedLvl = 2f; break;
        case 3: gameSpeedMultiplierBySpeedLvl = 4f; break;
        case 4: gameSpeedMultiplierBySpeedLvl = 8f; break;
        case 5: gameSpeedMultiplierBySpeedLvl = 32f; break;  // 最快
        default: gameSpeedMultiplierBySpeedLvl = gameSpeedLvl; break;
    }
    onGameSpeedChanged.Invoke(gameSpeedMultiplierBySpeedLvl);
}

// 时间推进公式
public float GetGameTimeSecondsDelta()
{
    return Time.deltaTime * (float)WorldTime.realTimeMultiplier * gameSpeedMultiplierBySpeedLvl / gameSpeedDividerBySetting;
}

// WorldTime.cs — 关键静态常量
public static uint realTimeMultiplier = 128u;
public static uint daysInSeason = 15u;
public static uint seasonsInYear = 4u;
```

## Module code 替换检查表

Phase 7 TimeCheats.cs 需要:
- `TIME_MGR_TYPE_FQN` ← `GameManager`（全局命名空间，MonoBehaviour）
- `SPEED_FIELD_NAME` ← `gameSpeedMultiplierBySpeedLvl`（private float，需 AccessTools）
  - 或调用 `GameManager.instance.SetGameSpeedLvl(int)` 设置档位
- 速度档位是 int（0–5），实际乘数是 float（0/1/2/4/8/32）
- 暂停：`SetGameSpeedLvl(0)` 或设 `gameSpeedMultiplierBySpeedLvl=0f`
- 正常：`SetGameSpeedLvl(1)`
- 最快：`SetGameSpeedLvl(5)`（32x）
- 如需绕过 enum 直接写乘数：`AccessTools.Field(typeof(GameManager), "gameSpeedMultiplierBySpeedLvl").SetValue(GameManager.instance, desiredMultiplier)`
