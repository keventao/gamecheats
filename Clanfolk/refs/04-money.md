# 金钱 API（已反编译验证）

来源：`ilspycmd -t Il2Cpp.MoneyManager / Il2Cpp.GameManager`（2026-06-02）。

## 访问

`GameManager.GetMoneyManager()`（**static**）→ `MoneyManager`。
（用 `GameRefs.GetManager("GetMoneyManager")`：AccessTools.Method + Invoke(gm,null)，静态方法忽略实例。）

## MoneyManager 方法（已验证签名）

| 方法 | 签名 | 用途 |
|---|---|---|
| `GetMoney()` | `int` | 当前定居点金钱 |
| `ChangeMoney(int change)` | `void` | 增/减 delta（+正 -负） |
| `SetMoney(int count, bool reset)` | `void` | 设绝对值；`reset=false` 即可 |

相关（未用）：`GetPersonalMoney`/`ChangePersonalMoney`、`GetTraderInitialMoney`、
字段 `currentMoney`/`money`/`totalMoney`/`personalMoney`/`traderMoney`。
交易/训练系统另有 `GetRequiredCoin`/`GetRewardCoin`/`GetTrainingCoin` 等（与定居点金钱不同）。

## 模块实现（MoneyCheats.cs）

- 绑定：`DrawGui` 首次 `GameRefs.GetManager("GetMoneyManager")`，缓存 + 三个 MethodInfo。
- 显示：`OnUpdate` 每帧 `GetMoney()` 刷新 `_lastShown`（避免 OnGUI 内反射抖动）。
- 按钮：+100 / +1000 / +10000 → `ChangeMoney`；清零 → `SetMoney(0,false)`；
  自定义金额（滚轮步进 1000）→ 增加(`ChangeMoney`) / 设为(`SetMoney`)。
