# For The King Cheats Roadmap

Last updated: 2026-05-05

## Current Status

Status: v0.1.0 project skeleton implemented; in-game smoke testing still pending.

Game/runtime:

- For The King
- Unity Mono
- BepInEx 5 x64
- Harmony

Current code state:

- BepInEx plugin entrypoint exists: `src/ForTheKingCheats/Plugin.cs`.
- F1 IMGUI panel is hosted through `CheatsRunner` on a `DontDestroyOnLoad` GameObject.
- Modules are registered through `ModuleRegistry`.
- Lightweight no-game policy tests exist under `src/ForTheKingCheats.Tests/`.

## Implemented Features

### Loader / UI

- BepInEx plugin id: `com.kk.ftk-cheats`.
- Plugin version: `0.1.0`.
- F1 toggles the cheat panel.
- Panel lists each registered module and status.
- Panel is draggable through `GUI.Window` / `GUI.DragWindow`.

### Time

- `TimeCheats` exposes buttons:
  - x1
  - x2
  - x5
  - x10
  - Reset
- Implementation writes `UnityEngine.Time.timeScale` directly.

### Players

- `Heal party to full` scans `CharacterStats` objects and heals player characters to `MaxHealth`.
- `Lock party HP` patches `CharacterStats.SetSpecificHealthRPC` and blocks incoming lower HP values for player characters.
- Healing is allowed while HP lock is enabled.

## Verified

- Static review of core modules and tests completed on 2026-05-05.
- Lightweight policy tests cover:
  - HP lock blocks damage.
  - HP lock allows healing.
  - Damage is allowed when lock is disabled.

## Needs Smoke Test

Use `docs/smoke-checklist.md`.

- Build succeeds in the local game environment.
- BepInEx loader files are present next to `FTK.exe`.
- Plugin log contains `For The King Cheats ready.`
- F1 opens/closes the panel in-game.
- Time x2/x5/x10 visibly changes gameplay speed without log errors.
- Reset returns time scale to x1.
- Heal party works on active player characters.
- Lock party HP prevents damage but does not block healing.

## Known Risks

- Runtime player-character detection relies on `m_CharacterOverworld.m_FTKPlayerID.IsPlayer()`.
- `Time.timeScale` can affect animations, AI, and physics globally.
- GUI uses standard Unity IMGUI; if the game host does not dispatch plugin `OnGUI`, the runner approach may need adjustment.
- Gold and Lore controls are intentionally not active until runtime storage is researched.

## Next Work

1. Run the smoke checklist in-game.
2. Confirm BepInEx 5 x64 install path and plugin load log.
3. Research safe gold/lore storage access.
4. Add tests for time-scale bounds or future policy helpers.
5. Add install verification notes after first successful in-game test.

## Project Links

- `README.md` - build/install quickstart.
- `docs/smoke-checklist.md` - manual test checklist.
- `refs/00-research-checklist.md` - research notes.
