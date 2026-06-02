# Clanfolk 角色属性 / 移动速度 API（已在 Assembly-CSharp.dll 验证）

## 类层次
`Human : Unit : Entity`。`UnitManager.humanList`（Il2CppInterop 暴露为**属性**）含全部小人。

## 心情（mood）
- `Unit.GetMoodAttribute() -> AttributeMood`（Unit 上，ALL 反编译 line 278977 簇内）。
- `AttributeMood : AttributeGeneric`。
- 数值在基类 `AttributeGeneric`：
  - `SetAttributeProgress(float progress)` — 设进度 0~1（AttributeGeneric.cs:771）
  - `SetFloorAttributeProgress(float)` — 设下限（可做软锁）
  - `MaxCurrentValue()` / `SetCurrentValuePercent(float)` / 字段 `currentValue/minValue/maxValue`
- 锁满实现：每 tick `GetMoodAttribute().SetAttributeProgress(1f)`。
- 其它需求同理：`GetSleep/Food/Water/Social/Satisfaction/Growth/PregnancyAttribute()`。

## 移动速度
`Unit` 上的可读写字段（Il2CppInterop 暴露为属性）：
- `float unitSpeedMult` — 运行期速度倍率（Unit.cs:2216）。**3倍加速即写 3f。**
- `float moveSpeed` — 基础速度（Unit.cs:2242）；游戏可能按状态重算，不作首选。
- 相关只读计算：`GetMoveSpeed()`、虚 `GetUnitSpeedMult()`、`GetVariableSpeedMult()`、
  `GetTaskMoveSpeedMult()` 等（native，无 managed 逻辑可读）。
- 实现：开启时每 tick `unitSpeedMult = 3f`；关闭时跑一次 `= 1f` 复位后停手。

## 重要坑
Il2CppInterop 把 **native 字段暴露为 C# 属性**，不是字段。
`AccessTools.Field(type, "unitSpeedMult"/"humanList"/"myEntityAttributes")` 会返回 null，
必须用 `AccessTools.Property`。旧的生命锁用 Field 找 `myEntityAttributes` 即此问题。

## 代码落点
`src/ClanfolkCheats/Modules/CharacterCheats.cs`
- 已删除生命锁（游戏战斗少，无需）。
- `LockMood()` / `SetSpeed()`，反射成员均缓存。
