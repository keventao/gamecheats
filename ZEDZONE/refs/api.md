# ZEDZONE 游戏 API 笔记（来源：BepInEx interop 反编译 + Jim97 样本对照，均已进游戏验证）

## 玩家状态访问链

```text
GameController.instance
  -> gameData                      (GameData)
    -> playerData                   (CharacterData)
      -> characterStatusData        (CharacterStatusData)
        -> hp / maxhp / energy / maxEnergy / fatigue / maxFatigue
           / hunger / thirst / sanity / bloodVolume / bodyTemperature
      -> inventoryData              (InventoryData)
```

`GameController.instance` / `GameData.playerData` / `BasicCharacterController.characterData`
均为实名静态属性或实例属性，可直接编译引用。

## 物品库

- `ItemManager.instance.itemAttrDic`：`Dictionary<int, ItemAttr>` 全量枚举，`hiddenItem == true` 跳过
- `ItemAttr`：`itemId` / `itemName` / `itemName_Runtime`（当前语言）/ `ItemName_EN` /
  `stackNumber`（最大堆叠）/ `itemType`（`ItemType` 16 值：Material/Food/FaceMask/
  MeleeWeapon/RangedWeapon/Clothing/Ammo/Backpack/Throwable/Magazine/Arrow/
  GunPart/Deployable/Container/Liqiud/Cloak）/ `weight`
- 入包正路：`GameController.AddItemToPlayer(ItemData, bool)`（只需 `itemId` + 数量，
  游戏侧自补初始化；武器按 `stackNumber` 给）
- 枪/匣附加：`ItemManager.EnsureRangedWeaponProperties(ItemData)`
- 备选（未采用）：`InventoryData.TryAddItem(data, bool, bool)` /
  `TryAddItemWithoutChangeItem(data)`（裸 ItemData 会被拒）

## Jim97 ScriptTrainer v1.0.3 补丁对照（参考用，本体已死）

| 补丁类 | 目标 | 作用 |
|---|---|---|
| `CharacterDataPatch`（TargetMethods） | `CharacterData` | 建人物属性修改 |
| `..._totalItemWeight` | `InventoryData.RefreshTotalItemWeight()` | 无限负重（模糊匹配，新版双重载炸 whole plugin） |
| `..._SetWeaponData` | `CharacterEquipmentData.SetWeaponData(ItemData, Int32)` | 武器满耐久满弹匣 |
| `..._CostItemDurability` | `InventoryData.CostItemDurability`（显式 Type[]） | 无限耐久 |
| `BasicMeleeWeaponOverridePatch`（TargetMethods） | `BasicMeleeWeapon` | 近战 |
| `FiringPatch`（TargetMethods） | `BasicRangedWeapon` 开火链 | 无限子弹（枪械/能量武器，弓弩无效） |
| `..._CanFire` | `BasicRangedWeapon.CanFire()` | 永远可开火 |
| `..._WeaponMalfunction` | `BasicRangedWeapon.WeaponMalfunction` Prefix false | 去卡壳 |
| `Scripts` | `AddSkillPoint/AddPerkPoint/AddAttrPoint`、`MaxBackpackSize`、`ZeroBackpackWeight`、`AddCar`（经 `InGameController+BasicVehicle+MapController`）、`Cure`（遍历 `CharacterBodyPartsData` + `CharacterStatusData`） | |

## 点数 API（v0.6.0/v0.7.0 实测）

- `CharacterData.characterAttrPoint` / `.characterSkillPoint` / `.characterPerkPoint`
  （`get/set_*` 实名，直接 `+= 10`；另有 `AddSkillPoint(Int32)` 方法）
- 建人物默认：属性 8 / 技能 10 / 特性 5（用户确认值）
- 双路径：游戏内走 `GameController.instance.gameData.playerData`；
  建人物界面走 `NewGameSubMonitorPanel_NewCharacter.characterData`
  （`FindObjectsOfTypeAll` + `TryCast` 找面板，`get_characterData()` 直接改，建物 UI 实时刷新）
- 存档/云同步对改后点数正常（`保存角色` + SteamCloud 上传成功）

## 红字调查（2026-10-06，未定性为插件问题）

- `Player.log` 两个 `IndexOutOfRangeException`（BepInEx 日志零 Error）：
  ① 近战链 `OnMeleeWeaponHit→AddAttrExp→RefreshPerks→RefreshInventorySize`；
  ② 悬停 `GetItemDetailString`。
- 嫌疑：极端点数值撑爆按正常范围建的表 / 刷的裸物品缺说明字段。
  用户跳过无-mod对照测试，接受现状。复发时再议。

## 本游戏被裁剪（stripped）的 API（实测）- IMGUI：`GUILayout.FlexibleSpace`、`GUILayout.Begin/EndScrollView`、`GUIStyle.font` setter
  → `NotSupportedException: Method unstripping failed`（共 1228 个复活失败方法）
- OS 字体：`Font.CreateDynamicFontFromOSFont` 全灭（`TypeLoadException`）
  → 用游戏自带 `SourceHanSans-VF-AllLanguage.ttf`（`FindObjectsOfTypeAll` + name 匹配，
  注意主菜单时字体尚未加载，读档后才有）
- `GameObject.AddComponent<RectTransform>()` 在已有 Transform 的对象上返回 null
  → 构造时自带：`new GameObject(name, Il2CppType.Of<RectTransform>())`
- 行内元素进 LayoutGroup 必须给 `LayoutElement` 显式宽高，否则按 0 排
- 注入类型的方法签名不能含 `System.Action`/`System.Exception` 等（注册 warning）；
  回调统一 `new Action(..)` 隐式转 Il2Cpp 委托（编译期验证）
- 直接编辑混淆 DLL 必死：ConfuserEx anti-tamper 在 module `.cctor` 自毁

## 耐久语义与刷枪显示问题（v0.4.7→v0.4.18 实测结论）

- `ItemData.durability` 是**百分比 0-100**：natural 枪（M2R/求生霰弹/S&W929/多功能斧，
  attrhp 各为 350/150/360）普查**全部 dur=100**。写 `attr.hp`（120/320/800）会超量溢出。
- 刷出的枪即使 `durability=100` 也显示 0 + 故障标签，但**装弹后一直能正常开火**
  （live `BasicRangedWeapon.isMalfunction=False`）→ 纯显示层 artifact，功能无碍。
- 维修包按百分比修（0→83 实测一次）。`InventoryData.RestoreItemDurability(itemId, 99999, …)`
  返回 0（参数语义不对，非正确修复入口）。
- `GetGunPartDurability` 对 natural 枪恒返回 attrhp，对刷出的枪返回 0；
  `GetInstalledGunPartsPrice` 刷出的枪为 0。零件组装链未补（open）。
- `HumanCharacterController.rangedWeapon` 可拿到 live 枪；
  `ClearWeaponMalfunction(gun)` 为游戏原生清除动作。
- `WeaponMalfunction` 为单重载，Harmony Prefix false 全禁无歧义（`NoMalfunction.cs` 已应用）。
