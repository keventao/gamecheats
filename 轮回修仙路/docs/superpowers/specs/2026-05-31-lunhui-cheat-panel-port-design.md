# 轮回修仙路 — 作弊面板移植设计 (Design Spec)

> 日期: 2026-05-31 · 项目: `轮回修仙路/` (LunHuiCheats) · 状态: 设计已批准,待 writing-plans

## 1. 背景与目标

第三方修改器《基础功能库 1.0.4》(ToGameMod) UI 完整(分类/排序/搜索/物品浏览器),用户想把这套**体验**移植进我们自己的 `LunHuiCheats`。

该 trainer 二进制被 Agile.NET VM 壳 + SenseShield DRM 保护,**无法复用、无法静态反虚拟化**(de4dot 在 v6.4 上 `Couldn't restore VM methods`),其精确 UI 目录也未能跑出截图。因此"移植"现实化为:

> **在 LunHuiCheats 里自建一个 分类 + 排序 + 搜索 的作弊面板(神似 trainer),分类用游戏自带的数据,功能按已逆向出的真实游戏类型实现。**

游戏侧逆向已完成(见 §2),Layer 3 数据齐备。本设计覆盖 **UI 外壳** + **作弊模块** 两层,范围为单个实现计划。

### 成功标准
- 面板按分类组织,可搜索、可排序;长列表可滚动。
- GodMode / PlayerStats / Inventory / Cultivation 四个模块在游戏中产生可见行为(各对应一条 smoke-checklist)。
- 纯逻辑(过滤/排序/反射访问器)有 xUnit 覆盖,游戏外可跑。
- `Disable All` 能还原所有锁定;反射失败的模块标 `Broken` 不崩溃。

## 2. 依赖的已验证游戏数据

来源:`refs/01-discovered-types-summary.md`(真机 FieldScanner/TypeScanner 验证)、`refs/lunhui-fieldscan.txt`、`refs/lunhui-typescan.txt`。全部经 `AccessTools.TypeByName(...)` + `FindObjectOfType` 名字反射访问(抗版本)。

| 类型 | 全名 | 用到的成员 |
|---|---|---|
| UnitData | `DataLib.UnitData` | `curHp:Int64` `maxHp:Int64` `curPhysicalAttacks:Int64` `curSpellAttacks:Int64` `MoveSpeed:Single` `bigWorldFlySpeed:Int32` `fightSpeed:Single` `discipleSpiritData:DiscipleSpiritData` |
| CharacterData | `CharacterData` | `unitData:UnitData` `currentExp:Int64` `currentLevel:Int32` `curDaoxin:Int32` |
| FakeInventoryData | `FakeInventoryData` | `AddItem(BaseRewardData,Int32)` `AddCoin(CoinData,Int32)` `Clear()` `size:Int32` + 分类列表 `AllCoins/AllEquips/AllDanYao/AllMaterials/AllPets/AllUseItem/AllFlyTalisman/AllSeedMaterials/...` |
| SpiritRoot | `DiscipleSpiritData+SpiritRoot` | `GetSpiritRootValue(SpiritRootType)` `GetMainSpritType()` `mainSpritRootDic` `spritRootDic` |
| BaseRewardData | `BaseRewardData` | 物品奖励载体(24 属性) — `AddItem` 入参 |
| CoinData | `CoinData` | 货币(25 属性) — `AddCoin` 入参 |

未找到(不依赖):`PlayerUnitData`、`BackpackGoods`、`Cultivation`、`Practice`、`LifeTime`、`Linggen`、`RefiningDanData`。

## 3. 架构

现有框架(保留,不重写):`Plugin.Load()` 注册模块 → `ModuleRegistry` → `GuiManager`(Rect-IMGUI,无 GUILayout)→ `CheatsRunner`(每帧驱动)。游戏访问经 `GameRefs` + `AccessTools` 名字反射。

两层扩展:

### Layer 1 — UI 外壳 (`Core/` + 新 `Core/Gui/`)

把"tab=模块"换成 **分类侧栏 + 顶栏 + 内容区**,全部 Rect-IMGUI。

