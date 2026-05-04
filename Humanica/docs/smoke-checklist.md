# Humanica Cheats v0.1.1 冒烟清单

> 每次版本更新后按顺序执行。"游戏中"= 已进入存档。
> 标注"[!] 占位"的条目需额外记录实际游戏行为。

## 基础加载

- [ ] MelonLoader 控制台显示 `HumanicaCheats v0.1.1 已加载 (4 模块)`
- [ ] 控制台显示 `[ResourceIndex.dump] enum values: 143`(启动 dump 跑过)
- [ ] 游戏无崩溃,正常进入主菜单
- [ ] 进入存档后按 **F1** → 左上角黄色标签出现,蓝色标题栏的窗口弹出
- [ ] 抓住标题栏 → 窗口能拖动到任意位置
- [ ] 再按 **F1** → 面板关闭,光标状态还原(进窗时强制 unlock + visible,关窗还原)
- [ ] Tab 栏显示 **时间 / 资源 / 村庄 / 解锁** 四个标签

## 时间模块

- [ ] 切换到"时间"Tab
- [ ] 点击 **×2** → 游戏速度明显加快(人物行走、建造进度)
- [ ] 点击 **×5** → 更快
- [ ] 点击 **×10** → 非常快
- [ ] 面板显示 `目标: ×10   实际: Time.timeScale = 10.0`(值一致说明游戏未覆写)
  - **若目标与实际不一致:** 记录到 refs/03-time-research.md,v0.2 需 OnUpdate 强写或 patch 覆写源
- [ ] 点击 **重置为 ×1** → 速度恢复正常,面板显示 `目标: ×1   实际: Time.timeScale = 1.0`

## 资源模块(v0.1.1 重做)

- [ ] 切换到"资源"Tab,显示 5 个槽,默认 `STICKS / LOG / COBBLESTONES / RAW_PELT / BREAD`
- [ ] 每个槽行格式:`EN / 中文 (idx)` 按钮 + `+5` + `+50` + `锁定≥50` toggle

### 手动仓库扩容

- [ ] 资源 Tab 显示手动扩容 **×1 / ×2 / ×5 / ×10** 和 **执行扩容** 按钮
- [ ] 控制台显示 `[WarehouseCapacityPatch] Disabled: no always-on warehouse resize patches are installed.`
- [ ] 点击倍率按钮 → active 高亮切换到对应倍率
- [ ] 选择 **×1** 后点击 **执行扩容** → UI 显示 `x1 不需要扩容`
- [ ] 在一次性测试存档中选择 **×2** 后点击 **执行扩容** → `UserData/HumanicaCheats/SaveBackups` 下创建备份
- [ ] 执行后 UI 显示尝试/扩容/跳过/错误数量,控制台写入 `[WarehouseCapacityPatch] Manual expansion`
- [ ] 重复点击同一倍率的 **执行扩容** → 容量不再继续叠乘,日志显示 already at / 跳过
- [ ] 从 **×5** 切到 **×2** 后执行 → 若多余格子为空则缩到原始 baseline ×2
- [ ] 从 **×5** 切到 **×2** 后执行 → 若多余格子有资源则拒绝缩容并显示错误数量
- [ ] 仓库可用格子或容量在 UI 刷新/重载后增加,每格资源数量不超过原版上限
- [ ] 从主菜单重载存档成功,无 `EndOfStreamException`
- [ ] 完全重启游戏后再次载入存档成功
- [ ] 扩容后进行几分钟战斗,不再复现 `coreclr.dll 0xc0000005` 崩溃

### 增量 +5 / +50

- [ ] STICKS `+5` → 仓库 sticks +5,控制台 `[ResourceCheats] AddRes(STICKS idx=1 amt=5) OK`
- [ ] LOG `+50` → 仓库 logs +50
- [ ] COBBLESTONES `+50` → 仓库 cobblestones +50(不再被截到 +10,验证 createIfNeeded=true 修复)
- [ ] RAW_PELT `+50` → 仓库 raw pelts +50
- [ ] BREAD `+50` → 仓库 bread +50
- [ ] 多次连点 +50 不应卡死游戏(回归 v0.1.0 卡死问题)

### 资源选择器

- [ ] 点击任一槽的资源名 → 弹出选择器,标题 `为槽 N 选资源`
- [ ] 搜索框默认聚焦(cyan 边框 + `|` 光标)
- [ ] 输入英文 `ax` → 列表过滤出全部 axes(STONE_AXE / COPPER_AXE / BRONZE_AXE / IRON_AXE)
- [ ] 输入中文 `斧` → 同样过滤(IME 输入完成后字符进 search)
- [ ] **Backspace** 删字,过滤恢复
- [ ] **Esc** 清空 + 失焦
- [ ] 鼠标滚轮在列表区滚动
- [ ] 点击列表项 → 选定,选择器关闭,槽资源名更新
- [ ] 重启游戏 → 槽配置保留(读 `<game>/UserData/MelonPreferences.cfg` 验证)

### 资源锁定

- [ ] 勾选任一槽的 **锁定≥50** toggle
- [ ] 消耗该资源到 < 50 → 自动补到 50
- [ ] 取消勾选 → 不再补充

## 村庄模块

- [ ] 切换到"村庄"Tab
- [ ] 确认 MelonLoader 控制台显示 `[VillageCheats] 建造速度 patch OK` 和 `[VillageCheats] 生产速度 patch OK`
  - **若 patch 失败:** 面板显示 `[!] 建造 patch 未绑定` / `[!] 生产 patch 未绑定`,记录到 refs/ 待 v0.2 修复
- [ ] 点击 **立即添加 1 名村民** → 新村民立即出现在游戏世界
  - 控制台应显示 `[VillageCheats] SpawnRandomVillager 已调用`
