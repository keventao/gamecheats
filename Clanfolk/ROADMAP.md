# Clanfolk Cheats Roadmap

Last updated: 2026-06-02

## Current Status

Status: v0.1.1+ — 8 模块。移速5x/心情锁/睡眠锁/瞬间工作 游戏内已验证；金钱/资源计数待冒烟确认数值；本地化已对接（196 物品）。

Game/runtime:

- Clanfolk (latest), Unity IL2CPP x64
- MelonLoader 0.7.x x64
- 游戏路径: `<CLANFOLK_GAME_ROOT>`（WSL 下）
- 游戏自带简体中文（重要：物品名/UI 本地化直接用游戏自己的翻译）

最近一次游戏内日志（2026-06-02 17:12）确认：
`ClanfolkCheats v0.1.1 loaded (6 modules)`，`[Char] OK`，`[Rsrc] Found 196 types`，
Build/God/Stor 三个补丁均成功 Patched，无我方异常。

## 模块状态（✅=游戏内已验证 / 🔧=补丁已落地待冒烟 / 🚧=占位）

| 模块 | 状态 | 说明 |
|---|---|---|
| Time 时间 | ✅ | x1/x2/x5/x10，`Time.timeScale`，无需补丁 |
| Resource 资源 | ✅ 发现/中文，🔧 计数/生成 | 196 种发现；物品名中文；快捷量 +10/+100；`xN`/总数计数已修（GetEntityCount/GetEntityTypeCount，每30帧刷新，待冒烟） |
| Character 角色 | ✅ 移速/心情/睡眠 | 移速5倍(postfix)；心情锁满(拦SetCurrentValue)；睡眠锁满不用睡(MaxCurrentValue)；均游戏内已验证 |
| Build 建造 | 🔧 | `[Build] Patched WorldObject resource counts + Recipe elapsed time` |
| GodMode 神模式 | 🔧 | `[God] Patched AttributeHealth.ChangeCurrentHealth` |
| Storage 存储 | 🔧 | `[Stor] Patched Item.GetMaxCount`，容量倍率 1/2/5/10x |
| Money 金钱 | 🔧 | `MoneyManager.GetMoney/ChangeMoney/SetMoney`；+100/+1k/+10k/清零/自定义；待冒烟 |
| Work 工作 | ✅ | 瞬间工作：prefix 6 个 Apply*Work 的 `deltaTime` ×100（砍伐/采集/开采/建造）；游戏内已验证 |

## 已实现功能细节

### Resource 资源
- 5 个可配置槽位 + 可搜索物品选择器。
- 物品发现：`ItemManager.GetLoadedPrefabArray`（196 种），多级回退。
- **本地化（已对接游戏翻译，见 refs/01）**：显示名顺序 =
  `Entity.GetEntityName()`（游戏中文名）> `TextBible.GetText(key)` >
  `GetEntityTypeToken(key)→GetText` > 硬编码 ZhNames > 子串 > 原 key。
- 生成：`SpawnEntitiesAtPosition` / `SpawnItem` / `SpawnEntity` 回退链。
- 快捷加按钮 **+10 / +100**（自定义数量默认 100，滚轮可调 1–9999）。
- 槽位锁定下限 UI 就绪；强制逻辑待资源查询 API。
- **计数（已修，见 refs/03）**：`xN`/总数原恒为 0 — 真因是旧代码把
  `GetAllEntityList()` 返回的 `Il2CppSystem.List<Entity>` 当 `System.Collections.IList`
  强转，永远 null、静默失败。改用 `EntityManager.GetEntityCount()`（总数）+
  `GetEntityTypeCount(key,NONE,false)`（每槽位），每 30 帧刷新，生成后即时更新。
  注：`isSpoiled=false` 不计腐坏食物，可后续补。

### Character 角色（refs/02）
- **锁循环真因（踩坑，已修）**：`UnitManager.humanList` 是 `Il2CppSystem List<ulong>`
  （存**实体ID**，非 Human 对象）。旧代码 `as System.Collections.IList` → 永远 null →
  foreach 跳过 → 心情/睡眠锁从没运行（编译过但运行时死）。
  修：反射 `Count`+`Item` 索引器迭代 ulong；再
  `GameManager.GetEntityManager(EntityClass.Unit).GetEntity(id)` 解析成 Unit。
  日志 `[Char] Lock loop: N humans` 确认。注：逐 unit 解析 `GetXxxAttribute`
  **只缓存非 null**（首个 unit 类型若缺该方法，缓存 null 会毒化后续全部 → 睡眠失效）。
- **睡眠锁满(不用睡) ✅已验证**：每 tick `GetSleepAttribute().MaxCurrentValue()`
  （current=max=36000）。`AttributeSleep` 不被每帧重算，直接设值即生效。
  值高=休息好，低=困（ShouldSleep/IsSleepy）。
