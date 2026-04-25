# Roadmap — Lords & Villeins Cheats

> 当前版本:**v0.1.0**
> 状态:**Partially in-game verified** (Economy.Money / Economy.Food / Time pass;Pawn 和 Build 待测;Wood/Stone 已撤出 v0.1 范围)
> 最后更新:2026-04-25

---

## 已完成 (v0.1.0)

### Loader / Core
- BepInEx 5.4.21 + HarmonyX 2.10 集成
- `Plugin.cs` 完整接线:版本检查、存档自动备份(轮转 5 份)、ConfigEntry 持久化、Harmony PatchAll、F1 面板
- 模块化 Core 框架(`ICheatModule` + `ModuleRegistry` + `ModuleStatus` 状态机 + `GuiManager` IMGUI Tab + `GameRefs` 单例引用 + `HarmonyHelpers.SafeRun` 包错)
- 全局 "Disable All" 红色按钮(主菜单/游戏中状态指示)
- 游戏版本兼容白名单(当前 `1.6.15`),不在名单内仅 warning 不阻断
- 启动期自动备份所有 Steam ID 下的 `.sgz` 存档

### 4 个 CheatModule(均 `Status = Ok`)
| 模块 | 功能 | 钩子 |
|---|---|---|
| **Economy** | 一键 +10000 Money / +1000 Food(Wood/Stone 已撤,见下方限制) | `GameManager.Update` Postfix → `Inventory.AddResource` |
| **Pawn** | 全家族:饱腹(Eat=1)、满血(HP=maxHP)、高心情(baseMood/Happiness=1)、解锁全技能 | `GameManager.Update` Postfix + `WorldNPC.aquiredSkills` |
| **Time** | 速度倍率覆写(0–100x,vanilla max 32x) | `GameManager.Update` Postfix → 写 private `gameSpeedMultiplierBySpeedLvl` |
| **Build** | 跳过材料检查(NPC 始终认为材料够) | `BuildBlueprint.HasResourcesForBlueprint` Prefix 短路 |

### 生命周期
- `BootstrapHooks`(独立静态类,非 module): patch `LoadingManager.InitGame` Postfix → `GameRefs.IsReady = true`;`LoadingManager.ExitToMainMenu` Postfix → `GameRefs.Reset()`
- `GameRefs.IsReady` 通过 `GuiManager` 在面板显示 ● in-game / ○ menu

### F1 / OnGUI 宿主架构(v0.1.0 实测后改造)
- 这游戏的 BepInEx Manager GameObject **不被 Player Loop 调度**:`BaseUnityPlugin.Update/OnGUI` 永远不 fire,`new GameObject(...) + DontDestroyOnLoad` 也不 fire(GameObject 在 7 秒后被某机制 destroy)
- 解法:`Core/CheatsRunner.cs` 是一个独立 MonoBehaviour,通过 `EconomyCheats.OnGameTick_Postfix` 每 tick 检查并在游戏内 `GameManager.gameObject` 上 AddComponent。寄生在游戏自己的 GameObject 上,Player Loop 一定调度它
- `GameManager` 在存档加载时会被 destroy 重建 → `CheatsRunner.OnDestroy` 清 `Plugin._attachedRunner` flag,下一帧 postfix 自动 re-attach
- F1 切换走 IMGUI 自己的 `Event.current`(legacy `Input.GetKeyDown` 与 OnGUI 同帧 toggle 会双触发翻回原状,已禁用)
- 加 frame guard 防 IMGUI 多 pass 同帧重复 toggle

### 工具与流程
- `tools/install.ps1` — 推送构建到 BepInEx/plugins
- `tools/tail-log.ps1` — 实时跟看 LogOutput.log
- `tools/run-and-check.ps1` — 自动化 build+install+launch+log-parse,exit code PASS/FAIL/WARN
- `dotnet test` — xUnit 测试 10 passed (1 skipped — BepInEx NuGet stub 限制 ConfigFile runtime instantiation)
- BepInEx ConfigurationManager 兼容(用户可不依赖我们的 IMGUI 也能改值)