- `ICheatModule` 增加 `string Category { get; }`(战斗/角色/背包/修为/通用/调试)。
- `GuiManager` 重构:
  - 左侧分类列表(点选切换当前分类)。
  - 顶栏:搜索框(`GUI.TextField`,按模块/功能名过滤)、排序下拉(名称 / 分类 / 最近用)、`Disable All`。
  - 右侧内容区:列出当前分类下、过滤后、排序后的模块,逐个 `DrawGui()`。
- 新复用件(`Core/Gui/`):
  - `GuiWidgets`:Rect 版 `Label/Button/Toggle/IntField/Int64Field/Slider`,内部用一个 `LayoutCursor`(struct,记录 x/y/行高)手算纵向布局。
  - `ScrollList`:封装 `GUI.BeginScrollView`/`EndScrollView`,给长列表用。
  - `ItemBrowser`:数据驱动的物品列表控件 — 输入 `IReadOnlyList<ItemRow>` + 当前分类过滤 + 搜索串 + 排序键,渲染 `名称 | 数量输入 | Add` 行;回调 `Action<ItemRow,int> onAdd`。
  - `FilterSort`(纯静态):`Filter(items, query)` + `Sort(items, key)`,**无 Unity 依赖**,可单测。
- 两种面板原型:**开关型**(GodMode/PlayerStats/Cultivation,用 GuiWidgets)、**浏览器型**(Inventory,用 ItemBrowser)。

### Layer 2 — 作弊模块 (`Modules/`)

每个 `ICheatModule`,名字反射 + value-change guard(参 `TimeCheats`),`DisableAll` 还原。`Register` 时解析关键类型,缺失则 `Status=Broken`。

| 模块 | Id / Category | 目标成员 | 行为 |
|---|---|---|---|
| **GodMode** | `godmode` / 战斗 | `UnitData.curHp/maxHp` | toggle 开启后,`CheatsRunner` 每帧 `curHp=maxHp` |
| **PlayerStats** | `player` / 角色 | `UnitData.curPhysicalAttacks/curSpellAttacks/MoveSpeed/bigWorldFlySpeed/fightSpeed` | 数字框/slider 读写;每项可选 lock(每帧写回) |
| **Inventory** | `inventory` / 背包 | `FakeInventoryData` + `All*` + `AddItem/AddCoin/size` | ItemBrowser 列出 `All*` 各分类现有项;Add → 反射 `AddItem(rewardData, qty)`;货币 → `AddCoin`;一键扩容 `size` |
| **Cultivation** | `cultivation` / 修为 | `CharacterData.currentExp/currentLevel/curDaoxin`;`UnitData.discipleSpiritData→SpiritRoot` | 数字框写经验/等级/道心;灵根读 `GetSpiritRootValue`,改 `spritRootDic` |
| TimeCheats(已存在) | `time` / 通用 | `Time.timeScale` | 仅加 `Category` |
| DebugDiagnostics(已存在) | `debug` / 调试 | scanner | 仅加 `Category` |

**Inventory `AddItem` 难点**:`AddItem` 需有效 `BaseRewardData` 实例。
- **一期**:只对 `All*` 列表里**已存在**的项操作 —— 取其 `BaseRewardData` 引用,调 `AddItem(existing, qty)` 加量。无需凭空构造。
- **二期**(本计划内,排最后):尝试反射构造新 `BaseRewardData`(`Activator` / IL2CPP 构造)按 itemId 加任意物品。失败则保留一期能力并在面板注明。

### Layer 0 — 共用管线 (`Core/`)

- `GameRefs` 增加缓存解析:`CurrentCharacterData`(`FindObjectOfType(CharacterData)`)→ `CurrentUnitData`(`characterData.unitData`)→ `Inventory`(`FindObjectOfType(FakeInventoryData)`)。`OnGameReady` 解析一次;为空/失效时懒刷新(带帧节流,避免每帧 `FindObjectOfType` 开销)。
- `ReflectAccessor`:包装 `AccessTools.Field/PropertyGetter/PropertySetter` 的 get/set,空安全 + 按 `(Type,memberName)` 缓存 `FieldInfo`/`MethodInfo`。模块通过它读写,不直接碰 AccessTools。**纯反射逻辑可单测**(用测试替身类型)。

