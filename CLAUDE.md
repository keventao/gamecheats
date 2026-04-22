# gamecheats — 项目规则

This directory hosts game modding / cheat-tool projects. Each subdirectory is one game (e.g. `LordsAndVilleins/`).

## 必须遵守的核心原则(同步全局 `~/.claude/CLAUDE.md`)

### 1. Think Before Coding — Don't assume. Don't hide confusion.
逆向工程游戏 DLL 时尤其重要:**不要猜方法名/字段名**。先 dnSpy 看,看完写。猜测必须显式标注为"占位,待确认"。

### 2. Simplicity First — Minimum code that solves the problem.
修改器自用阶段尤其禁止过度工程:不写"以后可能用到"的反射框架、不做"万一以后支持其他游戏"的抽象。功能模块短而直接。

### 3. Surgical Changes — Touch only what you must.
Mod 项目几乎都依赖 Harmony patch:**只 patch 必要的方法**,patch body 只改必要的字段。不顺手"优化"游戏其他行为。

### 4. Goal-Driven Execution — Define success criteria. Loop until verified.
Mod 的成功标志是**游戏内可见的行为变化**,不是代码"看起来对"。每个功能必须走对应的冒烟清单条目验证(见各项目 `docs/smoke-checklist.md`)。无法亲自验证时(例如需要在游戏内手动操作),明确说"代码已写,需用户在游戏内确认 X",而不是声称完成。

## 项目通用约定

- 每个游戏修改器一个独立子目录,独立 sln/csproj
- 游戏 DLL 永远 **不入仓**(`.gitignore` 强制 `**/*.dll`),用 csproj `<Reference HintPath="...">` 引用本机游戏目录
- 反编译笔记放 `<project>/refs/`(**入仓**;模块代码消费这些笔记)
- 反编译输出(ilspycmd 产物)放 `<project>/refs/decompiled/`,git 忽略(几千 .cs 文件,纯本地)
- 设计文档放 `<project>/docs/superpowers/specs/YYYY-MM-DD-<topic>-design.md`
- 实施计划放 `<project>/docs/superpowers/plans/`
- 每个项目自带 `<project>/ROADMAP.md`(状态、已知限制、下一版规划、版本历史)

## 当前活跃项目

| 项目 | 版本 | 状态 |
|---|---|---|
| `LordsAndVilleins/` | v0.1.0 | code complete,等游戏内冒烟。详见 `LordsAndVilleins/ROADMAP.md` |
