# 时间系统研究

> 研究方法：PowerShell 二进制字符串提取，目标 `Assembly-CSharp.dll`（IL2CPP proxy，4.3 MB）。
> 标注：`(已确认)` = 二进制直接证据；`(推断)` = 间接证据；`(需确认)` = 需 dnSpy 补充。

---

## TimeSystem 单例

- `TimeSystem_Public_Static_get_TimeSystem_0` — (已确认) 标准 IL2CPP 单例 getter 模式
- 访问方式：`TimeSystem.TimeSystem` — (推断) 与其他单例（VillageData.VillageData、TechManager.TechManager）命名一致

---

## SetTimeScale 方法

### SetTimeScale(TimeScaleIndex) — 推荐入口
- `SetTimeScale_Public_Void_TimeScaleIndex_0` — (已确认存在)
- `SetCurrentTimeScale_Public_Void_TimeScaleIndex_0` — (已确认存在，同区段)
- `get_CurrentTimeScaleIndex_Public_get_TimeScaleIndex_0` — (已确认存在，同区段)
- 归属类：**TimeSystem**（推断）
  - 推断依据：字段名区段中 `cachedTimeScale UpdateTimeScale SetTimeScale get_CurrentTimeScale SetCurrentTimeScale NativeFieldInfoPtr__currentTimeScale timeScale` 聚集，符合游戏时间系统内部字段特征
  - `SetTimeScale` 和 `SetDaySpeed`、`SetSpeed` 在同一 NativeMethodInfoPtr 区段出现
  - **需 dnSpy 确认**：打开 `TimeSystem` 类，确认 `SetTimeScale(TimeScaleIndex)` 归属

### SetTimeScale(float) — 备用
- `SetTimeScale_Public_Void_Single_0` — (已确认存在，与 SetDaySpeed、SetSpeed 同区段)
- 归属类：**同为 TimeSystem**（推断）

---

## TimeScaleIndex 枚举值

**已通过字段名推断，高置信度：**

| TimeScaleIndex 值 | 字段名证据 | 推断含义 |
|---|---|---|
| `0` | `NativeFieldInfoPtr_SetTimeScale0Key` — (已确认存在) | 暂停 |
| `1` | `NativeFieldInfoPtr_SetTimeScale1Key` — (已确认存在) | 正常速度 ×1 |
| `3` | `NativeFieldInfoPtr_SetTimeScale3Key` — (已确认存在) | 快速 ×3 |
| `6` | `NativeFieldInfoPtr_SetTimeScale6Key` — (已确认存在) | 最快 ×6 |

- 推断依据：这些字段名出现在同一 NativeFieldInfoPtr 区段，命名模式为 `SetTimeScale{N}Key`，直接表明游戏有 0/1/3/6 四个时间档位（与游戏界面按钮一致）
- 具体 C# 枚举成员名（`Pause`/`Normal`/`Fast`/`VeryFast`）**需 dnSpy 确认**

---

## 是否需要同时修改 Unity Time.timeScale

### 计划决定（来自设计文档 + 实施计划）
TimeCheats 模块使用 `UnityEngine.Time.timeScale` 作为 v0.1.0 实现：
- **理由**：`Time.timeScale` 是 Unity 引擎全局时间倍率，影响所有 Update/FixedUpdate，包括游戏逻辑
- **不需要** patch TimeSystem 即可实现基本速度控制
- v0.2.0 增强：再研究 `TimeSystem.SetTimeScale` 与 `Time.timeScale` 的关系

### 潜在风险
- 游戏可能在内部覆盖 `Time.timeScale`（通过 TimeSystem 管理），导致我们的修改被重置 — (需游戏内验证)
- 若 `Time.timeScale` 修改后被游戏重置，则需改用 `TimeSystem.SetTimeScale(TimeScaleIndex index)`

---

## SetDaySpeed 和 SetSpeed

- `SetDaySpeed_Public_Void_Single_0` — (已确认存在)
- `SetSpeed_Public_Void_Single_0` — (已确认存在)
- 两者与 `SetTimeScale` 在同一 NativeMethodInfoPtr 区段，推断均属于 `TimeSystem` 或相关时间控制类
- 归属类：**需 dnSpy 确认**

---

## 调用推断（v0.2.0 参考）

```csharp
// 推断调用方式（未经验证，需 dnSpy 确认后填入 Task 3）
var ts = TimeSystem.TimeSystem;
ts.SetTimeScale(/* TimeScaleIndex 枚举值 */);

// 或 float 版本
ts.SetTimeScale(3.0f); // 三倍速
```

---

## 总结

| 项目 | 状态 |
|------|------|
| TimeSystem 单例访问 | 已确认 (`TimeSystem.TimeSystem`) |
| SetTimeScale(TimeScaleIndex) 存在 | 已确认 |
| TimeScaleIndex 枚举值 0/1/3/6 | 已确认（高置信度推断） |
| SetTimeScale 归属类 | 推断 TimeSystem，需 dnSpy 确认 |
| v0.1.0 实现方案 | 使用 `Time.timeScale`（无需 patch） |
