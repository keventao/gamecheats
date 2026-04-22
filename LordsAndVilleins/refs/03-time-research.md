# 03 — Time 调研

> 调研于 2026-04-XX

## 搜词清单

`Time`, `Clock`, `Calendar`, `Season`, `Day`, `Tick`, `Speed`,
`GameTime`, `TimeManager`, `WorldTime`, `Pause`

## 关键类型

- **时间管理类**:`<FILL_TIME_MGR_TYPE_FQN>`
- **MonoBehaviour 还是普通类?**:`<>`(影响 patch Update 还是别的方法)

## 速度字段

- 字段名:`<FILL_SPEED_FIELD_NAME>`
- 类型:`<float 乘数 / int 档位 enum>`
- 默认 1x 对应的值:`<FILL>`
- 暂停对应的值:`<FILL>`
- 游戏内最大快进档对应的值:`<FILL>`

> 如果是 enum 档位(如 `enum Speed { Pause, Normal, Fast, VeryFast }`),
> 我们的 OverrideSpeed 需要绕过 enum 直接写底层 multiplier。这种情况下还要找:
> 字段名 `<FILL_SPEED_MULTIPLIER_FIELD>`(实际相乘因子)

## 季节/日期字段(可选)

- `<FILL_SEASON_FIELD>` : `<enum / int>`
- `<FILL_DAY_FIELD>` : `<int>`
- `<FILL_YEAR_FIELD>` : `<int>`

## 周期方法(Postfix 钩子)

- `<FILL_TIME_MGR_TYPE_FQN>.Update()` 是否存在?

## 反编译片段

```csharp
// 时间管理类的字段定义
```

## Module code 替换检查表

Phase 7 TimeCheats.cs 需要:
- `TIME_MGR_TYPE_FQN` ← `<FILL_TIME_MGR_TYPE_FQN>`
- `SPEED_FIELD_NAME` ← `<FILL_SPEED_FIELD_NAME>`(如果是 multiplier);否则用 `SPEED_MULTIPLIER_FIELD`
- 如果速度字段是 int enum 档位,把 module 里 `ConfigEntry<float>` 改成 int + 调整 GUI
