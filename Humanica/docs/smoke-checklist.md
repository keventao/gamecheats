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
| 资源 — 选择器/搜索 | ⬜ | |
| 资源 — 锁定 | ⬜ | |
| 村庄 | ⬜ | 建造速度方向需确认 |
| 解锁 | ⬜ | |

**验证日期:** ___________  
**MelonLoader 版本:** ___________  
**游戏版本:** ___________
