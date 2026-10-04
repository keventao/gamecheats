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

## 本游戏被裁剪（stripped）的 API（实测）

- IMGUI：`GUILayout.FlexibleSpace`、`GUILayout.Begin/EndScrollView`、`GUIStyle.font` setter
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
