# 轮回修仙路 — 已验证游戏类型速查 (v2026-05-31)

> 来源：BepInEx 6 IL2CPP 运行时 FieldScanner + TypeScanner，在真实游戏环境中验证。
> 游戏版本：Unity IL2CPP，Steam AppID 1993150。

---

## 关键发现

以下类型在 `FieldScanner` 中**成功扫描**，字段名和属性可直接用于 `AccessTools` 反射读写。

### 1. UnitData — 战斗/基础属性

- **全名**: `DataLib.UnitData`
- **基类**: `DataLib.BaseData`
- **获取方式**: 需要先拿到 `CharacterData`，再通过 `characterData.unitData`

| 字段/属性 | 类型 | 含义 |
|---|---|---|
| `curHp` | `Int64` | 当前生命 |
| `maxHp` | `Int64` | 最大生命 |
| `HP` | `Int64` | 生命（同curHp？需确认） |
| `curPhysicalAttacks` | `Int64` | 当前物攻 |
| `curSpellAttacks` | `Int64` | 当前法攻 |
| `curPhysicalDefense` | `Int64` | 当前物防 |
| `curSpellDefense` | `Int64` | 当前法防 |
| `MoveSpeed` | `Single` | 移动速度 |
| `bigWorldFlySpeed` | `Int32` | 大世界飞行速度 |
| `fightSpeed` | `Single` | 战斗速度 |
| `attackTime` | `Single` | 攻击间隔 |
| `weakness` | `Single` | 弱点？待确认 |
| `cloudZoom` | `Single` | 云缩放？待确认 |
| `quality` | `Int32` | 品质 |
| `Sex` | `Sex` (enum) | 性别 |
| `skillDatas` | `List<SkillData>` | 技能列表 |
| `achievementMethods` | `List` | 功法列表 |
| `apparelEquips` | `List` | 装备列表 |
| `characterBaseAttributes` | `CharacterBaseAttributesData` | 基础属性配置数据 |
| `breakData` | `BreakData` | 突破数据 |
| `bornLifeDay` | `Int64` | 出生日期/天数？ |
| `discipleSpiritData` | `DiscipleSpiritData` | 弟子灵体数据？ |
| `faceModelSaveData` | `FaceModelSaveData` | 捏脸数据 |
| `sectData` | `SectData` | 宗门数据 |
| `substractLifetimePecent` | `Single` | 寿命衰减百分比？ |
| `subtablesRoleUpgradeData` | `SubtablesRoleUpgradeData` | 角色升级数据？ |

### 2. CharacterData — 玩家角色

- **全名**: `CharacterData`
- **获取方式**: `FindObjectOfType<CharacterData>()` 或从 GameManager 中获取
- **包含**: `unitData` 属性指向 `UnitData`

| 字段/属性 | 类型 | 含义 |
|---|---|---|
| `unitData` | `UnitData` | 上面的战斗属性 |
| `currentExp` | `Int64` | 当前经验 |
| `currentLevel` | `Int32` | 当前等级 |
| `forgeLevel` | `Int16` | 炼器等级？ |
| `curDaoxin` | `Int32` | 道心 |
| `danYaoData` | `SerializationDictionary` | 丹药数据 |
| `haveAcquiredProp` | `SerializationDictionary` | 已获得道具？ |
| `monsterBaseLevel` | `SerializationDictionary` | 怪物基础等级？ |
| `taskSaveDataDir` | `SerializationDictionary` | 任务存档数据 |
| `worldId` | `Int32` | 世界ID |
| `enterWolrdId` | `Int32` | 进入的世界ID |
| `mineWorldDataIndex` | `Int32` | 矿场世界数据索引？ |
| `lastPos` / `pos` / `backPos` | `Vector3` | 位置相关 |
| `row` / `col` | `Int32` | 行列坐标？ |
| `tile` | `Tile` | 地块 |
| `isScanOpen` | `Boolean` | 扫描开启？ |
| `playerTransform` | `Transform` | 玩家Transform |
| `NpcTeams` | `List` | NPC队伍 |
| `interactionTimeData` | `InteractionTimeData` | 交互时间数据 |
| `weGameAchievementSyncUtcTicks` / `weGameAchievementSyncVersion` |  | WeGame成就同步 |

### 3. CharacterBaseAttributesData — 基础属性表 (配置数据)

- **全名**: `Configuration.CharacterBaseAttributesData`
- **基类**: `IConfig`
- **说明**: 配置表中的属性，不是运行时值

