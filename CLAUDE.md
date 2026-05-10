# gamecheats — 项目规则

This directory hosts game modding / cheat-tool projects. Each subdirectory is one game or packaged tool (e.g. `LordsAndVilleins/`, `spacehaven/`).

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

- 每个游戏修改器一个独立子目录;源码型 mod 独立 sln/csproj,打包型工具保留本地 README
- 游戏 DLL 永远 **不入仓**(`.gitignore` 强制 `**/*.dll`),用 csproj `<Reference HintPath="...">` 引用本机游戏目录
- 反编译笔记放 `<project>/refs/`(**入仓**;模块代码消费这些笔记)
- 反编译输出(ilspycmd 产物)放 `<project>/refs/decompiled/`,git 忽略(几千 .cs 文件,纯本地)
- Internal agent design and implementation plans should stay out of the public repo unless they are scrubbed for local paths and personal details.
- 复杂源码型项目自带 `<project>/ROADMAP.md`(状态、已知限制、下一版规划、版本历史)
- 根 `ROADMAP.md` 记录仓库级状态、打包型工具状态、跨项目事项

## 当前活跃项目

| 项目 | 版本 | 引擎 | Mod loader | 状态 |
|---|---|---|---|---|
| `LordsAndVilleins/` | v0.1.1 | Unity (Mono) | BepInEx 5 + Harmony | 部分游戏内验证通过;Pawn / Build 待冒烟。详见 `LordsAndVilleins/ROADMAP.md` |
| `For The King/` | v0.1.0 | Unity (Mono) | BepInEx 5 + Harmony | 项目 README 和源码骨架已入仓。详见 `For The King/README.md` |
| `Timberborn/` | v0.1.0 | Unity (Mono) | Official mod + Harmony | `KKDoubleResources` 已验证有效。详见 `Timberborn/README.md` |
| `Humanica/` | v0.1.1 | Unity (IL2CPP) | MelonLoader 0.7.2 + HarmonyX | GUI / 时间 / 资源(5 槽 + 中英搜索 + 持久化)游戏内验证通过;村庄 / 解锁 待冒烟。详见 `Humanica/ROADMAP.md` |
| `fightlife mods/` | package | Unity (Mono) | Managed DLL injection | Windows 安装包文件已入仓;含 heal team / gold / speed / damage。详见 `fightlife mods/README-安装说明.txt` |
| `spacehaven/` | package | save XML | Standalone Tk editor | macOS app + Windows Python launcher 已入仓;支持 Bank / Crew / Resources。详见 `spacehaven/README.md` |
