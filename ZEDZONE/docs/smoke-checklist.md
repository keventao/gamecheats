# ZED ZONE 内置修改器动态验证（smoke-checklist）

- 游戏：隔离区-丧尸末日生存（Steam AppID 1211600，Unity 2023.1.18f1 IL2CPP x64）
- 修改器：`ZGScriptTrainer.dll`（ScriptTrainer v1.0.3，BepInPlugin `ScriptTrainer.Jim97.ZED_ZONE`）
- 框架：BepInEx 6.0.0-be.733（控制台日志已开 `Logging.Console Enabled = true`）
- 配置：`BepInEx/config/ScriptTrainer.Jim97.ZED_ZONE.cfg`（[通用设置] 开启修改器/F9/缩放1；[修改数值设置] 背包大小=(26,41)）
- 存档：`%USERPROFILE%/AppData/LocalLow/LevenLiu/ZEDZONE/`（验证前整体复制备份）

## 步骤

### 0. 备份（必做）
复制 `AppData/LocalLow/LevenLiu/ZEDZONE/` 到别处。修改器会改背包/刷车/属性，坏档不负责。

### 1. 启动确认
- [ ] Steam 启动游戏，BepInEx 黑色控制台窗口弹出且无红色报错
- [ ] 我方检查 `BepInEx/LogOutput.log` 出现 `1 plugin to load` + ScriptTrainer 加载行（由我记录）
- [ ] 进主菜单按 **F9**，修改器窗口弹出（截图/描述：几个 tab、每个 tab 有哪些按钮）

### 2. 功能逐项（建议开新档测，逐条回我 通过/失败+现象）
- [ ] 无限耐久：拿近战砍东西 / 开枪，看耐久条是否不动（对应 `CostItemDurability` patch）
- [ ] 无限子弹：用枪械射击，看弹匣/备弹是否不减（注意：弓弩预期无效，游侠说明写了只限枪械和能量武器）
- [ ] 武器不卡壳：连续射击看是否出故障提示（对应 `WeaponMalfunction` patch）
- [ ] 背包加大：改 cfg `背包大小` 或点界面按钮，看格子是否变 (26,41)
- [ ] 无限负重：塞满东西看超重惩罚是否消失（对应 `totalItemWeight` patch）
- [ ] 加物品：用界面加物品，看是否按最大堆叠给、武器是否满耐久满弹匣
- [ ] 刷车辆：点刷车，车是否出现在人物附近
- [ ] 治疗：故意受伤/弄出伤势，点治疗，看 `CharacterStatusData`（hp/流血/体温）是否回满
- [ ] 建人物属性/技能点：加技能点/专长点/属性点，看是否生效（对应 `AddSkillPoint/AddPerkPoint/AddAttrPoint`）

### 3. 收尾记录（我方）
- [ ] 复制本次 `LogOutput.log` 关键行 + `ErrorLog.log` 到 `logs/`
- [ ] cfg 有无新增项（对比基线 26 行）
- [ ] 存档目录新增/变化文件列表
- [ ] 结论记入 `refs/`（patch 目标 × 实际行为对照表），作为自研 Mod 的基线

## 结果区（验证时填写）

