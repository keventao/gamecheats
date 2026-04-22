# 02 — Pawn 调研

> 调研于 2026-04-XX

## 搜词清单

`pawn`, `villager`, `villein`, `lord`, `peasant`, `character`, `npc`,
`inhabitant`, `resident`, `worker`, `population`, `colonist`

## 关键类型

- **村民/领主类**:`<FILL_PAWN_TYPE_FQN>`
- **村民集合**(从 Settlement 拿)路径:`<FILL_PAWNS_COLLECTION_PATH>`
  - 数据结构:`<List<Pawn> / Pawn[] / Dictionary<Id, Pawn>>`
  - 字段名:`<FILL>`
  - 例:如果 `Settlement.population.pawns`,则 `PAWNS_COLLECTION_PATH = "population/pawns"` 或拆成两步访问

## 状态字段

| 字段 | 实际名 | 类型 | "好的"方向 |
|---|---|---|---|
| 饥饿 | `<FILL_HUNGER_FIELD_NAME>` | `<float 0–1 / int 0–100>` | 大 / 小 |
| 健康 | `<FILL_HEALTH_FIELD_NAME>` | `<>` | 大 / 小 |
| 心情 | `<FILL_MOOD_FIELD_NAME>` | `<>` | 大 / 小 |

> 如果"健康"是大值代表健康(常见),module 用 100 设值;小值代表健康(不常见),module 用 0。
> 同样:饥饿通常小值=不饿,所以 module 用 0。

## 技能字段

- 字段路径:`<FILL_SKILLS_PATH>`
- 数据结构:`<Dictionary<SkillType, int> / int[] / 自定义>`
- 满值:`<FILL_SKILL_MAX_VALUE>`(例 100 或 20)
- 是否需要 cap?(有的游戏 skills 超过上限会出问题)

## 反编译片段

```csharp
// 把 Pawn 类的字段定义贴这里
```

## Module code 替换检查表

Phase 6 PawnCheats.cs 需要:
- `PAWNS_COLLECTION_PATH` ← `<FILL_PAWNS_COLLECTION_PATH>`
- `HUNGER_FIELD_NAME` ← `<FILL_HUNGER_FIELD_NAME>`
- `HEALTH_FIELD_NAME` ← `<FILL_HEALTH_FIELD_NAME>`
- `MOOD_FIELD_NAME` ← `<FILL_MOOD_FIELD_NAME>`
- `SKILLS_PATH` ← `<FILL_SKILLS_PATH>`
- 常量 `HEALTH_MAX` / `MOOD_MAX` / `HUNGER_MIN` 按上方"好的方向"调整
