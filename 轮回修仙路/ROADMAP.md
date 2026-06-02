# 轮回修仙路 Cheats Roadmap

Last updated: 2026-06-02

## Current Status

Status: **v0.0.6 — 游戏内大幅验证通过：GameRefs 捕获修复（hook 玩家 stat getter）、修为/角色改增量 UI、等级 +N 修正、背包物品库选择器（PropsData 全道具 + bag_type 子分类 tab）。已知遗留：角色属性升级后被游戏重算重置（需加"锁定"每帧重应用）；spawn 字段拷贝改进但物品入包效果待最终确认。详见版本历史 + 下一步。**

Game/runtime:

- 轮回修仙路 (Steam AppID 1993150)
- Game path: `<STEAM>\steamapps\common\轮回修仙路`
- Engine: **Unity IL2CPP** (confirmed via `GameAssembly.dll`)
- Mod loader: BepInEx 6 IL2CPP (installed and verified)
- Save format: JSON (`playerData.txt`, `packData.txt`, etc.)

## Completed

- [x] Project scaffold
- [x] BepInEx 6 IL2CPP .csproj with Il2CppInterop references
- [x] Core framework: Plugin, ICheatModule, ModuleRegistry, GuiManager, ModConfig, ModuleStatus, CheatsRunner, GameRefs
- [x] Utility: SaveBackup, HarmonyHelpers
- [x] Time module placeholder (Unity Time.timeScale)
- [x] xUnit test skeleton
- [x] PowerShell install/tail-log tools
- [x] BepInEx 6 IL2CPP installer (`tools/BepInEx-Unity.IL2CPP-win-x64.zip` + `install-bepinex.ps1`)
- [x] BepInEx 6 IL2CPP installed to game directory (verified)
- [x] Research checklist scaffold
- [x] **Runtime type & field scanner verified in-game** (19,395 type matches, 1,400 field scan lines)
- [x] **Key game types identified and documented** in `refs/01-discovered-types-summary.md`

## Discovered Types (Verified In-Game)

See `refs/01-discovered-types-summary.md` for full details.

| 类型 | 全名 | 用途 |
|---|---|---|
| `UnitData` | `DataLib.UnitData` | 战斗属性 (HP, 物攻/法攻, 物防/法防, 移速, 攻速) |
| `CharacterData` | `CharacterData` | 玩家角色 (经验, 等级, 道心, 突破数据, 位置) |
| `CharacterBaseAttributesData` | `Configuration.CharacterBaseAttributesData` | 基础属性配置表 |
| `FakeInventoryData` | `FakeInventoryData` | 背包系统 (AddItem, AddCoin, Clear, 多分类列表) |
| `SpiritRoot` | `DiscipleSpiritData+SpiritRoot` | 灵根系统 (五灵根字典) |
| `ExperienceData` | `DataLib.ExperienceData` | 经历/人生事件系统 |
| `RoleUpgradeData` | `Configuration.RoleUpgradeData` | 角色升级配置 (每级属性) |
| `SkillData` | `DataLib.SkillData` | 技能数据 |
| `DanYaoData` | `DanYaoData` | 丹药数据 |
| `PetData` | `DataLib.PetData` | 宠物数据 (129 属性) |
| `HeartAchievementMethod` | `HeartAchievementMethod` | 心法/功法 |

### 查找失败的类型
`PlayerUnitData`, `BackpackGoods`, `Cultivation`, `Practice`, `LifeTime`, `Linggen`, `RefiningDanData` — 不存在或名字不同。

## Next Work

### Phase 1 — Runtime Modules (implemented; code-complete, awaiting in-game smoke)

实现见分支 `feat/lunhui-cheat-panel`,设计/计划见 `docs/superpowers/`。UI 改为 分类侧栏 + 搜索/排序 面板 (`GuiManager` + `Core/Gui/*`)。

1. ✅ **PlayerStats** (`player`) — 反射读写 `UnitData.curPhysicalAttacks/curSpellAttacks/MoveSpeed/bigWorldFlySpeed`,每项可锁定
2. ✅ **GodMode** (`godmode`) — 每帧锁 `UnitData.curHp = maxHp`
3. ✅ **Inventory** (`inventory`) — 浏览 `All*` 列表(IL2CPP 用 `Count`/`Item` 反射迭代)+ `AddItem/AddCoin`;二期试构造 `BaseRewardData` by id
4. ⚠️ **Cultivation** (`cultivation`) — `curDaoxin` 可写 ✅;`currentExp`/`currentLevel` 只读 `{get;}`,**已改走 dump 到的真实方法**:等级→`CharacterData.UpdateLevel(level, isSavePos)`,经验→游戏自带 `CharacterData.FieldSetter("CharacterData","currentExp",val)`(经 `ReflectAccessor.TryInvoke` 反射调用);灵根目前只读显示。**待 Windows 游戏内冒烟确认两条写入路径真生效**
5. ✅ **Time** (`time`) — placeholder + value-change guard,归类 通用

