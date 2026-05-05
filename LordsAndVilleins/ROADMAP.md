# Lords & Villeins Cheats Roadmap

Last updated: 2026-05-05

## Current Status

Status: v0.1.1 partially in-game verified.

Game/runtime:

- Lords & Villeins
- Tested game version: 1.6.15
- BepInEx 5.4.21 x64
- HarmonyX 2.10
- Unity Mono

Current code state:

- Plugin entrypoint: `src/LordsAndVilleinsCheats/Plugin.cs`.
- Modules: Economy, Pawn, Time, Build, Royalty.
- Core framework: `ICheatModule`, `ModuleRegistry`, `ModuleStatus`, `GuiManager`, `GameRefs`, `ModConfig`.
- Save backups run on startup for Steam ID save folders with a max keep count of 5.
- Unit tests exist under `src/LordsAndVilleinsCheats.Tests/`.

## Implemented Features

### Loader / Core

- BepInEx plugin id: `com.kk.lav-cheats`.
- Plugin version: `0.1.1`.
- Game version whitelist currently includes `1.6.15`.
- Harmony patch summary is logged at startup.
- Save auto-backup runs before runtime changes.
- BepInEx ConfigurationManager compatibility through config entries.

### UI / Runner

- F1 toggles the IMGUI cheat panel after entering a save.
- The game does not reliably schedule BepInEx-owned `Update`/`OnGUI` callbacks.
- `CheatsRunner` is attached to `GameManager.gameObject` after the game is ready.
- `BootstrapHooks` tracks load/reset lifecycle through `LoadingManager` patches.
- Main menu F1 is not currently supported because the runtime host is not available yet.

### Economy

- `+100000 Money`.
- `+1000 Food`.
- Wood/Stone buttons were removed from v0.1 because personal inventories reject them via `allowedResources`.

### Pawn

- Family-wide pawn controls exist for hunger, health, mood/happiness, and skills.
- Max all skills path uses acquired skill data.
- Needs deeper in-game verification.

### Time

- Speed multiplier override.
- Supports values beyond vanilla max, with caution.

### Build

- Free-building decision path patches `BuildBlueprint.HasResourcesForBlueprint`.
- This skips the resource-check gate but does not fully guarantee material delivery is bypassed.

### Royalty

- Favor points display and direct add buttons:
  - +100
  - +1000
  - +10000
- Writes `RoyaltyManager.instance.favorPoints`, matching the game's own cheat dialogue approach.

## Verified

From the README and smoke checklist:

- Loader: F1 panel toggles after entering a save.
- Patch summary observed as `5/5 ok, 0 broken` on 2026-04-25.
- Economy: Money and Food buttons work.
- Time: speed override applies in real time.
- Royalty: favor points display and add buttons work.
- Test suite expected state: 10 passed, 1 skipped.

Current validation note:

- `dotnet test LordsAndVilleins/src/LordsAndVilleinsCheats.Tests/LordsAndVilleinsCheats.Tests.csproj` requires the game's managed assemblies (`Assembly-CSharp`, `Assembly-CSharp-firstpass`) to be available through the configured `GameRoot`. In a plain checkout without those assemblies, build fails before tests run.

## Needs Smoke Test

Use `docs/smoke-checklist.md`.

Still pending:

- Pawn module:
  - clear hunger
  - clear disease / health
  - max all skills
  - max mood
- Build module:
  - verify vanilla material use with FreeBuilding OFF
  - verify behavior with FreeBuilding ON
- Disable All button and persistence.
- Save/load safety after uninstall.
- Main-menu fallback host if F1 is desired before entering a save.

## Known Risks

- Wood/Stone require a stockpile-aware path or direct storage mutation; personal inventory add calls reject them.
- Money is `ResourceName.Money`, not GoldCoins/SilverCoins/CopperCoins.
- `BaseUnityPlugin.Update/OnGUI` does not fire in this host; runner attachment is game-object dependent.
- F1 toggle must use IMGUI `Event.current` with a frame guard to avoid double toggles.
- Build currently skips only the material-check gate; actual delivery/spend systems may still block construction.
- Pawn mood writes may be overwritten by the game's need/mood recomputation.
- Time multipliers above vanilla max can destabilize AI or simulation.
- Runtime reflection into private inventory structures may break on game updates.

## Next Work

1. Run the pending smoke checklist items for Pawn and Build.
2. Verify Disable All and config persistence.
3. Decide whether to support Wood/Stone via stockpile-aware inventory discovery.
4. Add a real free-building path if material delivery still blocks construction.
5. Improve main-menu hosting if panel access before entering a save is important.
6. Update research notes around Money vs coin resource names.
7. Consider need-buffer locking for stable pawn mood/health behavior.

## Version History

### v0.1.1

- Runner attachment refined for the game host lifecycle.
- Partially in-game verified against version 1.6.15.
- Economy, Time, and Royalty paths verified.

### v0.1.0

- Initial modular BepInEx/Harmony cheat panel.
- Added Economy, Pawn, Time, Build, and Royalty modules.
- Added save backup utility and development scripts.

## Project Links

- `README.md` - install, development, and current limitations.
- `docs/smoke-checklist.md` - manual release checklist.
- `refs/` - game-specific reverse-engineering notes.
- `tools/install.ps1` - install built plugin.
- `tools/tail-log.ps1` - follow BepInEx log.
- `tools/run-and-check.ps1` - automated build/install/launch/log check.