| 功能 | 结果 | 现象备注 |
|---|---|---|
| F9 窗口 | ❌ 失败（Jim97 插件） | 插件加载 abort，窗口根本没创建（见下） |
| 自研 v0.1.0 HpLock | ✅ 通过（2026-10-04） | 无红字，`ZedZoneHpLock v0.1.0 loaded`；挨打秒回满，1200/1200，无秒杀穿透。日志 `logs/LogOutput-20261005-hplock-v010-pass.log` |
| 自研 v0.2.0 StaminaLock | ✅ 通过（2026-10-04） | `energy→maxEnergy` + `fatigue→0` 方向正确，无需改 `maxFatigue` |
| 自研 v0.3.0 Panel | ❌ 被拦（2026-10-04） | F1 回调进了 `DrawWindow`，但内部某调用触发 `NotSupportedException: Method unstripping failed`（1228 个复活失败方法之一），每帧刷屏 |
| 自研 v0.3.1 Panel诊断版 | ❌ 被拦（2026-10-04） | 分段定位成功：`LogSectionError(String, Exception)` 参数不被 IL2CPP 支持（warning）；`Header` 段 `Label` 之后（FlexibleSpace/Button 嫌疑）触发 unstripping。标题能画出，中文全是 tofu 方块（缺 CJK 字体），emoji 同理 |
| 自研 v0.3.2 探针版 | ✅ 探针通过（2026-10-04，用户截图） | OK：Begin/End Vertical/Horizontal、Label、Space、Button、Toggle、TextField、DragWindow、ItemManager.instance、itemAttrDic 枚举；FAIL（被裁）：FlexibleSpace、ScrollView；playerStatus.chain 在主菜单 NRE（预期，存档内才有）。中文/emoji tofu（IMGUI 缺 CJK 字体）→ v0.3.3 起全英文无 emoji |
| 自研 v0.3.3 完整面板 | ⚠️ 半通过（2026-10-04，用户截图） | 布局/开关/分类/搜索/翻页/[+] 全渲染，但物品中文名 tofu（IMGUI 缺 CJK）。`Font.HasCharacter` + `CreateDynamicFontFromOSFont` + `Resources.FindObjectsOfTypeAll` 三件套齐活 |
| 自研 v0.3.4 中文字体版 | ❌ 字体没生效（2026-10-05） | 日志缺 `CJK font:` 行（PickupFont 静默失败 / 未命中字体），tofu 依旧。但 17 分类、开关、[+] 都已英文渲染 OK |
| 自研 v0.3.5 字体预烘焙版 | ❌ 无效（2026-10-05） | `CJK font:` 行一次都没出现 → PickupFont 内部某步在打日志前就挂了；截图窗口标题仍是 0.3.4（旧帧残留，无关）。中文 tofu 不变 |
| 自研 v0.3.6 字体诊断版 | ✅ 定位成功（2026-10-05，用户截图） | 游戏自带 `SourceHanSans-VF-AllLanguage.ttf`（思源黑体，CJK！）；OS 动态字体全被裁（YaHei/SimHei/SimSun/Arial 全 `TypeLoadException`）→ 这就是为什么找不到字体 |
| 自研 v0.3.7 游戏字体直用版 | ❌ GUIStyle.font 赋值被裁（2026-10-05，用户截图） | `PickupFont FAIL NotSupportedException` 刷屏（无节流，每帧进一次）； Cecil 复查：`set_font` 实例方法存在但 unstrip 失败；`GUIStyle.SetDefaultFont(Font)`（static，全局默认字体）在 interop 里带原生注入指针，理论上能用 |
| 自研 v0.3.8 SetDefaultFont 版 | ❌ 中文仍 tofu（2026-10-05，用户截图） | `SetDefaultFont OK` 但无效 → 启动调用太早或 VF 烘焙无声失败 |
| 自研 v0.3.9 完整面板+皮肤字体双修版 | ⚠️ 已被 v0.4.0 取代（未实际测试） | 诊断面板换回完整物品面板；`ApplyFontEarly` 追加 `GUI.skin.font` |
| 自研 v0.4.0 uGUI 版 | ❌ 空面板（2026-10-05，用户截图+后台日志） | `NullReferenceException at ItemPanel.RT`：`AddComponent<RectTransform>` 在已有 Transform 的对象上返回 null（运行时转换被拒）；同链 `AddComponent<Text>` 成功 → 只有这一个转换点坏 |
| 自研 v0.4.1 uGUI 转换修复版 | ✅ 面板出来了（2026-10-05，用户截图） | 中文/开关/分类/列表全渲染；两问题：① 无拖动（v0.4.0 压根没做）；② 列表行重叠（行缺 `LayoutElement.preferredHeight`，VLG 按 0 高堆叠） |
| 自研 v0.4.2 对齐+拖动版 | ⚠️ 半通过（2026-10-05，用户截图） | 拖动 OK，行不叠了；但滚动区整体掉到面板下方（`anchoredPosition=(10, y-380)` 重复减）、滚动条巨大（handle 没尺寸）、[+] 被挡住 |
| 自研 v0.4.3 滚动锚点重摆版 | ⚠️ 半通过（2026-10-05） | 布局进框、中文 OK；但行内 [+] 不可见 + 用户看到红字。查全量启动日志：游戏侧全是良性 warning（Localization/ShapeSettings/读档），唯一红色是我自己的 `scroll top=` 诊断行（LogError 级别，已在 v0.4.4 降为 Info）。[+] 失踪待定位 |
| 自研 v0.4.4 按钮可见性诊断版 | ✅ 定位成功（2026-10-05，后台日志） | `listdiag r0kids=2 [T@(0,0) (536,20)] [B@(536,0) (64,0)]` → 按钮存在、位置正确，**高度=0**（行 HLG 只认 LayoutElement 高度，按钮只有宽没有高） |
| 自研 v0.4.5 行按钮高度修复版 | ⚠️ 半通过（2026-10-05） | [+] 可见可点，但一律 `背包满/添加失败` → `TryAddItemWithoutChangeItem` 拒绝裸 ItemData（缺游戏侧初始化） |
| 自研 v0.4.6 入包路径切换版 | ✅ 通过（2026-10-05，用户确认+日志） | `GameController.AddItemToPlayer` 一次过（id=2/514，via=AddItemToPlayer）；裸 ItemData 只需 itemId+数量，游戏侧自补初始化 |
| 自研 v0.4.7 最大堆叠版 | ⚠️ 半通过（2026-10-05） | 入包 OK 但武器是坏的：耐久 0、弹药 0/4+1。根因：主路径没写 `durability`、没调 `EnsureRangedWeaponProperties`、没配弹药 |
| 自研 v0.4.8 武器满状态版 | ❌ 方向错了（2026-10-05） | `attr.hp`（120/320/800）是备用参考值不是耐久！写入后显示仍 0 + 故障；散装弹药 OK |
| 自研 v0.4.9 耐久诊断版 | ✅ 定位成功（2026-10-05） | `wdiag rw.hp=120 durBefore=0` + 入库 `storedDur=120`，但显示 0 → 显示≠`ItemData.durability` 写入值，或写入后被重置 |
| 自研 v0.4.10 入库直写版 | ❌ 无效（2026-10-05） | 入库后按住背包实例重写耐久，显示仍 0 |
| 自研 v0.4.11 背包普查版 | ✅ 立功（2026-10-05） | 普查 natural 枪（M2R/求生霰弹/S&W929）：**全部 `dur=100`，与 attrhp（350/150/360）无关** → 耐久是**百分比**！之前写超量值溢出 → 显示 0 + 故障；维修包按百分比修（→83）也对上了 |
| 自研 v0.4.12 零件耐久版 | ✅ 定位成功（2026-10-05） | `GetGunPartDurability` 返回值恒等于 attrhp（零件缺失时回退），与显示无关；排除零件理论 |
| 自研 v0.4.13 出厂弹匣版 | ⚠️ 半通过（2026-10-05） | `SwapMagazineIntoWeapon` 静态确认并调用，但管状供弹枪（Remington，defaultMagazineId=0）无匣可链；耐久/故障依旧 |
| 自研 v0.4.14 原生修复版 | ❌ 无效（2026-10-05） | `InventoryData.RestoreItemDurability(itemId, 99999, [inv])` 返回 0，参数语义不对 |
| 自研 v0.4.15 普查按钮版 | ✅ 工具化（2026-10-05） | 普查逻辑独立成面板按钮（标题栏“普查武器”），不再依赖 AddItem 触发 |
| 自研 v0.4.16 百分比修复版 | ❌ 方向错了（2026-10-05） | 写 `100` 后显示仍 0 + 故障。但 natural 枪普查 `dur=100` 实锤百分比语义，错的是别处 |
| 自研 v0.4.17 卡壳抑制版 | ⚠️ 半通过（2026-10-05） | 首个 Harmony 补丁（`WeaponMalfunction` Prefix false，单重载，加载成功）；`partsprice=0`（spawned）；显示依旧 |
| 自研 v0.4.18 实弹验证版 | ✅ 功能通过，显示存疑（2026-10-05） | live 枪 `isMalfunction=False`；用户实测：**刷的枪显示 0/故障，但装弹后一直能正常开火** → 纯显示层 artifact，功能无碍 |
| 自研 v0.5.0 无限弹药版 | ✅ 通过（2026-10-05） | 储备弹药每帧顶满（Ammo/Arrow 全背包）+ `CanFire`恒真 Harmony + 面板第三开关；打空/R 换弹验证通过 |
| 自研 v0.6.0 属性点版 | ✅ 通过（2026-10-05） | `CharacterData.characterAttrPoint += 10`，双路径（游戏内 playerData / 建人物 `NewGameSubMonitorPanel_NewCharacter.characterData`）；建人物默认 8 点可叠加 |
| 自研 v0.7.0 三点数版 | ✅ 通过（2026-10-06） | `characterSkillPoint`（默认 10）/`characterPerkPoint`（默认 5）同逻辑三按钮一排；建人物全程加点（attr 8→98 / skill 10→95 / perk 5→95），存档/云同步正常 |
| 红字调查（2026-10-06） | ⚠️ 结论：游戏侧，与插件无直接关联 | Player.log 两个 `IndexOutOfRangeException`：① 近战命中链 `OnMeleeWeaponHit→AddAttrExp→RefreshPerks→RefreshInventorySize`；② 悬停物品 `GetItemDetailString`。BepInEx 日志零 Error；均为游戏代码栈（EA Beta 已知类问题）。用户决定不做无 mod 对照测试，接受现状 |
| 自研 v0.5.0 无限弹药版 | ✅ 通过（2026-10-05） | 储备弹药每帧顶满（Ammo/Arrow 全背包）+ `CanFire`恒真 Harmony + 面板第三开关；打空/R 换弹验证通过 |
| 无限耐久 | ⏸️ 待插件加载后测 | |
| 无限子弹 | ⏸️ 待插件加载后测 | |
| 不卡壳 | ⏸️ 待插件加载后测 | |
| 背包加大 | ⏸️ 待插件加载后测 | |
| 无限负重 | ⏸️ 待插件加载后测 | |
| 加物品 | ⏸️ 待插件加载后测 | |
| 刷车辆 | ⏸️ 待插件加载后测 | |
| 治疗 | ⏸️ 待插件加载后测 | |
| 技能/属性点 | ⏸️ 待插件加载后测 | |

