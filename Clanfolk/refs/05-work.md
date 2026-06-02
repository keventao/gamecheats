# 瞬间工作 API（已反编译验证）

来源：`ilspycmd -t Il2Cpp.Unit / Il2Cpp.Node`（2026-06-02）。

## 工作进度核心方法（吃 deltaTime，进度 ∝ deltaTime）

**真正入口**（每帧按 `deltaTime` 推进进度）：

| 类型.方法 | 签名 | 工作类型 |
|---|---|---|
| `Node.ApplyNodeHarvestWork` | `bool (WorldObject, TilemapType, Unit, float deltaTime)` | 采集/砍伐 |
| `Node.ApplyNodeExtractionWork` | `bool (WorldObject, TilemapType, Unit, float deltaTime)` | 开采/挖掘 |
| `Node.ClearNodeForHarvest` | `bool (WorldObject, Unit, float deltaTime, TilemapType)` | 清理 |
| `WorldObject.ApplyObjectHarvestWork` | `bool (Unit, float deltaTime)` | 对象采集 |
| `WorldObject.ApplyHarvestWork` | `bool (Unit, float deltaTime)` | 采集 |
| `WorldObject.ApplyStateWork` | `override bool (Unit, float deltaTime)` | 建造/状态工作 |

**关键**：所有重载的时间参数都叫 `deltaTime` → 一个 Harmony **prefix**
`Prefix_ScaleDeltaTime(ref float deltaTime)` 按**参数名**注入，通吃全部，`deltaTime *= 100`。

## 走过的弯路（无效）
- `Unit.GetAppliedWorkTime`（virtual float, 2 重载）postfix ×100 → **无效**
  （该值不是进度累加点）。已弃。

## 模块实现（WorkCheats.cs，第 8 模块）
- 按类型+方法名反射取上述方法（仅含 `deltaTime` 参数的重载），逐个 Harmony prefix。
- 开关 `瞬间工作` → `_sWorkMult=100`（关=1），`OnUpdate` 同步；prefix `deltaTime *= mult`。

注：制造配方时间另由 BuildCheats patch `Recipe.ChangeElapsedTime`。
