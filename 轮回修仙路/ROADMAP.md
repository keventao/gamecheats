# 轮回修仙路 Cheats Roadmap

Last updated: 2026-05-31

## Current Status

Status: **v0.0.2 — reverse-engineering phase complete, handoff ready.**

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

### Phase 1 — Runtime Modules (ready to implement)

Priority order:

1. **PlayerStats** (`player`) — `CharacterData` -> `UnitData`
   - 修改: `curHp`, `maxHp`, `curPhysicalAttacks`, `curSpellAttacks`, `MoveSpeed`, `bigWorldFlySpeed`
   - 技术: AccessTools 反射读写属性

2. **GodMode** (`godmode`) — 最简单
   - Patch `UnitData.curHp` setter 或每帧恢复 HP

3. **Inventory** (`inventory`) — `FakeInventoryData`
   - 反射调用 `AddItem(BaseRewardData, Int32)`, `AddCoin(CoinData, Int32)`
   - 修改 `size`
   - 难点: 需先找到 `FakeInventoryData` 实例

4. **Cultivation** (`cultivation`)
   - 直接修改 `CharacterData.currentExp`, `currentLevel`
   - 灵根: `UnitData.discipleSpiritData` -> `SpiritRoot`
   - 道心: `CharacterData.curDaoxin`

5. **Time scale module** — 已有 placeholder，已加 value-change guard

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
