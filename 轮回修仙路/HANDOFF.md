# 轮回修仙路 — Handoff (v0.0.2)

> 上一阶段完成：反向工程 + 框架强化。下一阶段：实现作弊模块。

## 环境

- Steam AppID 1993150
- Unity IL2CPP (GameAssembly.dll present)
- BepInEx 6 IL2CPP 已装到 `<STEAM>\steamapps\common\轮回修仙路`
- 我们的 mod (`LunHuiCheats`) 已验证能在游戏中加载

## 已发现的运行时类型（在真实游戏中验证过）

类型名通过 `AccessTools.TypeByName("xxx")` 即可获取。

| 类型 | 全名 | 关键字段/方法 | 模块用途 |
|---|---|---|---|
| `UnitData` | `DataLib.UnitData` | `curHp`, `maxHp`, `curPhysicalAttacks`, `curSpellAttacks`, `MoveSpeed`, `bigWorldFlySpeed`, `fightSpeed`, `characterBaseAttributes` | PlayerStats / GodMode |
| `CharacterData` | `CharacterData` | `unitData`, `currentExp`, `currentLevel`, `curDaoxin`, `forgeLevel`, `danYaoData`, `haveAcquiredProp` | Cultivation / Player |
| `FakeInventoryData` | `FakeInventoryData` | `AddItem(BaseRewardData, Int32)`, `AddCoin(CoinData, Int32)`, `Clear()`, `size` | Inventory |
| `CharacterBaseAttributesData` | `Configuration.CharacterBaseAttributesData` | `Hp`, `Attack`, `S_Attack`, `Defense`, `S_Defense`, `CriticalHit`, `Dodge`, `Hit`, `Speed` | 配置参考 |
| `SpiritRoot` | `DiscipleSpiritData+SpiritRoot` | `mainSpritRootDic`, `spritRootDic`, `GetSpiritRootValue(SpiritRootType)` | Cultivation |
| `ExperienceData` | `DataLib.ExperienceData` | `year`, `month`, `day`, `maxAge`, `minAge`, `quality`, `talent` | 经历/年龄 |
| `RoleUpgradeData` | `Configuration.RoleUpgradeData` | `level`, `needExp`, `hp`, `mp`, `attack`, `defense` | 升级配置 |
| `SkillData` | `DataLib.SkillData` | 80 个属性 | 技能 |
| `DanYaoData` | `DanYaoData` | 30 个属性 | 丹药 |
| `PetData` | `DataLib.PetData` | 129 个属性 | 宠物 |
| `HeartAchievementMethod` | `HeartAchievementMethod` | 47 个属性 | 心法/功法 |
| `CoinData` | `CoinData` | 25 个属性 | 货币 |
| `SpiritStoneData` | `SpiritStoneData` | 25 个属性 | 灵石 |

**不存在的类型**: `PlayerUnitData`, `BackpackGoods`, `Cultivation`, `Practice`, `LifeTime`, `Linggen`, `RefiningDanData`

## 模块实现优先级

1. **GodMode** (`godmode`) — 最简单
   - 目标: `UnitData.curHp`
   - 方法: Patch HP setter 或每帧恢复

2. **PlayerStats** (`player`)
   - 目标: `CharacterData` -> `UnitData`
   - 方法: 反射读写 `curHp`, `curPhysicalAttacks`, `curSpellAttacks`, `MoveSpeed`, `bigWorldFlySpeed`

3. **Inventory** (`inventory`)
   - 目标: `FakeInventoryData`
   - 方法: 反射调用 `AddItem`, `AddCoin`, 修改 `size`
   - 难点: 需先找到 `FakeInventoryData` 实例（可能在 GameManager / Player 上）

4. **Cultivation** (`cultivation`)
   - 目标: `CharacterData.currentExp`, `currentLevel`, `curDaoxin`
   - 灵根: `UnitData.discipleSpiritData` -> `SpiritRoot`

5. **TimeCheats** — 已有 placeholder (`Modules/TimeCheats.cs`)，已加 value-change guard

## 框架现状

- `Plugin.cs`：`Registry.Add(new Modules.XxxCheats())` 即可注册新模块
- `AttachRunnerToGameHost()`：自动探测 8 个常见宿主类型名，fallback 到独立 GameObject
- `BootstrapHooks.Register()`：如果 `SceneController` 不存在则立即 fallback attach
- `GameRefs.FindByType<T>(string)` 已 public，方便模块运行时查找对象
- `DebugDiagnostics` 模块：GUI 中可手动触发 TypeScanner / FieldScanner

## 参考文件

- `refs/01-discovered-types-summary.md` — 详细字段表（含属性类型、方法签名）
- `refs/lunhui-fieldscan.txt` — 完整字段扫描原始输出 (1,400 行)
- `refs/lunhui-typescan.txt` — 完整类型扫描原始输出 (19,395 行)
- `tools/try-trainer-current.ps1` — 如需参考第三方 trainer 面板

## 开发流程

```powershell
cd "<STEAM>\steamapps\common\轮回修仙路"
# 修改代码...
dotnet build -c Release
powershell tools\install.ps1
# 启动游戏测试
```

## 风险

- `FakeInventoryData.AddItem` 可能需要有效的 `BaseRewardData` 实例
- 运行时修改经验/等级系统可能触发存档校验，先备份
- 反射失败时 `AccessTools` 返回 null，需做空检查
