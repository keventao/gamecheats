# Humanica Cheats — ROADMAP

## v0.1.1 (当前)

**状态:** 部分游戏内验证通过(2026-05-02)。GUI 架构重写,资源模块重做。

**v0.1.0 → v0.1.1 重大变更:**

### IL2CPP IMGUI 架构重写
v0.1.0 用标准 `GUILayout.*` + `GUI.Window` + `GUI.Button` 写的面板,在游戏内表现为:
窗口能弹出但**空白、不能拖、按钮不响应**。诊断后定位三层坑:

1. `GUILayout.*` 一族(BeginHorizontal/Window 等)走 `GUILayoutUtility.BeginLayoutGroup`,
   该路径触发 `ExitGUIException..ctor` / `LayoutedWindow..ctor` 等内部 ctor 签名不匹配,
   抛 `MissingMethodException` 把整个 WindowFunction 第一行就干掉
2. `GUI.Window` 的 `Rect` 返回值不回传:`_windowRect = GUI.Window(...)` 的赋值永远是输入值,
   `GUI.DragWindow` 抓到了拖动手势(hot=11)但窗口位置永不更新
3. `GUI.Button` 的 `bool` 返回值同样不回传:IMGUI 内部 hot control 状态正常翻转,
   但我方代码拿到的永远是 `false`,按钮永远不触发动作

修复:**完全弃用所有有返回值的 IMGUI 方法**,只用 `GUI.Box` / `GUI.Label`(纯绘制),
所有交互(按钮/Toggle/拖动/滚轮)用 `Event.current` + `Rect.Contains` 自实现。
新增 `Core/Layout.cs` 包 `ImguiUtil.Button` / `ImguiUtil.Toggle` helper。

### 资源模块重做
4 个固定资源槽 → **5 个用户可配置槽 + 模态选择器**:
- 点击槽位资源名 → 弹出全部 ~120 项资源列表(去掉 `___DEPRECATED`)
- 搜索框支持**中文 + 英文**过滤(英文匹配 enum 名,中文匹配 i18n 表)
- 滚轮滚动列表
- 选定持久化到 `UserData/MelonPreferences.cfg`,跨会话保留
- +5 / +50 量级(原 +100/+1000;游戏资源消耗量小)
- 锁定 ≥50(原 ≥500)
- 资源 Tab 增加手动仓库扩容 ×1/×2/×5/×10;点击时备份存档并一次性扩容当前仓库
- 默认槽位:STICKS / LOG / COBBLESTONES / RAW_PELT / BREAD

新增 `Core/ResourceI18n.cs`,~100 项 EN → 中文翻译表,缺失项 fallback 到 enum 名。

### 游戏内验证完成项
- ✅ F1 切换 + 自绘窗口 + 拖动 + 4 Tab 切换
- ✅ 资源 Tab:5 槽 + 选择器 + 中英搜索 + 持久化
- ✅ 时间 Tab:×1/×2/×5/×10 切换有效,Time.timeScale 实测同步
- ✅ ResourceIndex 启动 dump 共 **143 项**,确认是 enum 类型
- ✅ 已验证 idx 映射:`STICKS=1` `COBBLESTONES=2` `LOG=3` `WILD_BERRIES=4`
  `APPLE=5` `RAW_MEAT=6` `RAW_PELT=7` `BREAD=32` 等(完整 dump 见
  MelonLoader `Latest.log` 中 `[ResourceIndex.dump]` 节)
- ✅ `AddResourceIntoFreeWarehouse` 第三参 `createIfNeeded`:**必须传 `true`**。
  传 `false` 会截到现有仓库剩余容量(LOG 仓库大没事;COBBLESTONES/RAW_PELT 等
  容量小的被截到 ~10),累积过多还会让游戏 AI 死循环卡死
- ✅ 仓库容量研究:显示倍率、pack size、常驻 ResizeInventory 路线均已验证风险,当前改为手动一次性扩容路线

### 待游戏内确认项
- ⏳ 村庄 Tab:添加村民、建造 ×10、生产 ×10
- ⏳ 解锁 Tab:InstantResearchAll
- ⏳ 资源 Tab: 手动仓库扩容备份、存档重载、完整重启重载、战斗稳定性
- ⏳ 资源锁定 toggle 在低消耗游戏下行为(LockMin=50 是否合适)

---

## v0.1.0 已知限制 — 状态更新

