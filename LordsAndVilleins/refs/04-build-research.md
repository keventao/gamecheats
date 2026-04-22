# 04 — Build 调研

> 调研于 2026-04-XX

## 搜词清单

`Build`, `Blueprint`, `Construction`, `Building`, `Structure`,
`Recipe`, `Craft`, `Material`, `Cost`, `Consume`

## 关键类型

- **建造系统类**:`<FILL_BUILD_TYPE_FQN>`
- **材料消耗方法**:`<FILL_MATERIAL_CONSUMPTION_METHOD>`
  - 完整签名:`<例:bool TryConsumeMaterials(BuildingDef def)>`
  - 返回类型:`<bool / void>`(决定 Prefix 是否需要 ref __result)
  - 参数:`<>`

## Prefix 短路细节

| 原方法返回 | Prefix 签名应该是 |
|---|---|
| `bool` | `static bool Prefix(ref bool __result) { __result = true; return false; }` |
| `void` | `static bool Prefix() { return false; }` |
| `bool TryX(out int actualUsed)` | `static bool Prefix(out int actualUsed, ref bool __result) { actualUsed = 0; __result = true; return false; }` |

**实际签名**(填):`<FILL_PREFIX_SIGNATURE>`

## 全建筑解锁(可选,本计划默认不做)

- `<FILL_UNLOCK_DATA_TYPE>` 是否存在?
- `<FILL_UNLOCK_LIST_FIELD>`?

## 反编译片段

```csharp
// 材料消耗方法的反编译
```

## Module code 替换检查表

Phase 8 BuildCheats.cs 需要:
- `MATERIAL_CONSUMPTION_TYPE` ← `<FILL_BUILD_TYPE_FQN>`
- `MATERIAL_CONSUMPTION_METHOD` ← `<FILL_MATERIAL_CONSUMPTION_METHOD>`
- `OnConsume_Prefix` 签名按上方"实际签名"调整