- **心情锁满 ✅已验证（最难，三次返工）**：`AttributeMood` 的值由游戏每帧从 cap 拉回，
  设值（MaxCurrentValue/SetAttributeProgress/OnAttributeTicked postfix）全被覆盖；
  且 UI/逻辑**直接读 native 字段**，patch `GetCurrentValue()` getter **不触发**（无 native 调用）。
  **解法 = 拦写入**：Harmony prefix `AttributeGeneric.SetCurrentValue(int)`（mood 过滤）
  强制 `val=max(10000)`；prefix `ChangeCurrentValue(int)` 挡负数；
  循环每 tick `GetMoodAttribute().MaxCurrentValue()` 主动顶满。
  日志实证 `MoodSet fired: val=493 max=10000`（cap 拉回走 SetCurrentValue，被 prefix 改满）。
  教训：il2cpp 下「值被每帧重算 + native 直读字段」的属性，必须拦 **setter**，
  不能拦 getter（native 不走方法），也不能从 OnUpdate 写值（会被覆盖）。
- **移动速度5倍**（已验证 3倍→改 5）：Harmony postfix 补 `Unit.GetMoveSpeed()`，`__result *= 5`，
  由 `OnUpdate` 同步静态 `_sSpeedMult`（开 5f/关 1f）。`SpeedMultiplier` 常量可调。
  （注：先前直接写 `Unit.unitSpeedMult` 字段无效，已弃用。）
- 生命锁已删除（游戏战斗少，用户要求换成移速）。
- 停止衰老：仍占位（需成长/年龄属性）。

### Money 金钱（refs/04，新模块）
- `GameManager.GetMoneyManager()`（static）→ `MoneyManager`。
- `GetMoney()` 显示当前钱；`ChangeMoney(delta)` 加减；`SetMoney(count,false)` 设绝对值。
- UI：+100/+1000/+10000、清零、自定义金额（滚轮步进 1000）增加/设为。
- 第 7 模块，已注册 `Plugin.Awake()`。

### Time / Build / GodMode / Storage
- Time：`Time.timeScale`。
- Build/God/Stor：见上表，补丁在 `Register()` 内 `harmony.Patch` 落地。

## 开发环境与关键经验（新会话必读）

### 构建 / 安装
```bash
export DOTNET_ROOT=$HOME/.dotnet
G="<CLANFOLK_GAME_ROOT>"; P=$HOME/gamecheats/Clanfolk
$HOME/.dotnet/dotnet build "$P/src/ClanfolkCheats/ClanfolkCheats.csproj" -c Release /p:GameRoot="$G"
cp -f "$P/src/ClanfolkCheats/bin/Release/net6.0/ClanfolkCheats.dll" "$G/Mods/ClanfolkCheats.dll"
```
- **游戏运行时 DLL 被锁**：`Mods/ClanfolkCheats.dll` 在游戏开着时无法覆盖
  （`Access denied` / `Input/output error`）。必须先完全退出游戏再装。
  可后台轮询 `cp` 直到解锁自动装好。

### 反编译（验证游戏 API，切勿猜名字）
```bash
dotnet tool install -g ilspycmd --version 8.2.0.7535   # 一次性
export DOTNET_ROOT=$HOME/.dotnet PATH="$PATH:$HOME/.dotnet/tools" DOTNET_ROLL_FORWARD=LatestMajor
DLL="$G/MelonLoader/Il2CppAssemblies/Assembly-CSharp.dll"
ilspycmd -t Il2Cpp.<TypeName> "$DLL"        # 按全名（命名空间 Il2Cpp）反编译单类型
```
- 只装了 net8 runtime，靠 `DOTNET_ROLL_FORWARD=LatestMajor` 跑 ilspycmd(net6)。
- 类型全名带 `Il2Cpp.` 前缀；`--list-types` 会崩，改用 `-t` 按名取。
- 字符串侦察：`strings -n6 "$DLL" | grep -i <keyword>`。

### IL2Cpp 反射坑（踩过）
- **native 字段经 Il2CppInterop 暴露为 C# 属性**，不是字段。
  `AccessTools.Field(...)` 取 `unitSpeedMult`/`humanList`/`myEntityAttributes` 会返回 **null**，
  必须用 `AccessTools.Property`。
- 虚方法（如 `GetEntityName`、`GetMoveSpeed`）可正常反射调用 / Harmony patch。
- Harmony 扫描 UnityEngine 时刷的 `ControlOptions/CountOptions TypeLoadException` 是无害噪音。

### 关键游戏 API（已验证，详见 refs/）
- 本地化：`GameManager.GetTextBible()` → `TextBible.GetText(key, GrowthStage.NONE)`；
  `Entity.GetEntityName()`（虚，返回当前语言名）；`EntityManager.GetEntityTypeToken(key)→string[]`。
- 类层次：`Human : Unit : Entity`；`UnitManager.humanList`（属性）。
- 属性：`Unit.Get{Mood,Sleep,Food,Water,Social,Satisfaction,Growth}Attribute()`；
  基类 `AttributeGeneric.SetAttributeProgress(float)` / `SetFloorAttributeProgress` / `MaxCurrentValue`。