待办: 在 Windows 真机跑 `docs/smoke-checklist.md`,确认行为;修 Cultivation 经验/等级写入路径;灵根编辑(二期)。

### Handoff Notes

- `Plugin.cs` 中的 `AttachRunnerToGameHost()` 已增强：自动探测常见宿主类型名，fallback 到独立 GameObject
- `BootstrapHooks.cs` 已增强：如果 `SceneController` 不存在则立即 fallback attach
- `GameRefs.FindByType<T>()` 已提升为 public，方便模块查找运行时对象
- 第三方 trainer zip 已提交到 repo，可用于参考或 sandbox 测试

## Known Risks

- IL2CPP method names may differ from decompiled C# source; always verify via dump.
- Game classes may use Chinese names; preserve original names in research notes.
- Runtime injection of cultivation/experience systems may trigger anti-cheat or corrupt saves.
- Unity `Time.timeScale` affects UI animations; may need selective patching instead.
- `FakeInventoryData.AddItem()` 可能需要有效的 `BaseRewardData` 实例，不能传 null。

## Version History

### v0.0.6（游戏内验证：捕获修复 + 增量 UI + 物品库选择器）

一次长会话，多数功能在 Windows 游戏内实测。

**GameRefs 捕获修复（核心）**
- 根因：`CharacterData : Object`（非 MonoBehaviour），世界扫描永远找不到；原 hook 的
  `init/AddDaoxin/LeveUp` 都是稀有/一次性事件，存档已加载时不触发 → 玩家数据从没捕获。
- 修：`BootstrapHooks` 改 hook 玩家 HUD 每帧读的属性 getter（`get_currentLevel`/`get_currentExp`），
  被动捕获活的 CharacterData；`GameRefs.PassCharacterData` 改"首次非空即锁定"。
- 注：`get_curDaoxin`/`get_unitData` 是 IL2CPP 字段访问器、patch 不了（无害，前两个够用）。
- **游戏内确认**：日志 `[GameRefs] Captured CharacterData via hook. type=CharacterData`。✅

**增量 UI（修为 + 角色）**
- 修为：显示当前 等级/道心 + 增量框 + 「+N」。道心→`AddDaoxin(delta)`；等级→`UpdateLevel(delta)`。
  **等级 bug 修正**：`UpdateLevel(n)` 是"加 n 级"非"设为 n"（实测 13+`UpdateLevel(14)`→27），改传 delta。✅
- 经验：只读（`currentExp` 无 setter、派生值；设等级即可）。
- 角色：物攻/法攻/移速/飞速 同款增量 UI（`UnitData` 可写属性，`TrySet` 当前+delta）。✅ 生效。

**背包物品库选择器（重点新功能）**
- 配置库入口：`DataLib.GMDataBaseSystem` 静态非泛型重载
  `List<Object> SearchConfAllStatic(DBName, Il2CppSystem.Type)`（泛型 `<T>` 重载在 IL2CPP 下
  反射调不动，此重载把元素 Type 当参数传）。元素 `Il2CppSystem.Object` 按 Pointer 重包成具体类型。
- 枚举 `PropsData`（DBName.Props=24）= 全道具 862 条；按 `bag_type` 分**子分类 tab**（全部/丹药/装备/材料/…）。
- DBName 关键值：Props=24, RewardLibrary=26, EquipLibrary=75, DanyaoLibrary1=77, Jindan=66, TalismanLibrary=36。
- spawn：点物品「+数量」→ 建 `BaseRewardData`，把 PropsData 配置字段拷进去
  （id→rewardId、bag_type→bagType、prop_type→propType 及跨枚举 rewardType、prop_quality→quality、
  stack_num、prop_use→propUse、name…）→ `AddItem`。

**框架**
- `ReflectAccessor.TryInvoke(instance, method, out result, params args)` — 按名+参数个数反射调 IL2CPP 方法。
- 面板加宽（640×600，运行时强制下限覆盖旧配置）+ tab/内容加边距。

### 已知遗留（下次优先）

1. **角色属性升级后重置**：物攻等增量是一次性写入，玩家**升级时游戏按"基础属性+等级"重算 → 覆盖**。
   需加"锁定"开关：`PlayerStats.OnUpdate` 每帧重应用记录的目标值（即旧 lock 机制，增量模型下保留最后设定值）。
2. **spawn 入包效果待最终确认**：v0.0.6 已把配置字段拷进 reward（对比 `[SpawnDump] REAL` vs `MINE`），
   AddItem 返回 True，但需游戏内确认物品真出现且可用；若仍不行，考虑游戏工厂 `RewardLibararyDataControll.CreateReward`。
3. 子分类 tab 标签：`BagLabel` 把 BagType 英文名映射中文，未覆盖的回退英文（看 `[Props]` 日志补全）。
4. 各表名字字段：部分库（装备/丹药等）`strFields` 少，名字可能走本地化，picker 仅显有的字符串字段。

