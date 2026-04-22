# 05 — Bootstrap 调研(GameRefs 生命周期)

> 调研于 2026-04-XX

## 目的

找到两个钩子点:
1. **游戏世界加载完成**:Postfix 后把 `GameRefs.Settlement` 赋值,`GameRefs.IsReady = true`
2. **回主菜单 / 退出存档**:Postfix 后 `GameRefs.Reset()`

## 搜词清单

`GameManager`, `World`, `Main`, `Bootstrap`, `Game`, `Application`, `App`,
`SceneLoad`, `OnLevelLoaded`, `Awake`, `Start`,
(以及 Phase 1.1 里确认的 `Settlement` 全限定类名,搜索它的 "Used By")

## "游戏已加载"信号

- 钩子类:`<FILL_BOOTSTRAP_TYPE_FQN>`
- 钩子方法:`<FILL_BOOTSTRAP_METHOD>`(常见 `Start`, `OnAfterLoad`, `Awake`)
- Patch 类型:`Postfix`
- 在该方法 Postfix 中如何拿到 Settlement?
  - 方案 A:`__instance` 本身就是 Settlement
  - 方案 B:`__instance.<某字段路径>` 是 Settlement
  - 方案 C:用 `AccessTools.TypeByName(...).GetField("Instance").GetValue(null)` 拿单例
- 实际方案:`<FILL_HOW_TO_GET_SETTLEMENT>`

## "游戏已卸载"信号

- 钩子类:`<FILL_UNLOAD_TYPE_FQN>`
- 钩子方法:`<FILL_UNLOAD_METHOD>`(常见 `OnDestroy`, `Cleanup`, `OnExit`)

## 直接可用的 patch 代码

填完上面后,这两段应该可以直接抄进 EconomyCheats.cs 末尾的 GameRefsBootstrap 类:

```csharp
[HarmonyPatch(typeof(<FILL_BOOTSTRAP_TYPE_FQN>), "<FILL_BOOTSTRAP_METHOD>")]
[HarmonyPostfix]
static void OnLoad_Postfix(<FILL_BOOTSTRAP_TYPE_FQN> __instance) {
    LordsAndVilleinsCheats.Core.GameRefs.Settlement = __instance; // 或 __instance.<path>
    LordsAndVilleinsCheats.Core.GameRefs.IsReady = true;
    LordsAndVilleinsCheats.Plugin.Registry?.NotifyGameReady();
}

[HarmonyPatch(typeof(<FILL_UNLOAD_TYPE_FQN>), "<FILL_UNLOAD_METHOD>")]
[HarmonyPostfix]
static void OnUnload_Postfix() {
    LordsAndVilleinsCheats.Core.GameRefs.Reset();
    LordsAndVilleinsCheats.Plugin.Registry?.ResetGameReady();
}
```

## 主菜单 vs 游戏中的区分

(可选)如何在代码里判断"我现在在主菜单 vs 游戏中"?
- `<FILL_GAME_STATE_CHECK>`(例:`SceneManager.GetActiveScene().name != "MainMenu"`)
