# 05 — Bootstrap 调研(GameRefs 生命周期)

> 调研于 2026-04-22 (ilspycmd 10.0.0.8330)

## 目的

找到两个钩子点:
1. **游戏世界加载完成**:Postfix 后把 `GameRefs.Settlement` 赋值,`GameRefs.IsReady = true`
2. **回主菜单 / 退出存档**:Postfix 后 `GameRefs.Reset()`

## 搜词清单

`GameManager`, `World`, `Main`, `Bootstrap`, `Game`, `Application`, `App`,
`SceneLoad`, `OnLevelLoaded`, `Awake`, `Start`,
(以及 Phase 1.1 里确认的 `EconomyManager`/`GameManager` 全限定类名，搜索它的 "Used By")

## "游戏已加载"信号

- 钩子类:`LoadingManager`（全局命名空间，MonoBehaviour，`LoadingManager.instance`）
- 钩子方法:`LoadingManager.InitGame(bool loadedSaveFile)`
  - 在所有 `AfterLoadingFinished()` 协程完成后被调用
  - 签名：`public void InitGame(bool loadedSaveFile)`
  - 当 `gameMode != MainGameState.MainMenu` 时才真正进入游戏
- Patch 类型：`Postfix`
- 在该方法 Postfix 中如何拿到各单例：
  - 方案 C（推荐）：所有管理器都是静态单例，直接访问
    - `GameManager.instance`：游戏时间/速度管理
    - `EconomyManager.instance`：资源工厂
    - `PlayerManager.instance`：玩家/家族管理
    - `NPCManager.instance`：NPC 数据库
    - `BuildManager.instance`：建造系统
  - 方案 A 变体：`__instance` 是 `LoadingManager`，通过 `!__instance.IsInMainMenu()` 判断是否在游戏中

- **注意**：游戏没有名为 Settlement 的单例类。`GameRefs.Settlement` 应存储为对 `PlayerManager.instance` 或 `GameManager.instance` 的引用。

## "游戏已卸载"信号

- 钩子类:`LoadingManager`
- 钩子方法:`LoadingManager.ExitToMainMenu()`
  - 签名：`public void ExitToMainMenu()`
  - 调用时已停音乐/环境音，然后触发 `ReloadMainGameState(MainGameState.MainMenu)` 协程

## 补充说明：InitGame 完整逻辑

```
LoadingManager.ReloadMainGameState() 协程：
  → 加载/生成数据 (LoadSaveData/BindSaveData)
  → AfterLoadingFinishedOnAll() 协程（依次调用所有 Manager.AfterLoadingFinished()）
  → InitGame(loadedSaveFile)      ← 钩点在此
```

`AfterLoadingFinishedOnAll()` 调用顺序（部分）：
1. `gameManager.AfterLoadingFinished()`
2. `economyManager.AfterLoadingFinished()`
3. `playerManager.AfterLoadingFinished()`
4. `NPCManager.AfterLoadingFinished()`
5. ...（共约20+个 manager）

## 直接可用的 patch 代码

填完上面后,这两段应该可以直接抄进 EconomyCheats.cs 末尾的 GameRefsBootstrap 类:

```csharp
[HarmonyPatch(typeof(LoadingManager), "InitGame")]
[HarmonyPostfix]
static void OnLoad_Postfix(LoadingManager __instance, bool loadedSaveFile)
{
    // 排除主菜单（MainMenu 时 IsInMainMenu() 返回 true）
    if (__instance.IsInMainMenu()) return;

    // 游戏已完全加载，所有单例就绪
    // GameRefs 不需要 Settlement 对象，直接标记就绪
    LordsAndVilleinsCheats.Core.GameRefs.IsReady = true;
    LordsAndVilleinsCheats.Plugin.Registry?.NotifyGameReady();
}

[HarmonyPatch(typeof(LoadingManager), "ExitToMainMenu")]
[HarmonyPostfix]
static void OnUnload_Postfix()
{
    LordsAndVilleinsCheats.Core.GameRefs.Reset();
    LordsAndVilleinsCheats.Plugin.Registry?.ResetGameReady();
}
```

## 主菜单 vs 游戏中的区分

- `LoadingManager.instance.IsInMainMenu()` → `bool`（返回 true = 在主菜单）
  - 实现：`return gameMode == MainGameState.MainMenu;`
- `GameManager.instance.IsMainMenuSaveFile()` → `bool`（保存文件类型判断）
- 推荐：`!LoadingManager.instance.IsInMainMenu()`

## GameRefs 中各功能对应的实际单例路径

| 功能 | 访问路径 |
|---|---|
| 玩家仓库 Inventory | `PlayerManager.instance.playerInventory.GetInventory()` |
| 玩家家族 NPC 列表 | `PlayerManager.instance.GetWorldRulingOrganization().GetFamilyMembersAsReference()` |
| 游戏时间/速度 | `GameManager.instance`（字段通过 AccessTools 读写） |
| 建造蓝图数据 | `BuildManager.instance` |
| 资源工厂/经济 | `EconomyManager.instance` |
| NPC 数据库（全部） | `NPCManager.instance.worldNPCDB` |

## 完整 Bootstrap 状态图

```
主菜单 (IsInMainMenu=true)
    → 用户开始/加载游戏
    → LoadingManager.ReloadMainGameState() 协程
        → AfterLoadingFinishedOnAll()
        → InitGame(loadedSaveFile)  ← Postfix: GameRefs.IsReady=true
游戏中 (IsInMainMenu=false)
    → 用户退出/返回主菜单
    → LoadingManager.ExitToMainMenu()  ← Postfix: GameRefs.Reset()
主菜单 (IsInMainMenu=true)
```