### v0.0.5（修为 经验/等级 写入路径）

- **问题**：`currentExp`/`currentLevel` 是 IL2CPP 只读自动属性 `{get;}`，经 Il2CppInterop
  无托管 backing field，`ReflectAccessor.TrySet` 必败（v0.0.4 仍在尝试，UI 显 ✗）。
- **修**：dump (`refs/lunhui-fieldscan.txt` CharacterData 段) 找到真实方法：
  `UpdateLevel(Int32 level, Boolean isSavePos)`、`LeveUp(Int32, Boolean, Boolean)`、
  游戏自带 native 字段写入器 `FieldSetter(String typeName, String fieldName, Object val)`。
  - `ReflectAccessor` 新增 `TryInvoke(instance, method, out result, params args)`——按名+参数个数
    匹配 IL2CPP 方法，逐参 Coerce 后反射调用，带缓存。
  - `Cultivation` 写入按钮:道心→`TrySet(curDaoxin)`;等级→`UpdateLevel(level,true)`;
    经验→`FieldSetter("CharacterData","currentExp",exp)`;写后 `_synced=false` 重读回显游戏实际接受值。
- 编译：0 错误（6 个预存 nullable 警告）。已装 `BepInEx\plugins\LunHuiCheats\`。
- **待 Windows 游戏内确认**：进世界按 P → 修为面板改等级/经验 → 点写入 →
  日志 `[Cultivation] write ... level=True`(方法存在=True)+ **游戏内等级/经验数值真变化**。
  若 `UpdateLevel` 只刷显示不改数据，改试 `LeveUp(level,false,true)`；
  若 `FieldSetter` 的 fieldName 命名不符，看 LogOutput.log 报错后据真实名调整(别猜)。

### v0.0.4（GameRefs 解析 bug 修复 + 库存日志降噪）

- **修 CharacterData/UnitData 永远 null 的真因**：`GameRefs.PassInventory` 在首次捕获
  库存时把共享标志 `_resolved=true`，于是 CharacterData 的多阶段回退扫描（Phase2-5）
  被永久跳过 → `CharacterData`/`UnitData`（=`CharacterData.unitData`）始终 null →
  PlayerStats/GodMode/Cultivation 面板显示"未找到"。
  修：删除 `_resolved` gate，getter 在**自身目标**为 null 时按 1.5s 节流重跑 `ResolveAll`；
  Phase5 深转储（写 `lunhui-deepdump.txt`）加 `_deepDumpDone` 一次性守卫，避免重扫刷盘。
- **库存刷屏**：`RebuildRows` 每 ~1s 给 8 个列表各打 found/loaded 日志 → 改 `_logged` 一次性。
- 测试：16/19 通过；3 失败为环境问题（测试宿主加载不到 `UnityEngine.CoreModule`，
  `SaveBackup`/`ModuleRegistry` 经 `Plugin` 静态初始化触发），非本次改动引入。
- 待 Windows 游戏内确认：进世界按 P → 各模块找到活数据；日志 `[GameRefs] Done: CharacterData=True`。

### v0.0.3

- Categorized/sortable/searchable IMGUI cheat panel (sidebar + search/sort; `GuiManager` + `Core/Gui/*`).
- Plumbing: `ReflectAccessor`, `FilterSort`, `ItemBrowserModel`, `ModuleRegistry.OnUpdateAll/Categories`, cached `GameRefs`, per-frame `OnUpdate` dispatch.
- Modules: GodMode, PlayerStats, Inventory (browse All* via IL2CPP Count/Item reflection + AddItem/AddCoin + add-by-id), Cultivation (curDaoxin writable; exp/level read-only — write path TBD).
- xUnit: ReflectAccessor / FilterSort / ItemBrowserModel / registry tests (game-independent, pass on any machine).
- Code-complete on branch `feat/lunhui-cheat-panel`; in-game smoke pending on Windows.

### v0.0.2

- Runtime scanner verified in-game.
- Key types identified and documented.
- Framework hardened (auto-discovery, fallback attach, value guards).
- Handoff-ready for module implementation on another machine.

### v0.0.1

- BepInEx 6 IL2CPP project scaffold created.
- Core framework modeled after LordsAndVilleins project.
- IL2CPP engine confirmed.

### v0.0.0

- Empty scaffold.

## Known Risks

- IL2CPP method names may differ from decompiled C# source; always verify via dump.
- Game classes may use Chinese names; preserve original names in research notes.
- Runtime injection of cultivation/experience systems may trigger anti-cheat or corrupt saves.
- Unity `Time.timeScale` affects UI animations; may need selective patching instead.

## Version History

### v0.0.1

- BepInEx 6 IL2CPP project scaffold created.
- Core framework modeled after LordsAndVilleins project.
- IL2CPP engine confirmed.

### v0.0.0

- Empty scaffold.