### 调研 + 文档
- `refs/01..06-*.md` — ilspycmd 反编译产出的游戏内部架构笔记(全部入仓)
- `docs/superpowers/specs/` — 设计文档(已 approved 并实施)
- `docs/superpowers/plans/` — 实施计划(已执行)
- `docs/smoke-checklist.md` — 手动冒烟清单(中文)
- `README.md` — 安装、开发、测试快速指南
- `LordsAndVilleins.slnx` + `Directory.Build.props` + `NuGet.Config`(BepInEx 私有 feed)

---

## 已知限制(v0.1.0)

| 限制 | 来源 | 影响 |
|---|---|---|
| **Wood / Stone 不能凭空增加(已撤出 v0.1 范围)** | `Inventory.AddResource` 第一行 `if (!allowedResources.Contains(name)) return false`。玩家个人 inventories(钱包 / 食物袋)的 `allowedResources` 不含 Wood/Stone — 测了 40+ 个 inv 全 reject。Wood/Stone 只能存在 stockpile / lumberyard 类 storage building 的 inventory 里 | v0.1 panel 已删 Wood/Stone 按钮,只保留 Money/Food。要支持需:走世界级 stockpile(`FindObjectsOfType<Stockpile>()`)或 reflect 进 `existingResourceContent[Wood].Gain()` 绕过 allowedResources check |
| **玩家货币是 `ResourceName.Money`,不是 `GoldCoins`** | 实测 `GoldCoins`/`SilverCoins`/`CopperCoins` 全 0,玩家钱包存在 `ResourceName.Money`(120K+) | `refs/01-economy-research.md` 原稿写了 `GoldCoins` 占位,代码已修正用 `Money`。研究笔记需补一行实测说明 |
| **`BaseUnityPlugin.Update/OnGUI` 在这游戏不 fire** | 这游戏 Player Loop 不调度 BepInEx Manager GameObject,也不调度独立 `DontDestroyOnLoad` 场景的 GameObject(独立 GO 在 7s 后被 destroy) | 已通过 `CheatsRunner` MonoBehaviour 寄生到 `GameManager.gameObject` 解决。代价:F1 在主菜单不可用(此时 GameManager 还没实例化),只在进存档后才有 |
| **F1 切换不能用 `Input.GetKeyDown`** | 同帧内 legacy Input + OnGUI Event 双触发会 toggle 两次回原状 | 已改为只走 IMGUI `Event.current`,加 frame guard |
| **BepInEx console 默认 disabled** | `BepInEx.cfg` 默认 `[Logging.Console] Enabled = false` | 已改 true,user 启动时能看到 live 输出 |
| Build 模块只跳过"材料检查门",不跳过实际递送 | 游戏建造分两层:`HasResourcesForBlueprint` 是 NPC 决策入口,但 `inventory.SpendResources` 仍由 ticket 系统驱动 | NPC 仍会去仓库取材料;若仓库真的没有,blueprint 会卡在"材料运送"阶段。要"完全免费",需另 patch `Inventory.SpendResources` 或直接给 blueprint inventory 注入 |
| Pawn 模块"高心情"是直接覆盖 `baseMood`/`baseHappiness` | 实际心情由各 need 的 mood impact 叠加 | 玩家可能看到设的值被游戏下一帧重新计算覆盖;最稳的做法是 lock 全部 need buffer 到满。下版本考虑加 |
| `InventoryProxy → Inventory` 的 walk 用了私有字段反射 | `PlayerManager.playerInventory.GetInventory()` 返回 `IInventoryView`(proxy),不直接是 `Inventory` | 游戏更新若改 InventoryProxy 内部结构,Economy 模块可能 silently 失效。`run-and-check.ps1` 探测不到这种"patch 成功但行为退化"的情况 |
| 时间模块超过 32x 可能让 AI/物理崩 | 游戏自身只暴露到 5 档(对应 32x),我们直接写 multiplier 字段,可超出 | UI 已加注释提示;实际上限取决于具体场景 |
| 没有"按 NPC 单选"的精细控制 | 当前所有 Pawn cheat 是"全家族" | 想只给某村民解锁技能或回血,得手动改 ConfigEntry。下版本可加面板内 NPC dropdown |
| Economy lock 功能已撤(v0.1 自用阶段简化) | user 只需要 +N 一次性按钮,lock UI 删除 | ConfigEntry binding 仍保留以保持 cfg 兼容性,但 panel 不显示 |
| UI 只英文 | 自用阶段决策 | 公开发布前需补中文 |
| `ModConfigTests.Constructor_BindsDefaults_WhenFileMissing` 被 skip | BepInEx.Core NuGet 5.4.21 是 compile-only meta-package,test runtime 加载 ConfigFile 失败 | 不影响生产代码;测试覆盖率上的小遗憾 |
| 不碰 Steam 成就 | 设计决策(spec §8 红线 4) | 用了作弊后,理论上仍可解锁成就 — 不主动屏蔽,玩家自负 |

