# gamecheats

A multi-game, multi-engine cheat/mod workspace. Each top-level directory is **one game**, fully independent — no shared solution or code across games.

Start with a project's local `README.md` and `ROADMAP.md`; they hold authoritative status, limits, and game-version notes. The root [`ROADMAP.md`](ROADMAP.md) is only a cross-project index.

## Projects

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

## Layout (source-type projects)

- `src/<Name>Cheats/` — `Core/` shared, `Modules/` features (one `ICheatModule` per cheat), `Util/`.
- `src/<Name>Cheats.Tests/` — xUnit, logic runnable outside the game.
- `refs/` — committed decompile notes; read before changing any patch or game-API assumption.
- `docs/smoke-checklist.md` — manual in-game checks; the real "done".
- `tools/` — PowerShell `install.ps1`, `tail-log.ps1`, `run-and-check.ps1`.

## Build / test

Run from a project's `src/` (or solution) directory:

```bash
dotnet build -c Release          # plugin DLL
dotnet test                      # xUnit (net8.0)
```

Game-assembly paths are per-project, not hardcoded: resolved via an MSBuild prop (`$(GameManaged)`/`$(GameRoot)`) fed by a per-game environment variable. Check each `.csproj`/`Directory.Build.props`. Override with `-p:GameRoot="<GAME_ROOT>"`.
