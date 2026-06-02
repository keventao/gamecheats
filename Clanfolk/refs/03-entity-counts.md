# 物品库存计数 API（已反编译验证）

来源：`ilspycmd -t Il2Cpp.EntityManager Assembly-CSharp.dll`（2026-06-02）。

`EntityManager : ControlsSystem`，按 `EntityClass` 分管理器
（`GameManager.GetEntityManager(EntityClass.Item)` 取物品管理器）。

## 计数方法（用这些，别枚举列表）

| 方法 | 签名 | 用途 |
|---|---|---|
| `GetEntityCount()` | `int` | 该管理器内全部实体数（物品管理器 → 物品总件数，UI 表头“件物品”） |
| `GetEntityTypeCount(string entityType, GrowthStage flags = NONE, bool isSpoiled = false)` | `int` | 单一类型计数（槽位 `xN`） |
| `GetEntityTypeList(string, GrowthStage = NONE)` | `List<Entity>` | 单类型实体列表（暂未用） |
| `GetAllEntityList()` | `List<Entity>` | 全部实体列表 |

- `GetEntityType()`（Entity）→ string key；字段 `myEntityType`。
- `GrowthStage.NONE == 0`（枚举默认）；反射取 `param[1].ParameterType` + `Enum.ToObject(t,0)`。
- `isSpoiled=false` 只数未腐坏；腐坏食物需另算 `true`（当前未计入，可后续补）。

## 坑：Il2Cpp 列表 ≠ System.Collections.IList

`GetAllEntityList()` 返回 `Il2CppSystem.Collections.Generic.List<Entity>`，
**不是** `System.Collections.IList`。
旧 ResourceCheats 把它 `as System.Collections.IList` → 永远 null → 计数恒为 0，
且静默失败（无 `[Rsrc] EntityList:` 日志）。这是 `xN`=0 的真因。

修复：弃用列表枚举，改用 `GetEntityCount` / `GetEntityTypeCount`
（返回 `int` 装箱，反射安全，无需处理 Il2Cpp 集合类型）。
仅查询 5 个显示槽位 + 总数，可每 ~30 帧刷新，生成后即时更新。