| 原 v0.1.0 限制 | v0.1.1 状态 |
|---|---|
| ResourceIndex 4 个值为占位推断 | ✅ 解决 — 改成用户可选 + i18n + dump,支持任意 idx |
| `AddResourceIntoFreeWarehouse` 第三参语义未确认 | ✅ 解决 — 必须 `true` |
| 建造速度 patch 方向未确认 | ⏳ 仍未验证(等村庄 Tab 冒烟) |
| patch 目标依赖 IL2CPP 命名空间路径 | ⏳ v0.1.1 未变更,仍需游戏更新时重新对齐 |
| 多存档并发未测试 | ⏳ 仍未测 |

---

## v0.2.0 (待规划)

- 完成村庄 / 解锁 Tab 游戏内冒烟,失败项进入 v0.2 修复列表
- 若村庄建造速度方向错误:修正 postfix 为 `÷10`
- 自己种植作物生长 ×10:放在村庄 Tab,仅当 `HasPlantGrowingTrigger` 与 `Planted == true` 均可确认时生效;否则安全跳过并显示 patch/日志警告
- 己方村民移动速度 2倍/5倍:放在村庄 Tab 作物生长同一区域,同一栏两个选项;仅当移动目标可确认在 `CreatureManager.Villagers` 中时生效
- ResourceI18n 翻译表补全(目前覆盖 ~100 / 143 项)
- 属性/技能作弊(若游戏有村民属性系统)
- 天气/季节控制
- 存档备份
- (可选) 把 IL2CPP IMGUI 弃用 patterns 抽到 LordsAndVilleins 或共享 helper(目前 spec
  排除跨游戏抽象,等出现第三个游戏再说)

---

## 版本历史

| 版本 | 日期 | 说明 |
|------|------|------|
| v0.1.1 | 2026-05-02 | GUI 架构重写(绕 IL2CPP IMGUI 返回值坑) + 资源模块改 5 槽可选 + 中英搜索 + 持久化。游戏内基础功能验证通过。 |
| v0.1.0 | 2026-05-01 | 初版合并到 main。4 模块实现(时间/资源/村庄/解锁)。代码完成,GUI 在 IL2CPP 下不可用。 |
---

## 2026-05-03 Warehouse Capacity Status

- Always-on warehouse capacity / slot resizing patches are currently disabled in code.
- Verified behavior before disabling:
  - Display-only capacity scaling could show larger capacity but did not create usable slots.
  - Per-pack amount scaling corrupted saves and must not be reintroduced.
  - Runtime `Inventory.ResizeInventory()` could create extra slots and survive save reload.
  - The same ResizeInventory-based build caused repeatable combat crashes with Windows `coreclr.dll` `0xc0000005`.
  - A crash-isolation build with `WarehouseCapacityPatch` disabled completed the same combat without crashing.
- Current shipped state:
  - Resource add buttons and other modules remain available.
  - Resource tab exposes manual one-shot expansion buttons.
  - Warehouses resize only when the player clicks the expansion button.
  - Manual expansion records baseline pack counts and applies selected multipliers relative to that baseline, so repeated clicks no longer stack.
  - Lowering the multiplier shrinks only when packs above the target are empty.
- Next safe direction:
  - Do not use always-on Harmony prefixes on warehouse getters.
  - Verify manual one-shot expansion on disposable saves, including combat after expansion.

## 2026-05-04 Stable In-Game Status

- Confirmed working in Humanica 0.8.18 with MelonLoader 0.7.2:
  - own villager movement speed x2/x5
  - production speed x10
  - build speed x10
  - self-planted crop growth x10
  - warehouse x5 auto/manual expansion
  - warehouse resource high-water restore after restart
- Warehouse expansion final behavior:
  - no always-on warehouse getter patches
  - auto expansion runs once after loading a save when multiplier > x1
  - manual `执行扩容` can be clicked to expand newly loaded warehouses
  - shrink is blocked to prevent item loss
  - baseline mismatch falls back to the current warehouse pack count
  - game save hooks save the resource snapshot through `SaveLoader.StartSave(string)`
  - periodic snapshots were removed to avoid main-thread config writes and log spam
- Expected log markers:
  - `Save snapshot hook OK (Il2CppHumanica.SaveLoading.SaveLoader, methods=1)`
  - `warehouse resource snapshot saved (game-save)` or `unchanged (game-save)`
  - no `warehouse resource snapshot saved (periodic)` lines
