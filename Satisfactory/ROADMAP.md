# Satisfactory Trainer — Roadmap / Status

Target build: **493833** (UE 5.3.2). "Done" = confirmed in-game by the user.

## Status legend
- ✅ done & verified in-game
- 🟡 code complete, **awaiting in-game confirmation**
- ⬜ not started

## Features

| Feature | Status | Notes |
|---|---|---|
| PDB-sourced offsets (no guessing) | ✅ | `refs/RE-notes.md`, verified via `llvm-pdbutil` |
| Memory I/O layer (RPM/WPM/modules) | ✅ | `ProcessMemory` |
| UE5.3 object iteration + FName decode | 🟡 | `UnrealRuntime`; self-verifies layout on attach, but only proven once run against the live game |
| Achievement Enable (F1) | 🟡 | `bSuppressAchievements`→0; **needs in-game check that an achievement fires with AGS on** |
| Instant Manual Craft (F2) | 🟡 | snaps `UFGWorkBench` progress; **needs in-game check at a Craft Bench** |
| Pure-logic unit tests | ✅ | 20 passing (chunk/FName/offset math) |

## Next (in-game verification — only the user can do this)

1. `dotnet publish -r win-x64 --self-contained` → copy exe to Windows.
2. Launch game, load save, run exe as Admin.
3. Confirm on attach: prints object count (~hundreds of thousands) and non-zero
   backend/workbench counts.
4. F2 at a Craft Bench: hold-craft completes instantly.
5. F1 with Advanced Game Settings enabled: an achievement unlocks.
6. Report results → flip 🟡 to ✅ or file findings in `refs/`.

## Possible later work (not requested)
- ⬜ Auto-detect build & refuse mismatched offsets more gracefully
- ⬜ Config file for hotkey remap
- ⬜ "Instant craft via speed multiplier" alternative lever (option A in refs)

## Known limits
- RVAs are build-locked; re-extract on every game patch (`refs/pdb-extraction.md`).
- Requires Administrator (OpenProcess write).
- Single-player intent only.
