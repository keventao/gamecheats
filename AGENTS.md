# Repository Guidelines

## Language & Core Principles

- First reply to the user in Chinese.
- Think Before Coding: do not assume, and do not hide confusion.
- Simplicity First: use the minimum code that solves the problem.
- Surgical Changes: touch only what is necessary.
- Goal-Driven Execution: define success criteria, then loop until verified.

## Project Structure & Module Organization

This repository hosts multiple game-specific cheat-tool projects, one subdirectory each. Active projects:

- `LordsAndVilleins/` — BepInEx + Harmony mod for *Lords & Villeins* (Unity Mono). The structure below describes this project.
- `For The King/` - BepInEx + Harmony mod for *For The King* (Unity Mono). See `For The King/README.md` for project-specific commands and structure.
- `Humanica/` — MelonLoader + HarmonyX mod for *Humanica* (Unity IL2CPP). See `Humanica/README.md` for project-specific commands and structure.

The sections below describe **LordsAndVilleins**; for Humanica, prefer `Humanica/README.md` and `Humanica/ROADMAP.md`.

- `src/LordsAndVilleinsCheats/` contains the mod source. Core shared services live in `Core/`, feature modules live in `Modules/`, and utility code lives in `Util/`.
- `src/LordsAndVilleinsCheats.Tests/` contains xUnit tests for logic that can run outside the game.
- `tools/` contains PowerShell helper scripts for installing, launching, and reading logs.
- `refs/` stores dnSpy/game research notes. Check these before changing Harmony patches or game API assumptions.
- `docs/smoke-checklist.md` lists manual in-game checks for release confidence.

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
