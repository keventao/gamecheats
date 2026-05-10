# Repository Guidelines

## Language & Core Principles

- First reply to the user in Chinese.
- Default to Chinese replies unless the user asks for English.
- Keep a caveman-full style: short, precise, low ceremony.
- Prefix shell commands with `rtk` by default.
- Think Before Coding: do not assume, and do not hide confusion.
- Simplicity First: use the minimum code that solves the problem.
- Surgical Changes: touch only what is necessary.
- Goal-Driven Execution: define success criteria, then loop until verified.

## Project Overrides

- Treat this repository as a multi-game, multi-engine cheat/mod workspace. Supported targets may include Unity, Godot, Mono, IL2CPP, BepInEx, MelonLoader, native games, and future game-specific stacks. Prefer project-local docs before general assumptions:
  - `Humanica/README.md`, `Humanica/ROADMAP.md`, and `Humanica/refs/**` for Humanica.
  - `For The King/README.md` and `For The King/refs/**` for For The King.
  - `Timberborn/README.md`, `Timberborn/ROADMAP.md`, and `Timberborn/refs/**` for Timberborn.
  - `LordsAndVilleins/README.md`, `LordsAndVilleins/ROADMAP.md`, and `LordsAndVilleins/refs/**` for Lords & Villeins.
- Review likely cheat/gameplay integration code first:
  - `*/src/**/Modules/**`
  - `*/src/**/Core/**`
  - `*/src/**/*.csproj`
  - `*/src/**/*.Tests/**`
  - project `refs/**`, `docs/**`, and smoke checklists when behavior changes.
- Avoid unrelated asset, binary, or generated-file churn. Do not import or edit game binaries, copied assemblies, engine import folders, mod loader runtime files, saves, backup folders, `Library`, `.godot`, `Temp`, or logs unless explicitly requested.
- Game cheat/mod development tasks should use local Game Studios capability when it helps:
  - Relevant CCGS roles as reference: `producer`, `game-designer`, `systems-designer`, `unity-specialist`, `gameplay-programmer`, `ui-programmer`, `qa-lead`, `release-manager`.
  - Relevant CCGS skills are installed globally with the `ccgs-` prefix, for example `ccgs-project-stage-detect`, `ccgs-sprint-plan`, `ccgs-dev-story`, `ccgs-design-review`, `ccgs-architecture-review`, `ccgs-smoke-check`, `ccgs-qa-plan`, `ccgs-milestone-review`, and `ccgs-release-checklist`.
  - Superpowers remains the development methodology layer: writing plans, executing plans, TDD, debugging, and verification. It supports Game Studios judgment; it does not replace project-specific or domain-specific judgment.
  - For complex game-mod tasks, define acceptance criteria first with the relevant Game Studios skill/role perspective, then implement. For Unity-specific tasks, include the unity-specialist perspective. For non-Unity engines, use the closest available technical specialist plus project-local research. Major milestone or release calls should consider producer + the relevant technical specialist + release-manager perspectives.
  - Use Codex subagents only when the user explicitly asks for agents, delegation, or parallel work.

## Project Structure & Module Organization

This repository hosts multiple game-specific cheat-tool projects, one subdirectory each. Active projects:

- `LordsAndVilleins/` - BepInEx + Harmony mod for *Lords & Villeins* (Unity Mono). The structure below describes this project.
- `For The King/` - BepInEx + Harmony mod for *For The King* (Unity Mono). See `For The King/README.md` for project-specific commands and structure.
- `Timberborn/` - research workspace for a future *Timberborn* mod or cheat. See `Timberborn/README.md` and `Timberborn/ROADMAP.md`.
- `Humanica/` - MelonLoader + HarmonyX mod for *Humanica* (Unity IL2CPP). See `Humanica/README.md` for project-specific commands and structure.
- `fightlife mods/` - packaged Unity Mono managed-DLL cheat files for *FightLife Vanguard*. See its local install notes.
- `spacehaven/` - offline save editor and future modloader/Workshop mod workspace for *Space Haven*. See `spacehaven/README.md` and `spacehaven/MODDING.md`.

The sections below describe **LordsAndVilleins**; for other projects, prefer their local README and roadmap/status docs.

- `src/LordsAndVilleinsCheats/` contains the mod source. Core shared services live in `Core/`, feature modules live in `Modules/`, and utility code lives in `Util/`.
- `src/LordsAndVilleinsCheats.Tests/` contains xUnit tests for logic that can run outside the game.
- `tools/` contains PowerShell helper scripts for installing, launching, and reading logs.
- `refs/` stores dnSpy/game research notes. Check these before changing Harmony patches or game API assumptions.
- `docs/smoke-checklist.md` lists manual in-game checks for release confidence.
- Root `ROADMAP.md` tracks repository-level status and cross-project follow-ups.

## Build, Test, and Development Commands

- `dotnet build -c Release` builds the plugin DLL.
- `dotnet test` runs the xUnit test project.
- `powershell tools/install.ps1` copies the Release build to `BepInEx/plugins/LordsAndVilleinsCheats/`.
- `powershell tools/tail-log.ps1` follows `BepInEx/LogOutput.log`.
- `powershell tools/run-and-check.ps1` runs the build/install/launch/log-check loop. Exit codes are `0` PASS, `1` FAIL, and `2` WARN.

`Directory.Build.props` defines the default `GameRoot`. Override it when needed, for example:

```bash
dotnet build -c Release -p:GameRoot="<LAV_GAME_ROOT>"
```

## Coding Style & Naming Conventions

Use C# with nullable reference types enabled. The mod targets `netstandard2.1`, uses C# 10, and disables implicit usings. Follow the existing brace style: namespace and type blocks use Allman braces with four-space indentation. Use `PascalCase` for public types, methods, and properties; `camelCase` for locals and parameters; and `_camelCase` for private fields.

New cheat features should usually be implemented as `ICheatModule` classes under `Modules/` and registered from `Plugin.Awake()`.

## Testing Guidelines

Tests use xUnit on `net8.0`. Name test files after the unit under test, such as `ModuleRegistryTests.cs`, and use descriptive test method names like `TestInfrastructure_Works`. Prefer pure logic tests for config, module registry behavior, save backup handling, and Harmony helper behavior. For in-game behavior, update `docs/smoke-checklist.md` and verify logs for the patch summary.

## Commit & Pull Request Guidelines

Recent commit history was not available in this environment, so use concise imperative commit subjects, for example `Add royalty favor module tests`. Pull requests should describe the gameplay change, list test results (`dotnet test`, smoke checks, or log output), note the tested game version, and link any relevant `refs/` research or roadmap item. Include screenshots only when the IMGUI panel changes.

## Security & Configuration Tips

This mod backs up saves, but contributors should still test against disposable saves. Do not commit local game paths, copied game assemblies, BepInEx binaries, or save files.
For packaged projects, do not commit `.bak` restore files, savegames, or OS metadata such as `.DS_Store`.
