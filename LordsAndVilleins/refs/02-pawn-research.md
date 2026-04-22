# 02 — Pawn 调研

> 调研于 2026-04-22 (ilspycmd 10.0.0.8330)

## 搜词清单

`pawn`, `villager`, `villein`, `lord`, `peasant`, `character`, `npc`,
`inhabitant`, `resident`, `worker`, `population`, `colonist`

## 关键类型

- **村民/领主类**:`WorldNPC`（全局命名空间，非 MonoBehaviour，存于 `NPCManager.instance.worldNPCDB`）
  - `WorldNPC` 包含：HP、skills（技能）、needs（需求/饥饿/心情）、inventory、组织归属
  - 对应的物理实体（场景中）：`ActiveNPC`（`ActiveNPC.worldNPC` 引用 `WorldNPC`）
  - 基础数据（名字/统计/关系）：`BaseNPC`（`WorldNPC.baseNPC` 引用）

- **村民集合**(从 PlayerManager 拿)路径:`PlayerManager.instance.GetWorldRulingOrganization().GetFamilyMembersAsReference()`
  - 数据结构：`List<WorldNPC>`（`WorldOrganization.familyMembers` 私有字段，通过 `GetFamilyMembersAsReference()` 暴露）
  - 所有 WorldNPC（含非玩家家族）：`NPCManager.instance.worldNPCDB`（`WorldNPCDB`，可迭代）

## 状态字段

| 字段 | 实际名 | 类型 | "好的"方向 |
|---|---|---|---|
| 饥饿（吃饱度） | `AIAgent.needs[AgentNeedName.Eat].GetValue()` | `float 0–1` | 大（1=完全饱，0=极度饥饿）|
| 健康（HP） | `WorldNPC.HP` : `public float HP` | `float 0–maxHP`（maxHP 通常为 1.0f）| 大（1=满血）|
| 心情 | `ActiveNPC.baseMood` : `public float baseMood` | `float` | 大（更高=更快乐）|
|  | `ActiveNPC.baseHappiness` : `public float baseHappiness` | `float` | 大 |

> **饥饿说明**：`AgentNeedName.Eat` 的 `needBufferValue`（0–1）表示满足度，1=完全满足（不饿），0=极度饥饿。设为 1.0 即"不饿"。
> **健康说明**：`WorldNPC.HP` 范围 [0, maxHP]，maxHP 通常初始化为 1.0f。设为 maxHP 即"满血"。
> **心情说明**：`baseMood`/`baseHappiness` 由各 need 的 mood impact 叠加而来；直接设置需同时修改 needs。
> 获取 WorldNPC 的 AIAgent（以访问 needs）：`worldNPC.activeNPC.AIAgent`（需要 activeNPC 不为 null）

## 技能字段

- 字段路径：`WorldNPC.aquiredSkills`（已习得技能集合） 和 `WorldNPC.skillsInProgress`（进行中技能）
  - 已掌握技能：`public HashSet<SkillName> aquiredSkills`
  - 进行中技能（含进度百分比）：`public Dictionary<SkillName, float> skillsInProgress`
- 数据结构：`HashSet<SkillName>`（已习得）+ `Dictionary<SkillName, float>`（进度 0–1）
- 技能类型枚举 `SkillName`（`Skills` 命名空间）：
  - `Cooking`, `Building`, `Reading`, `Writing`, `Woodcutting`, `Consecration`, `HorseRiding`, `SwordFighting`, `Archery`, `SpearFighting`, `AxeFighting`, `FistFighting`, `WarVeteran`
- **没有 level 整数值**：技能系统是二进制"已习得/未习得"，通过 `aquiredSkills.Contains(SkillName.X)` 判断
- 满值：将技能名加入 `aquiredSkills` 即为"已掌握"；`skillsInProgress[skillName] = 1.0f` 表示学习进度100%
- 注意：直接写 `aquiredSkills.Add(SkillName.X)` 即可解锁技能（无需 cap 检查）

## 反编译片段

```csharp
// WorldNPC.cs — 字段定义（关键部分）
public class WorldNPC : ISerializable, IInventoryOwner, ...
{
    public float maxHP = 1f;
    public float HP = 1f;
    public HashSet<SkillName> aquiredSkills;
    public Dictionary<SkillName, float> skillsInProgress;
    public Dictionary<NPCProfessionName, NPCProfession> professions;
    public Inventory inventory;
    public DebuffModule<AgentDebuffType> debuffModule;
    public DebuffModule<AgentHealthConditionDebuffType> healthConditionDebuffModule;
}

// ActiveNPC.cs — 心情字段
public class ActiveNPC : IAIAgentType, ...
{
    public float baseMood;
    public float baseHappiness;
}

// AgentNeed.cs — 饥饿/需求访问
public class AgentNeed
{
    private float needBufferValue; // 0–1，1=满足，0=极度匮乏
    public void SetValue(float value) { needBufferValue = value; }
    public float GetValue() { return needBufferValue; }
}

// AgentNeedName enum
public enum AgentNeedName
{
    Eat = 1, Sleep = 2, Comfort = 4, Procreate = 8, Social = 0x10,
    StableTemperature = 0x20, Beauty = 0x40, SelfExpression = 0x80,
    Status = 0x100, PlayerReputation = 0x200, FairPayment = 0x400
}

// SkillName enum (Skills namespace)
public enum SkillName
{
    Undefined, Cooking, Building, Reading, Writing, Woodcutting,
    Consecration, HorseRiding, SwordFighting, Archery, SpearFighting,
    AxeFighting, FistFighting, WarVeteran
}

// 遍历玩家家族成员
foreach (WorldNPC npc in PlayerManager.instance.GetWorldRulingOrganization().GetFamilyMembersAsReference())
{
    // 满血
    npc.HP = npc.maxHP;
    // 解锁所有技能
    foreach (SkillName skill in Enum.GetValues(typeof(SkillName)))
        if (skill != SkillName.Undefined) npc.aquiredSkills.Add(skill);
    // 不饿（通过 ActiveNPC → AIAgent）
    if (npc.activeNPC?.AIAgent?.needs != null)
        npc.activeNPC.AIAgent.needs[AgentNeedName.Eat].SetValue(1f);
}
```

## Module code 替换检查表

Phase 6 PawnCheats.cs 需要:
- `PAWNS_COLLECTION_PATH` ← `PlayerManager.instance.GetWorldRulingOrganization().GetFamilyMembersAsReference()` → `List<WorldNPC>`
- `HUNGER_FIELD_NAME` ← `AIAgent.needs[AgentNeedName.Eat].SetValue(1f)`（通过 `activeNPC.AIAgent`）
- `HEALTH_FIELD_NAME` ← `WorldNPC.HP`（public float）；设为 `npc.maxHP`
- `MOOD_FIELD_NAME` ← `ActiveNPC.baseMood` / `baseHappiness`；但更有效的是通过 needs 系统
- `SKILLS_PATH` ← `WorldNPC.aquiredSkills`（`HashSet<SkillName>`）；`Add(skill)` 解锁
- 常量：`HEALTH_MAX` = `npc.maxHP`（通常 1.0f），`HUNGER_MIN` = 1.0f（最大值表示不饿），无独立 MOOD_MAX
