# Phase 1 — dnSpy 调研清单

每完成一项把 `[ ]` 改为 `[x]`,然后 commit 对应的 0X 文件。

- [x] 01 — Economy(金币、食物、木材、石材的存储与更新)
- [x] 02 — Pawn(村民/领主对象、属性、技能、状态字段)
- [x] 03 — Time(游戏时间、速度乘数、季节)
- [x] 04 — Build(蓝图、材料消耗、建造完成回调)
- [x] 05 — Bootstrap(获取上述单例的 Awake/Start 入口)
- [x] 06 — Version(`Application.version` 当前值,作为兼容白名单初值)

## dnSpy 通用操作

1. 启动 dnSpy(或 dnSpyEx)
2. File → Open → `<LAV_GAME_ROOT>\Lords and Villeins_Data\Managed\Assembly-CSharp.dll`
   - 同时 File → Open → `Assembly-CSharp-firstpass.dll`(可能含部分基础类)
3. 等待解析完成(30–60 秒)
4. `Ctrl+Shift+K` 跨 assembly 搜索关键词
5. 命中类右键 → Analyze → 看 Used By / Uses
6. 决定 patch 钩子,把结论填进对应 `0X-*.md`

## 完成标志

所有 `[ ]` 变 `[x]`,且 6 个 0X 文件中所有 `<FILL_*>` 占位都被替换为真实值。

完成后通知 controller(Claude Code)继续 Phase 4。
