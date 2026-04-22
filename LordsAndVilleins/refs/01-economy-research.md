# 01 — Economy 调研

> 调研于 2026-04-XX
> 游戏版本:`<FILL_GAME_VERSION>`
> Assembly-CSharp.dll SHA256:`<FILL_SHA256>`(用 `Get-FileHash` 算)

## 搜词清单(在 dnSpy 里 Ctrl+Shift+K)

`gold`, `money`, `coin`, `treasury`, `economy`, `resource`, `food`, `wood`, `stone`,
`Settlement`, `Manor`, `Estate`, `Village`, `Stockpile`, `Storage`

## 关键类型

- **聚合体类**(持有金币/食物等的顶层对象):
  - 全限定类名:`<FILL_SETTLEMENT_TYPE_FQN>`(例:`HonestlyGames.LAV.Settlement`)
  - 这是整个 mod 的"游戏世界根",所有模块都从它出发

- **资源字段**(填字段精确名,标 public<LOCAL_PRIVATE>,类型):
  - 金币:`<FILL_GOLD_FIELD_NAME>` : `<int 还是 long?>` (访问性:`<public<LOCAL_PRIVATE>>`)
  - 食物:`<FILL_FOOD_FIELD_NAME>` : `<type>`
  - 木材:`<FILL_WOOD_FIELD_NAME>` : `<type>`
  - 石材:`<FILL_STONE_FIELD_NAME>` : `<type>`

  > 如果资源不是直接字段而是 `Dictionary<ResourceType, int>` 之类,在这里注明实际数据结构,
  > 后续模块代码需要从 `Field name → Dictionary[key]` 调整。

## 周期性方法(Postfix 钩子)

- 全限定签名:`<FILL_ECON_TICK_METHOD>(args)`(例:`UpdateEconomy()` / `OnDayEnd()` / `Tick()`)
- 调用频率:每 `<game-tick / second / day>` 一次
- 适合 Postfix:是 / 否(如果不适合,用什么替代)

## 已排除的备选

- `<列出查看后排除的方法,理由>`

## 反编译片段(关键代码,直接粘贴)

```csharp
// <FILL_SETTLEMENT_TYPE_FQN>.<FILL_ECON_TICK_METHOD>
// 从 dnSpy 选中方法 → Ctrl+C(Copy IL/decompiled)
```

## Module code 替换检查表

填完上面之后,Phase 5 EconomyCheats.cs 里需要把这些占位替换:

- `SettlementTypeFqn` ← `<FILL_SETTLEMENT_TYPE_FQN>`
- `EconTickMethod` ← `<FILL_ECON_TICK_METHOD>`
- `GOLD_FIELD_NAME` ← `<FILL_GOLD_FIELD_NAME>`
- `FOOD_FIELD_NAME` ← `<FILL_FOOD_FIELD_NAME>`
- `WOOD_FIELD_NAME` ← `<FILL_WOOD_FIELD_NAME>`
- `STONE_FIELD_NAME` ← `<FILL_STONE_FIELD_NAME>`