| 字段/属性 | 类型 | 含义 |
|---|---|---|
| `Hp` | `Int64` | 生命值 |
| `Hp_grop` | `Single` | 生命成长 |
| `Attack` | `Int64` | 攻击 |
| `attack_Deepen` | `Int32` | 攻击加深？ |
| `attack_Reduction` | `Int32` | 攻击减免？ |
| `Attack_grop` | `Single` | 攻击成长 |
| `S_Attack` | `Int64` | 法术攻击 |
| `S_Attack_grop` | `Single` | 法术攻击成长 |
| `Defense` | `Int64` | 防御 |
| `Defense_grop` | `Single` | 防御成长 |
| `S_Defense` | `Int64` | 法术防御 |
| `S_Defense_grop` | `Single` | 法术防御成长 |
| `CriticalHit` | `Int32` | 暴击 |
| `CriticalHit_Deepen` | `Int32` | 暴伤加深？ |
| `CriticalHit_Reduction` | `Int32` | 暴伤减免？ |
| `CriticalHit_Resistance` | `Int32` | 暴击抵抗？ |
| `Dodge` | `Int32` | 闪避 |
| `Hit` | `Int32` | 命中 |
| `Speed` | `Single` | 速度 |
| `Speed_ratio` | `Single` | 速度比率 |
| `AttackSpeed` | `Single` | 攻击速度 |
| `distance` | `Single` | 距离/射程 |
| `useLevel` | `Int32` | 使用等级 |
| `quality` | `Int32` | 品质 |
| `job` | `Int32` | 职业 |
| `mode` | `Int32` | 模式/模型？ |
| `camp` | `Int32` | 阵营 |
| `DiscipleSoul` | `String` | 弟子魂？ |
| `Nomal_skill` | `Int32` | 普通技能 |
| `pet_skill` | `String` | 宠物技能 |
| `weapon_show` | `String` | 武器展示 |
| `ModleMan` / `ModleWomen` | `String` | 男女模型 |
| `icon` | `Int32` | 图标 |
| `name` | `String` | 名称 |
| `Info` | `String` | 信息/描述 |

### 4. FakeInventoryData — 背包/仓库

- **全名**: `FakeInventoryData`
- **说明**: 包含多个分类列表和方法

| 字段/属性 | 类型 | 含义 |
|---|---|---|
| `AllCoins` | `List` | 所有货币 |
| `AllEquips` | `List` | 所有装备 |
| `AllDanYao` | `List` | 所有丹药 |
| `AllMaterials` | `List` | 所有材料 |
| `AllPets` | `List` | 所有宠物 |
| `AllUseItem` | `List` | 所有消耗品 |
| `AllSpirites` | `List` | 所有魂魄/精灵？ |
| `AllInvObjects` | `List` | 所有背包对象 |
| `AllAchievementMethods` | `List` | 所有功法 |
| `AllHeartAchievementMethods` | `List` | 所有心法 |
| `AllPetAchievementMethods` | `List` | 所有宠物功法 |
| `AllCreateMaterials` | `List` | 所有制作材料 |
| `AllFixedTools` | `List` | 所有固定工具？ |
| `AllFlyTalisman` | `List` | 所有飞行符 |
| `AllSeedMaterials` | `List` | 所有种子材料 |
| `AllEDUProps` | `List` | 所有教育/培养道具？ |
| `size` | `Int32` | 背包大小/容量 |

| 方法 | 参数 | 含义 |
|---|---|---|
| `AddItem` | `BaseRewardData, Int32` | 添加物品 |
| `AddItem` | `List` | 批量添加物品 |
| `AddCoin` | `CoinData, Int32` | 添加货币 |
| `Clear` | | 清空背包 |
| `ClearUp` | | 整理背包 |
| `ClearUp` | `List` | 整理指定列表 |
| `AddAmount` | `BaseRewardData, List, Int32` | 增加数量 |

### 5. SpiritRoot — 灵根

- **全名**: `DiscipleSpiritData+SpiritRoot` (嵌套类)
- **字段**:
  - `mainSpritRootDic` — 主灵根字典
  - `spritRootDic` — 灵根字典
  - `addSpriteRootDic` — 附加灵根字典
- **方法**:
  - `GetSpiritRootValue(SpiritRootType)` — 获取指定类型灵根值
  - `GetMainSpritType()` — 获取主灵根类型

### 6. ExperienceData — 经历/人生事件

- **全名**: `DataLib.ExperienceData`
- **基类**: `ExperienceBaseData`
- **说明**: 用于经历系统，不是修为/经验

| 字段 | 类型 | 含义 |
|---|---|---|
| `year` | `Int32` | 年 |
| `month` | `Int32` | 月 |
| `day` | `Int32` | 日 |
| `maxAge` | `Int32` | 最大年龄 |
| `minAge` | `Int32` | 最小年龄 |
| `experienceType` | `Int32` | 经历类型 |
| `quality` | `Int32` | 品质 |
| `talent` | `Int32` | 天赋 |
| `sex` | `Int32` | 性别要求 |
| `destinyName` | `String` | 命运名称 |
| `info` | `String` | 描述 |
| `use` / `useInfo` | `String` | 使用条件/信息 |
| `reward` | `Int32` | 奖励ID？ |

