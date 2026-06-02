# Clanfolk 本地化 / 物品名 API（已在 Assembly-CSharp.dll 验证）

反编译工具：`ilspycmd`（net8 roll-forward）。来源 DLL：
`<GameRoot>/MelonLoader/Il2CppAssemblies/Assembly-CSharp.dll`。

## 取物品本地化名（首选）

`Il2Cpp.Entity` 有虚方法：

```csharp
public virtual string GetEntityName();   // Entity.cs:6938，返回当前语言（中文）名
```

`Il2Cpp.Item : Entity` 继承之。prefab 与场上实体都是 `Entity` 子类，反射无参调用
`GetEntityName()` 即可拿到游戏自带的中文名 —— 覆盖全部物品，不止硬编码字典那几十个。

> 注意：实体的 `displayName` 字段在 prefab 上常为空/英文，**不要**用它当主来源。

## 按 key 本地化（仅 key、无实体时的兜底）

- `Il2Cpp.GameManager.GetTextBible()` — static，返回 `TextBible`（GameManager.cs:2898）。
- `TextBible.GetText(string key, GrowthStage flags = GrowthStage.NONE)` — TextBible.cs:646，
  返回该 key 的当前语言文本；未知 key 通常回显 key 本身。
- `EntityManager.GetEntityTypeToken(string entityType) -> string[]`（EntityManager.cs:1632）
  由 entityType 取文本 token 数组，逐个喂给 `GetText`，取首个能本地化者。
  token 由 `EntityManager.entityTypeTokenLookup: Dictionary<string,string[]>` 提供；
  数组内索引约定在 managed 代理里看不到（逻辑在 native），故按"名称 token 在前"
  的经验逐个尝试。已接为 `TryTokenLocalize()`。

`GrowthStage` 为 `Il2Cpp` 命名空间下枚举，默认 `NONE == 0`，反射时
`Enum.ToObject(enumType, 0)` 构造。

## 不存在的 API（曾被误用）

- ~~`TextBible.GetTextLookupDictionary()`~~ —— **TextBible 上没有此方法**，旧代码据此查字典
  必然失败，导致大多数物品回退到英文 key。已改为 `GetText`。

## 代码落点

`src/ClanfolkCheats/Modules/ResourceCheats.cs`
- `GetEntityDisplayName()` → 优先 `GetEntityName()`，再退 `displayName`。
- `TryGameLocalize()` → `GetTextBible().GetText(key, NONE)`（带方法缓存）。
- `TryTokenLocalize()` → `GetEntityTypeToken(key)` 各 token 再 `GetText`；
  需 `_itemEntityManager`（发现期缓存的 item EntityManager）。
- 顺序：发现期写入的游戏名 > `GetText(key)` > token→`GetText` > 硬编码 `ZhNames` > 子串匹配 > 原 key。