- 速度：`Unit.GetMoveSpeed()`（postfix 目标）、字段 `unitSpeedMult`/`moveSpeed`（写无效/会被重算）。

## 待冒烟测试（游戏内确认）
- [ ] 移动速度3倍：日志 `[Char] Patched Unit.GetMoveSpeed`；角色页开关 → 小人快 3 倍。
- [ ] 心情锁满：心情条满且不掉。
- [ ] Resource 选择器：196 种物品名显示中文。
- [ ] Build/God/Storage 补丁的实际游戏内效果。
- [ ] 物品 `xN`/总数计数（已修，见 refs/03）：表头“件物品”非 0；槽位 `xN` 反映场上库存；生成后即时增长。
- [x] 睡眠锁满 ✅：`MaxCurrentValue` 设满 36000，不睡。
- [x] 心情锁满 ✅：拦 `SetCurrentValue` 强制 10000，不掉。
- [x] 移速 5 倍 ✅。
- [ ] 金钱：日志 `[Money] OK`；+100/+1k/+10k/清零/设为均改变当前金钱显示。
- [x] 瞬间工作 ✅：日志 `[Work] Patched 6 work-apply method(s)`；开关 → 砍树/采集/开采/建造秒完成。

## Next Work
1. 完成上面冒烟项，逐个把 🔧 转 ✅。
2. ~~修 Resource 库存计数（`xN` 显示 0）~~ ✅ 已改 GetEntityCount/GetEntityTypeCount（refs/03），待冒烟。
3. 停止衰老：研究 `GetGrowthAttribute()` / 年龄字段。
4. 资源锁定下限的 `OnUpdate` 强制逻辑。
5. 视需要给 GodMode/Build 增加更多开关（无饥饿/无寒冷等）。

## 已知风险
- Clanfolk 更新可能改 IL2CPP 代理名/签名 → 需重新反编译核对。
- 直接写运行期字段（如 unitSpeedMult）常被游戏每帧重算覆盖，优先用 Harmony postfix 补 getter。

## 版本历史

### 2026-06-02（瞬间工作）
- 新增 Work 模块（第 8 个）✅已验证：prefix `Node.ApplyNode{Harvest,Extraction}Work` /
  `ClearNodeForHarvest` + `WorldObject.Apply{Object,}HarvestWork` / `ApplyStateWork`
  共 6 个方法的 `deltaTime` ×100（按参数名注入）→ 砍伐/采集/开采/建造秒完成。refs/05。
- 弯路：`Unit.GetAppliedWorkTime` postfix ×100 无效（非进度累加点），已弃。

### 2026-06-02（金钱+睡眠+心情+移速5x，全部游戏内验证）
- 新增 Money 模块（第 7 个）：MoneyManager.GetMoney/ChangeMoney/SetMoney，refs/04。
- Character：移速 3→5 倍 ✅；睡眠锁满(不用睡) `MaxCurrentValue` ✅；
  心情锁满（拦 `SetCurrentValue` 强制 max，绕过 cap 重算+native 直读字段）✅。
- 修锁循环真因：humanList 是 List<ulong> 实体ID，需索引器迭代 + GetEntity(id) 解析；
  GetXxxAttribute 只缓存非 null（修睡眠毒化）。

### 2026-06-02（计数修复）
- Resource `xN`/总数计数修复：弃 `GetAllEntityList`（Il2Cpp 列表强转 IList 恒 null），
  改 `GetEntityCount`/`GetEntityTypeCount`，每 30 帧刷新。记录 refs/03。已装，待冒烟。

### 2026-06-02（本次会话）
- Resource 接入游戏本地化（GetEntityName/GetText/token 三级），物品名中文化；快捷量改 +10/+100。
- Character：心情锁真实现；删生命锁、加移动速度3倍（先字段写无效 → 改 GetMoveSpeed postfix）。
- 建立反编译工作流（ilspycmd + net8 roll-forward），记录到 refs/01、refs/02。
- 游戏内验证：模组加载、6 模块、Char 初始化、Rsrc 发现 196 种。

### v0.1.1
- ReflectAccessor / SaveBackup / ModConfig；ResourceCheats 增强；GameRefs 真实检测；xUnit 13 通过。

### v0.1.0
- 初始骨架：6 标签（Time 可用，5 占位）、自绘 IMGUI、测试工程。

## Project Links
- `README.md` - 安装 / 开发 / IL2CPP 说明。
- `docs/smoke-checklist.md` - 手动冒烟清单。
- `refs/00-research-checklist.md` - 研究清单。
- `refs/01-localization.md` - 本地化 / 物品名 API（已验证）。
- `refs/02-character-attributes.md` - 角色属性 / 移动速度 API（已验证）。
- `refs/03-entity-counts.md` - 物品库存计数 API（GetEntityCount/GetEntityTypeCount，已验证）。
- `refs/04-money.md` - 金钱 API（MoneyManager.GetMoney/ChangeMoney/SetMoney，已验证）。
- `refs/05-work.md` - 瞬间工作 API（Unit.GetAppliedWorkTime postfix ×倍率，已验证）。