---

## v0.1.0 in-game verification(2026-04-25 进行中)

| 模块 | 项 | 结果 | 备注 |
|---|---|---|---|
| Loader | F1 弹窗(进存档后) | ✅ pass | 主菜单 GameManager 未实例化,F1 此时无效;进存档后 ~1s attach 上 |
| Loader | "Disable All" 按钮 | ⏳ untested | 自用阶段降级,panel 暂未重显该按钮 |
| Economy | +10000 Money | ✅ pass | `Inventory.AddResource(ResourceName.Money, ...)` 第一个 inv 即接受 |
| Economy | +1000 Food | ✅ pass | 第一个 inv reject(allowedResources),第二个 inv 接受 |
| Economy | +1000 Wood / Stone | ❌ 撤出 v0.1 | 玩家个人 inventory 全 reject,见已知限制 |
| Time | OverrideSpeed | ✅ pass | 游戏速度被实测覆写 |
| Pawn | Clear hunger / Max HP / Max mood / Max skills | ⏳ untested | 待 user 在 panel 试 |
| Build | FreeBuilding | ⏳ untested | 待 user 造个建筑测 |

## v0.2 路线图(短期,1–2 周)

按优先级排:

### Must
- [ ] **完成 Pawn / Build 游戏内冒烟**,把结果写进上节
- [ ] **Wood/Stone 加资源**:走 `FindObjectsOfType<Stockpile>()` 或 reflect 进 `existingResourceContent[Wood].Gain()`,绕过 `allowedResources` check
- [ ] **Economy 真锁机制**:`Inventory.SpendResources` 也 patch,使 Lock Money ON 时钱真的不掉(目前 v0.1 已删 lock UI,要恢复 lock 必须做这项;否则保持只用 +N 按钮)
- [ ] **Pawn need 全 lock**:patch `AgentNeed.SetValue` 或在 tick 里把所有 needs(Sleep / Comfort / Social 等)都顶满,而不只是 Eat

### Should
- [ ] **NPC 单选 UI**:Pawn Tab 加 dropdown 选具体村民,只对选中的应用 cheat
- [ ] **资源类型扩展**:目前只 Money/Food,扩展到所有玩家 inventory 接受的 ResourceName
- [ ] **Build 全免费**:patch `Inventory.SpendResources` 在 FreeBuilding ON 时返回空 ticket
- [ ] **中文 UI**:加 `Locale/zh-CN.json`,GuiManager 读 ConfigEntry 选语言
- [ ] **F1 在主菜单也可用**:不依赖 GameManager.gameObject,而是 hook `MainMenu` / `LoadingScreen` 类的某个 always-alive MonoBehaviour 作为 host

### Could
- [ ] **存档编辑器(独立工具)**:offline `.sgz` 解析 + 修改(spec 排除项,但实施完成后回头看意义可能更大)
- [ ] **公开发布准备**:Nexus Mods 上传、贴 README 多语版、收集兼容性反馈
- [ ] **CI**:GitHub Actions 跑 `dotnet test`(只能跑 layer 1)