### 7. RoleUpgradeData — 角色升级

- **全名**: `Configuration.RoleUpgradeData`

| 字段 | 类型 |
|---|---|
| `id` | `Int32` |
| `level` | `Int32` |
| `needExp` | `Int64` |
| `needCoin` | `Int32` |
| `hp` | `Int64` |
| `mp` | `Int64` |
| `attack` | `Int64` |
| `defense` | `Int64` |
| `spellAttack` | `Int64` |
| `spellDefense` | `Int64` |
| `speed` | `Single` |
| `hit` | `Int32` |
| `dodge` | `Int32` |
| `criticalHit` | `Int32` |
| `criticalHitResistance` | `Int32` |
| `quality` | `Int32` |

### 8. 其他已确认类型

| 类型名 | 全名 | 说明 |
|---|---|---|
| `CoinData` | `CoinData` | 货币数据，25个属性 |
| `SpiritStoneData` | `SpiritStoneData` | 灵石数据，25个属性 |
| `SkillData` | `DataLib.SkillData` | 技能数据，80个属性 |
| `SkillStateMachine` | `MySkillLib.SkillStateMachine` | 技能状态机，31个属性 |
| `BaseRewardData` | `BaseRewardData` | 基础奖励数据，24个属性 |
| `DanYaoData` | `DanYaoData` | 丹药数据，30个属性 |
| `PetData` | `DataLib.PetData` | 宠物数据，129个属性 |
| `HeartAchievementMethod` | `HeartAchievementMethod` | 心法/功法，47个属性 |
| `JindanData` | `Configuration.JindanData` | 金丹配置，12个属性 |

---

## 查找失败的类型 (NOT FOUND)

这些类型在目标类型列表中但扫描时未找到，可能名字不对或已被移除：

| 类型名 | 猜测 |
|---|---|
| `PlayerUnitData` | 可能不存在，直接用 `UnitData` |
| `BackpackGoods` | 可能是 `FakeInventoryData` 内部类或别名 |
| `Cultivation` | 修为类，可能叫 `PracticeData` 或不存在单独类型 |
| `Practice` | 修炼类，可能与 `Cultivation` 是同一个 |
| `LifeTime` | 寿命类，可能叫 `LifeSpan` 或在 `UnitData` 中 |
| `Linggen` | 灵根，实际扫描到的是 `SpiritRoot` |
| `RefiningDanData` | 炼丹数据，可能不存在单独类型 |

---

## 下一步建议 (给下一台机器的 Claude Code)

### 模块实现优先级

1. **PlayerStats** (`player`)
   - 目标: `CharacterData` -> `UnitData`
   - 功能: 无限HP (lock `curHp` = `maxHp`), 修改 `curPhysicalAttacks`, `curSpellAttacks`, 修改 `MoveSpeed`, `bigWorldFlySpeed`
   - 技术: 用 `AccessTools.Property` 读写，或 Harmony patch `get_curHp`

2. **Inventory** (`inventory`)
   - 目标: `FakeInventoryData`
   - 功能: 添加物品 (反射调用 `AddItem`), 添加货币 (`AddCoin`), 修改 `size`
   - 注意: 需要先找到 `FakeInventoryData` 的实例，可能在 GameManager 或 Player 上

3. **Cultivation** (`cultivation`)
   - 目标: 经验系统 — 直接修改 `CharacterData.currentExp`, `currentLevel`
   - 灵根: `UnitData.discipleSpiritData` -> `SpiritRoot`
   - 道心: `CharacterData.curDaoxin`
   - 寿命: 查找 `LifeTime` 替代方案或 `bornLifeDay` 相关计算

4. **GodMode** (`godmode`)
   - 目标: `UnitData.curHp` / `maxHp`
   - 最简单: patch `UnitData` 的 HP setter，受伤时恢复

### 如何找到游戏单例

当前 `BootstrapHooks` 尝试 hook `SceneController.Start`。如果没有这个类，需要尝试：
- `GameManager` (常见)
- `GameMain`
- `PlayerManager`
- `WorldManager`
- 任何继承自 `MonoBehaviour` 且名字包含 `Manager`, `Controller`, `Main` 的类型

用 `FindObjectOfType` 遍历所有 `MonoBehaviour` 也是一种方案（但性能差）。

### 参考文件

- `refs/lunhui-fieldscan.txt` — 完整字段扫描 (1400行)
- `refs/lunhui-typescan.txt` — 完整类型扫描 (19395行)
- `refs/00-research-checklist.md` — 研究清单模板