## 拦路虎（2026-10-04 首启即现，已存档 `logs/LogOutput-20261005-trainer-fail.log`）

`[Error] Error loading [ZED ZONE 内置修改器 1.0.3]: HarmonyException: Ambiguous match for
HarmonyMethod[(class=InventoryData, methodname=RefreshTotalItemWeight, args=undefined)]`

- 根因：修改器是按老版本游戏做的，`[HarmonyPatch(typeof(InventoryData), "RefreshTotalItemWeight")]`
  没写参数类型；新版游戏里这个方法有两个重载（`public void()` + `private void(HashSet<InventoryData>)`），
  Harmony 模糊匹配直接炸，`PatchAll` 中断 → **整个插件加载失败，所有功能全灭，F9 无窗口**。
- 全 DLL 只有这一处模糊匹配撞车（已逐个核对 8 个补丁类的 attribute）：
  `SetWeaponData / CanFire / WeaponMalfunction` 都是单重载，安全；
  `CostItemDurability` 写了显式 `Type[]`，安全；
  `CharacterData / BasicMeleeWeapon / Firing` 三个走 `TargetMethods()` 显式枚举，免疫。
- 修复方案：给该 attribute 补上显式空参数类型（切到 `(Type, String, Type[])` 构造），
  精确定位 `public void RefreshTotalItemWeight()`，其余不动。
- ✅ 修复已执行（2026-10-04 23:16）：原 DLL 备份于 `orig/ZGScriptTrainer.dll.orig`；
  过程踩坑记录：Cecil `Resolve()` 需配 `DefaultAssemblyResolver`（search dir `BepInEx/core` +
  `BepInEx/interop`），`Write` 必须落到别名再 `Copy-Item` 替换（Defender/Steam 云同步会间歇锁原文件，
  `Move-Item -Force` 对被锁目标报 0x800700B7）。落盘后复验：该补丁 attribute 已是
  `.ctor(Type,String,Type[])`。等用户重进游戏验证。