---

## 长期方向

- **支持游戏 DLC / 新版本**:游戏更新后,本 ROADMAP "已知限制"可能需要加新条目;`refs/06-version-research.md` 加新 hash
- **跨游戏复用 Core 框架**:如果 `gamecheats/` 仓库里加第二个 Unity Mono 游戏的 mod,把 `Core/` 抽成共享 NuGet 包(目前 spec §10 明确排除"跨游戏抽象",等真出现第二个游戏再说)
- **从 IMGUI 升 UGUI**:如果用户想要"和游戏 UI 同风格"的面板,工作量 3-5 倍,等社区有需求

---

## 版本历史

### v0.1.0 — 2026-04-22(code complete) → 2026-04-25(部分游戏内验证)
- 项目从 0 到 4-module-Ok,1 天内完成
- 21 commits,覆盖:setup → spec → plan → core (TDD) → modules (reflection) → tooling → docs
- 自动化验证(run-and-check.ps1)PASS,游戏内冒烟分两次:
  - 04-22:仅自动化层确认 patch summary 4/4 ok
  - 04-25:游戏内实测 Loader.F1 / Economy.Money / Economy.Food / Time pass;Pawn / Build 待续;Wood/Stone 撤出范围

#### 决策摘要
- 形态:游戏内 mod,**不**做外部 trainer / 存档编辑器
- 架构:模块化(Core + 4 模块),**不**做单文件大 Plugin / 反射数据驱动框架
- UI:Unity IMGUI,**不**做 UGUI / 纯热键
- 受众:自用为主,代码按发布质量,UI 双语 + Nexus 发布物料延后
- 调研工具:`ilspycmd`(CLI 反编译)替代 dnSpy GUI,实现 controller 自动化
- (2026-04-25 增补)Economy v0.1 自用阶段简化:删除 lock UI,只保留 +N 按钮;Wood/Stone 撤出。Lock 路径代码 + ConfigEntry 保留为 v0.2 真锁的基础

#### 主要踩坑
- 游戏没有 `Settlement` 类,资源在 `Inventory` (`Dictionary<ResourceName, Resource>`),需用 `AddResource` API 而非字段直写
- Skills 是 `HashSet<SkillName>` 二进制,不是整数等级
- `PlayerManager.playerInventory.GetInventory()` 返回 `InventoryProxy` 不是 `Inventory`,必须 walk 私有 `inventoryViews` 列表找具体 Inventory
- BepInEx.Core NuGet 包是 compile-only stub,test 运行时无法 instantiate ConfigFile
- 游戏 exe FileVersion 是 Unity 引擎版(`2021.3.45.x`),真实游戏版本要从 `Player.log` 取
- (2026-04-25)**玩家货币是 `ResourceName.Money` 不是 `GoldCoins`**:`refs/01-economy-research.md` 原写 `GoldCoins` 占位,实测玩家钱包 Money=120K,GoldCoins=0
- (2026-04-25)**`BaseUnityPlugin.Update/OnGUI` 在这游戏不 fire**:Player Loop 不调度 BepInEx Manager GameObject。独立 `DontDestroyOnLoad` GameObject 也不行(7s 后被 destroy)。解决:`CheatsRunner` MonoBehaviour 寄生到 `GameManager.gameObject`,通过 `EconomyCheats.OnGameTick_Postfix` 每 tick 检查 attach
- (2026-04-25)**`GameManager` 在存档加载时被 destroy 重建**:`CheatsRunner.OnDestroy` 必须清 attach flag,下一帧 postfix 自动 re-attach
- (2026-04-25)**legacy `Input.GetKeyDown` 与 OnGUI Event 同帧双触发**:删 legacy 路径,只用 IMGUI `Event.current.KeyDown` + frame guard
- (2026-04-25)**Wood/Stone 不能凭空增加**:`Inventory.AddResource` 第一行 `if (!allowedResources.Contains(name)) return false` 拒掉,玩家所有个人 inventory 的 allowedResources 不含 Wood/Stone