## 4. 数据流

```
键 P → GuiManager 开关
面板 toggle/数字框 → 模块内部状态(_godMode, _moveSpeed, _lockX...)
CheatsRunner.Update()(每帧)→ Registry 各模块 ApplyPerFrame() → ReflectAccessor 写 UnitData/CharacterData
ItemBrowser "Add" 点击 → Inventory 模块 → ReflectAccessor 调 FakeInventoryData.AddItem/AddCoin
Disable All → Registry.DisableAll() → 各模块清锁 + 还原
```

`ICheatModule` 增补:`string Category { get; }`。是否需要新增 `ApplyPerFrame()`?——现有 `CheatsRunner` 已每帧调 GUI;每帧逻辑可放各模块私有、由 Runner 调一个新的 `OnUpdate()`。**决定**:`ICheatModule` 加 `void OnUpdate()`(默认空实现交给模块),Runner 每帧遍历调用。GodMode/PlayerStats 的锁写在 `OnUpdate`。

## 5. 错误处理

- 每个反射查找 null 检查;模块关键类型/成员缺失 → `Status=Broken`,面板该模块显 `(!)` 且不执行逻辑。
- `ReflectAccessor` get/set 包 try-catch,失败记一次 warning(去重,避免每帧刷屏)。
- `DisableAll` / 模块 `DisableAll()` 还原所有 lock 与改过的速度值(保存原值)。
- 存档备份已在 `Plugin.Load` 跑(`SaveBackup`,留 5 份)。改经验/等级前提示用户已备份。

## 6. 测试

xUnit(`src/LunHuiCheats.Tests`,游戏外可跑):
- `FilterSortTests`:过滤/排序纯函数,含空串、大小写、中文、稳定排序。
- `ReflectAccessorTests`:对测试替身类型(普通 C# class)读写字段/属性、缺失成员返回默认+不抛、缓存命中。
- `ItemBrowserModelTests`:物品列表模型的分类切换/搜索/排序输出。
- `ModuleRegistryTests`(已存在):扩展验证 `Category` 分组。

游戏内行为 → `docs/smoke-checklist.md` 新增条目:godmode 锁血、player 改攻/速、inventory 加量/加币/扩容、cultivation 改经验/等级/道心/灵根。

## 7. 范围与 YAGNI

**做**:分类/排序/搜索/滚动面板、四模块(Inventory 含一期+二期)、共用管线、纯逻辑单测。

**不做(砍)**:物品图标/Sprite、价格/简介列、uGUI 复刻(坚持 IMGUI 避开 IL2CPP uGUI 注入)、抄 trainer 的具体分类名(用游戏 `All*` 列表自带分类)、NPC 关系/技能/宠物等额外系统(数据已有,留后续计划)。

## 8. 风险

| 风险 | 应对 |
|---|---|
| `AddItem` 需构造 `BaseRewardData` | 一期只克隆已有项加量;二期试构造,失败降级 |
| `FindObjectOfType(CharacterData)` 性能/时机 | 缓存 + 帧节流懒刷新;OnGameReady 首解析 |
| 改经验/等级触发存档校验或损档 | 已自动备份;面板警示;先在废档测 |
| 游戏更新改字段名 | 名字反射 + 模块 Broken 降级;重扫 refs 即可修 |
| `SpiritRoot` 为嵌套类型 `DiscipleSpiritData+SpiritRoot` | `AccessTools.TypeByName` 用 `+` 嵌套全名;改字典项需先确认 `SpiritRootType` 枚举值 |

## 9. 开放问题(实现中解决,不阻塞)

- 当前玩家 `CharacterData` 是否唯一?多实例时如何挑(可能需按 `playerTransform`/tag 区分)。
- `FakeInventoryData` 实例归属(GameManager 还是 Player 上)—— 先 `FindObjectOfType`,不行再顺 `CharacterData` 找引用。
- 灵根 `spritRootDic` 的键类型与可写性 —— 二期前用 scanner 复核。