- [ ] 勾选 **建造速度 ×10** → 建造进度条快速填满(验证方向: 工期缩短而非变长)
  - **若建筑反而变慢:** CalculateProgressPerTimeStep 返回的是工期而非进度速率,记录到 refs/,v0.2 改为 `__result /= 10f`
- [ ] 取消勾选 **建造速度 ×10** → 恢复正常建造速度
- [ ] 勾选 **生产速度 ×10** → 工坊产出速度明显加快

### 自己种植作物生长

- [ ] 村庄 Tab 显示 **自己种植作物生长 ×10** 开关
- [ ] 若作物 patch 失败,面板显示 `[!] 作物生长 patch 未绑定`,记录 MelonLoader 控制台日志
- [ ] 开关关闭时,自己种植作物按正常速度生长
- [ ] 开启开关后,自己种植作物生长明显加快
- [ ] 野外自然植物 / flora 不加速
- [ ] 普通资源点、建造速度、工坊生产速度不受该开关影响
- [ ] 关闭开关后,新观察到的作物生长恢复正常速度
- [ ] 保存并从主菜单重载无异常
- [ ] 完全重启游戏后重载存档无异常

### 己方村民移动速度

- [ ] 村庄 Tab 在同一栏显示 **己方村民移动速度**、**2倍**、**5倍**
- [ ] 若移动 patch 失败,面板显示 `[!] 村民移动 patch 未绑定`,记录 MelonLoader 控制台日志
- [ ] 未选择倍率时,己方村民移动速度正常
- [ ] 点击 **2倍** 后,己方村民移动明显加快约 2 倍,按钮高亮
- [ ] 点击 **5倍** 后,己方村民移动明显加快约 5 倍,按钮高亮且 2倍 取消高亮
- [ ] 再次点击已高亮按钮后,倍率取消,己方村民恢复正常移动速度
- [ ] 野外动物、敌人、非己方单位不受该倍率影响

## 解锁模块

- [ ] 切换到"解锁"Tab
- [ ] 面板显示 `[!] 建筑解锁由科技驱动，解锁全部科技后建筑自动可用`
- [ ] 点击 **解锁全部科技 (InstantResearchAll)** → 科技树全亮
  - 控制台应显示 `[UnlockCheats] InstantResearchAll 已调用`
  - 验证: 所有科技显示为已研究,依赖科技解锁的建筑变为可建

## 冒烟结果汇总

| 模块 | 状态 | 备注 |
|------|------|------|
| 基础加载 | ⬜ | |
| 时间 | ⬜ | |
| 资源 — 增量 | ⬜ | |
| 资源 — 手动仓库扩容 | 🟨 | 常驻 ResizeInventory patch 已禁用;改为点击按钮一次性扩容,需一次性测试存档验证 |
| 资源 — 选择器/搜索 | ⬜ | |
| 资源 — 锁定 | ⬜ | |
| 村庄 | ⬜ | 建造速度方向、自己种植作物 ×10 需确认 |
| 解锁 | ⬜ | |

**验证日期:** ___________  
**MelonLoader 版本:** ___________  
**游戏版本:** ___________
## 2026-05-03 Warehouse Capacity Crash Isolation

- Current expected state: always-on warehouse patches are disabled.
- Startup log should contain:
  - `[WarehouseCapacityPatch] Disabled: runtime warehouse slot resizing caused repeatable combat crashes.`
- Resource tab now exposes manual one-shot expansion; warehouses resize only when the player clicks `执行扩容`.
- Regression notes:
  - ResizeInventory-based slot expansion created usable slots and survived save reload.
  - It also caused repeatable combat crashes (`coreclr.dll`, `0xc0000005`).
  - Disabling the patch allowed the same combat scenario to complete without crashing.
- Before re-enabling any warehouse expansion:
  - Test only on disposable saves.
  - Confirm save backup creation.
  - Confirm no combat crash after waiting several minutes and fighting.
  - Confirm save, reload, full restart, and reload again.

## 2026-05-04 Stable Smoke Addendum

Run on a disposable save first, then on the backed-up target save.

This addendum supersedes older warehouse shrink checks. Stable builds must not shrink expanded warehouses.

### Warehouse Expansion And Save Snapshot

- [ ] Startup log contains the installed DLL hash expected for the current build.
- [ ] Startup log contains `Save snapshot hook OK (Il2CppHumanica.SaveLoading.SaveLoader, methods=1)`.
- [ ] Startup log does not contain `warehouse resource snapshot saved (periodic)`.
- [ ] Load a save with warehouse multiplier x5 selected.
- [ ] Auto expansion log shows consistent baselines, for example `baseline=16,16,16,16,16,16`.
- [ ] Click `执行扩容`; log shows `manual warehouse expansion x5`.
- [ ] Add or gather warehouse resources after expanding.
- [ ] Save the game normally.
- [ ] Log shows `warehouse resource snapshot saved (game-save)` or `warehouse resource snapshot unchanged (game-save)`.
- [ ] Exit the game, restart, reload the same save.
- [ ] Warehouses expand back to x5.
- [ ] Resources present at the time of the normal save are restored.
- [ ] No warehouse shrink happens when changing from a higher multiplier to a lower multiplier.

### Village Cheats

- [ ] Own villager movement speed x2 visibly increases movement.
- [ ] Own villager movement speed x5 visibly increases movement more than x2.
- [ ] Clicking the active movement multiplier again restores normal movement.
- [ ] Build speed x10 visibly accelerates building progress.
- [ ] Production speed x10 visibly accelerates workshop output.
- [ ] Self-planted crop growth x10 affects planted crops.
- [ ] Wild plants and non-crop resource deposits do not receive the crop multiplier.
