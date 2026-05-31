# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repo is

Multi-game, multi-engine cheat/mod workspace. Each top-level dir is **one game**, independent — no shared solution or code across games. Read a project's local `README.md` + `ROADMAP.md` first; they hold authoritative status, limits, game-version notes. Root `ROADMAP.md` is only a cross-project index.

| Project | Type / loader | Build |
|---|---|---|
| `Humanica/` | MelonLoader + HarmonyX, Unity IL2CPP | `dotnet` |
| `LordsAndVilleins/` | BepInEx 5 + Harmony, Unity Mono | `dotnet` |
| `For The King/` | BepInEx 5 + Harmony, Unity Mono | `dotnet` |
| `FactoryTown/` | BepInEx + Harmony, Unity Mono | `dotnet` |
| `Farthest Frontier/` | Harmony plugin, Unity Mono | `dotnet` |
| `Timberborn/` | Official mod: `KKDoubleResources` (C#) + `all-in-one-gen` (JSON gen) | mixed |
| `spacehaven/` | Save editor + Space Haven Mod Loader XML | Python/Tk/XML |
| `fightlife mods/` | Unity Mono managed-DLL injection | packaged binaries (no source) |
| `轮回修仙路/` | BepInEx 6 + HarmonyX, Unity IL2CPP | `dotnet` scaffold |

## Layout (source-type projects)

- `src/<Name>Cheats/` — `Core/` shared, `Modules/` features (one `ICheatModule` per cheat, registered in `Plugin.Awake()`), `Util/`.
- `src/<Name>Cheats.Tests/` — xUnit, logic runnable **outside game** (config, registry, save-backup, Harmony helpers).
- `refs/` — **committed** dnSpy/decompile notes; read before changing any patch or game-API assumption. `refs/decompiled/` is local-only.
- `docs/smoke-checklist.md` — manual in-game checks; the real "done".
- `tools/` — PowerShell `install.ps1`, `tail-log.ps1`, `run-and-check.ps1`.

## Build / test

Run from project `src/` (or sln) dir:

```bash
dotnet build -c Release          # plugin DLL
dotnet test                      # xUnit (net8.0)
```

Game-assembly path is per-project, not hardcoded: resolved via MSBuild prop (`$(GameManaged)`/`$(GameRoot)`) fed by a per-game env var — check `.csproj`/`Directory.Build.props` (e.g. `Farthest Frontier` → `FARTHEST_FRONTIER_GAME_ROOT`). Override: `-p:GameRoot="<GAME_ROOT>"`.

`tools/run-and-check.ps1`: build→install→launch→log-check (exit `0`=PASS, `1`=FAIL, `2`=WARN). Timberborn validation: `Timberborn/tools/check_*.sh`.

## Reverse-engineering discipline (project-critical)

- **Never guess game method/field names.** dnSpy / `refs/` first, then write. Unverified names marked "占位,待确认".
- **Patch only methods you must**; bodies change only necessary fields. No opportunistic "improvements".
- **"Done" = visible in-game behavior change**, not code that looks right. Each feature maps to a `smoke-checklist.md` item. Cannot verify yourself → say "code written, user must confirm X in-game", do not claim done.
- Prefer save-XML / save-editor over runtime injection when the game supports it.

## Conventions

- Game DLLs/PDBs never enter git (`.gitignore` `**/*.dll`); reference local game dirs via csproj `<Reference HintPath>`.
- Keep generated mods, saves, backups, extracted assets, logs out of git. No `.bak`, savegames, `.DS_Store`.
- Internal agent design/plans stay out of public repo unless scrubbed of local paths + personal details.
- Update `<project>/ROADMAP.md` when status changes.

## Style (C# projects)

Nullable on, implicit usings off. Allman braces, 4-space indent. `PascalCase` types/methods/props, `camelCase` locals/params, `_camelCase` private fields. Check each `.csproj` for its own target framework/C# version.

Default to Chinese replies unless user asks for English.
