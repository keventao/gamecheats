# Clanfolk Cheats Roadmap

Last updated: 2026-06-02

## Current Status

Status: v0.1.1+ — 真实 Harmony 补丁已落地多模块，游戏本地化已对接，物品发现在游戏内验证成功（196 种）。

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
| Resource 资源 | ✅ 发现/中文，🔧 生成 | 196 种物品发现成功；物品名走游戏本地化显示中文；快捷量 +10 / +100 |
| Character 角色 | 🔧 | 心情锁满（真实现）、移动速度3倍（postfix）；待游戏内确认 |
| Build 建造 | 🔧 | `[Build] Patched WorldObject resource counts + Recipe elapsed time` |
| GodMode 神模式 | 🔧 | `[God] Patched AttributeHealth.ChangeCurrentHealth` |
| Storage 存储 | 🔧 | `[Stor] Patched Item.GetMaxCount`，容量倍率 1/2/5/10x |

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
- ⚠️ 已知：发现时 `total items = 0`，选择器显示 `xN` 计数为 0（库存计数未匹配到场上实体，仅显示问题，不影响生成）。

### Character 角色（refs/02）
- **心情锁满**：每 tick `unit.GetMoodAttribute().SetAttributeProgress(1f)`
  （`AttributeMood : AttributeGeneric`）。
- **移动速度3倍**：Harmony postfix 补 `Unit.GetMoveSpeed()`，`__result *= 3`，
  由 `OnUpdate` 同步静态 `_sSpeedMult`（开 3f/关 1f）。
  （注：先前直接写 `Unit.unitSpeedMult` 字段无效，已弃用。）
- 生命锁已删除（游戏战斗少，用户要求换成移速）。
- 停止衰老：仍占位（需成长/年龄属性）。

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
- [ ] 物品 `xN` 计数为 0 的修复（可选）。

## Next Work
1. 完成上面冒烟项，逐个把 🔧 转 ✅。
2. 修 Resource 库存计数（`xN` 显示 0）：发现期扫场上实体匹配 entityType。
3. 停止衰老：研究 `GetGrowthAttribute()` / 年龄字段。
4. 资源锁定下限的 `OnUpdate` 强制逻辑。
5. 视需要给 GodMode/Build 增加更多开关（无饥饿/无寒冷等）。

## 已知风险
- Clanfolk 更新可能改 IL2CPP 代理名/签名 → 需重新反编译核对。
- 直接写运行期字段（如 unitSpeedMult）常被游戏每帧重算覆盖，优先用 Harmony postfix 补 getter。

## 版本历史

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
