# 07 — Favor Points 调研

> 调研于 2026-04-25 (ilspycmd 10.0.0.8330)

## 搜词清单

`Favor`, `FavorPoints`, `Royalty`, `Royal`, `King`, `Tax`

## 关键类型

- **存储类**:`RoyaltyManager`(MonoBehaviour 单例,`RoyaltyManager.instance`)
- **字段**:`public int favorPoints` — **直接 public 字段**,无 getter/setter
- **变化事件**:`public UnityEvent OnFavorPointsChange` — 加完后 invoke 让 UI 刷新

## 铁证:游戏自带的开发者秘籍按钮

`UICheatDialogue.OnFavorPointsGainClick` (UICheatDialogue.cs:1172-1176):

```csharp
public void OnFavorPointsGainClick()
{
    int.TryParse(favorPointsToGain.text, out var result);
    RoyaltyManager.instance.favorPoints += result;
}
```

游戏自己用的就是这一行。我们的实现照抄,额外补 `OnFavorPointsChange.Invoke()` 让 UI 即时更新(开发者秘籍菜单不在乎 UI 刷不刷新,我们在乎)。

## 不依赖项

- `royalFamily` 可以是 null —— 加 favorPoints 不依赖国王存在
- `RoyaltyResources` / `RoyaltySoldiers` 等 GameModule 可以未启用 —— 这些只 gate 税收评估流程,不 gate 字段读写
- 主菜单状态 —— RoyaltyManager 是 MonoBehaviour 单例,`instance` 可能在主菜单时为 null,所以读写前判 null

## Favor Points 用途参考

游戏内来源(`RoyaltyManager`):
- `InitNewGameData` (line 162-166): 新游戏初值 = `royaltyData.startingFavorPoints`
- `EvaluateTaxes` (line 254-258): 完美完成国王税收任务 → +100
- `AcceptFamily` / `GenerateCustomFamily`: 接受家族 → 扣 favor

也就是 favor points 用来"花 favor 召唤工匠家族",`CalculateCustomFamilyFavorPointPrice` 显示等级 5 工匠 8 人家族大约 200~300。我们提供 +100 / +1000 / +10000 三档应该能覆盖各种使用场景。

## 实现笔记(对应 `Modules/RoyaltyCheats.cs`)

- 不需要 Harmony patch — 单按钮型,触发时直接读写 `RoyaltyManager.instance.favorPoints`
- `Status = ModuleStatus.Ok` 在 Register 直接置位(无 patch 失败可能)
- DisableAll 是空实现 — 加 favor 是一次性动作,无"持续效果"要 disable
- 全部读写包在 `HarmonyHelpers.SafeRun` 里,避免单例尚未初始化时炸出未捕获异常
