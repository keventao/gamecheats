# Satisfactory Trainer

External (out-of-process) trainer for **Satisfactory** (build **493833**, UE 5.3.2).
Reads/writes the running game's memory from a separate console app — **no DLL
injection, no game files modified**.

## Cheats

| Hotkey | Cheat | What it does |
|---|---|---|
| **F1** | Achievement Enable | Re-enables Steam achievements while **Advanced Game Settings** are ON (forces `UOnlineIntegrationBackend.bSuppressAchievements = false`). |
| **F2** | Instant Manual Craft | Craft Bench / Equipment Workshop hand-crafting completes instantly (snaps `UFGWorkBench` manufacturing progress to done while you hold-craft). |
| **F10** | Quit | Exits the trainer; in-game state is left as-is. |

Both start **OFF**. Toggle them after a save is loaded.

## How it works

The game ships **full unstripped PDBs**. Every address and struct offset used
here was extracted from them — nothing is guessed. See:

- `refs/RE-notes.md` — the offsets, RVAs, and UE object/name layout (build 493833)
- `refs/pdb-extraction.md` — how to re-extract after a game update

At runtime the trainer resolves engine globals against the loaded module bases,
walks `GUObjectArray`, decodes FNames, finds the two target classes, and writes
the cheat fields on a ~20 Hz loop.

## Build

From `src/`:

```bash
dotnet build -c Release      # compile
dotnet test                  # 20 pure-logic tests (chunk/FName/offset math)
```

## Run on Windows

The trainer **must run on the same Windows machine as the game** (it reads the
game process). Build a Windows executable from WSL/Linux:

```bash
cd src/SatisfactoryTrainer
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -o publish
# -> publish/SatisfactoryTrainer.exe  (copy to Windows, run as Administrator)
```

1. Launch Satisfactory and **load a save**.
2. Run `SatisfactoryTrainer.exe` **as Administrator** (needed for
   `OpenProcess` write access).
3. Use F1 / F2 to toggle, F10 to quit.

## Limits / notes

- **Build-locked.** RVAs change every game patch. If the game updates, the
  trainer prints "RVAs likely stale" and exits — re-extract per `refs/`.
- Achievement re-enable holds the flag false each tick; verify in-game that an
  achievement actually fires with AGS on (see `docs/smoke-checklist.md`).
- Single-player only intent. Do not use against other players' sessions.
- Status of each feature is tracked in `ROADMAP.md`. "Done" = confirmed in-game,
  not "code compiles".
