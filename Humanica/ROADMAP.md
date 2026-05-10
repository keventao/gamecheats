# Humanica Cheats Roadmap

Last updated: 2026-05-05

## Current Status

Status: v0.1.1 stable local build after in-game smoke testing; warehouse workflow accepted as conservative and one-shot only.

Game/runtime:

- Humanica 0.8.18
- Unity 2019.4.41f2
- MelonLoader 0.7.2 Open-Beta
- IL2CPP x64

Current code state:

- Plugin entrypoint: `src/HumanicaCheats/Plugin.cs`.
- Custom-painted IMGUI panel opened with F1.
- Modules: Time, Resource, Village, Unlock.
- Pure policy tests exist under `src/HumanicaCheats.Tests/`.

## Implemented Features

### Loader / UI

- MelonLoader mod metadata is declared in `Plugin.cs`.
- F1 opens a custom-painted panel.
- Standard `GUILayout`, `GUI.Window`, `GUI.Button` return values, `GUI.Toggle` return values, and `GUI.TextField` return values are intentionally avoided because this game's generated IL2CPP IMGUI bindings are unreliable.
- `Core/Layout.cs` handles positional drawing and input.

### Time

- Time scale controls:
  - x1
  - x2
  - x5
  - x10

### Resource Tab

- 5 configurable resource slots.
- Resource picker with search.
- `+5` and `+50` resource buttons.
- Resource lock floor.
- Warehouse multiplier controls:
  - x1
  - x2
  - x5
  - x10

### Warehouse Expansion

Final accepted behavior:

- No always-on warehouse getter/capacity/free-space Harmony patches.
- `WarehouseCapacityPatch.Register()` remains disabled for runtime hooks.
- Expansion is one-shot only.
- Manual expansion runs when the player clicks the expansion button.
- Auto expansion runs once after loading a save when multiplier > x1.
- Expansion calls the game's own `Inventory.ResizeInventory(int)`.
- Baseline mismatch falls back to the current warehouse pack count.
- Shrinking is blocked to prevent item loss.
- Resource snapshot is high-water only.
- Snapshot is saved on manual expansion, auto expansion, and game save.
- Periodic snapshot writes were removed to avoid main-thread config IO and log spam.

### Village Tab

- Build speed x10.
- Production speed x10.
- Self-planted crop growth x10.
- Own villager movement speed x2/x5.

Implementation notes:

- Crop growth targets `ResourceDeposit.AdditionalRecoverySpeed`.
- Crop multiplier is guarded by plant-growing trigger and player-planted checks.
- Movement speed targets `MoveController.CalculateMoveSpeed`.
- Runtime ownership detection was loosened after strict checks returned false for player villagers in game logs.

### Unlock Tab

- Research unlock entry point exists.
- Deeper in-game verification is still needed.

## Verified

Confirmed working in local smoke testing:

- Time scale x1/x2/x5/x10.
- Resource add buttons.
- Resource picker and persisted resource slots.
- Warehouse x5 auto/manual expansion.
- Warehouse high-water resource restore after restart.
- Game-save-triggered warehouse snapshot.
- Build speed x10.
- Production speed x10.
- Self-planted crop growth x10.
- Own villager movement speed x2/x5.

Expected stable log markers:

```text
Save snapshot hook OK (Il2CppHumanica.SaveLoading.SaveLoader, methods=1)
warehouse resource snapshot saved (game-save)
```

Not expected:

```text
warehouse resource snapshot saved (periodic)
```

Lightweight policy tests cover crop growth, villager movement speed, warehouse auto-expansion, baseline inference, and warehouse snapshot high-water behavior.

## Needs Smoke Test

- Unlock tab behavior.
- Save snapshot hook status should be visible in panel, not only logs.
- Behavior after a Humanica game update.
- Warehouse multipliers other than the already-tested x5 should be tested only on disposable saves.

## Known Risks

- Humanica updates may change IL2CPP proxy names or method signatures.
- Warehouse expansion is experimental by nature; test on disposable saves before using a new multiplier on important saves.
- The game save hook currently patches `SaveLoader.StartSave(string)`. If the game adds another save path, snapshot triggering may need another hook.
- `ResourceCheats` is large and owns UI, prefs, resource operations, warehouse expansion, save hooks, and snapshot restore state. Future feature work would benefit from splitting it.
- Startup `ResourceIndex` dump can be noisy now that resource discovery is stable.

## Next Work

1. Keep the stable warehouse workflow unchanged unless new evidence requires it.
2. Add a cleaner in-panel status line for save snapshot hook state.
3. Add an optional warning when the game save hook is not patched.
4. Verify unlock behavior.
5. Consider reducing startup `ResourceIndex` dump noise after feature work settles.
6. Consider extracting warehouse expansion/snapshot controller code out of `ResourceCheats`.
7. Add edge-case tests for warehouse snapshot parse/format malformed pairs and duplicate IDs.

## Version History

### v0.1.1

Major changes:

- Rebuilt the GUI around custom-painted IMGUI.
- Replaced fixed resource slots with configurable resource slots.
- Added save backups before warehouse expansion.
- Added manual and auto one-shot warehouse expansion.
- Added high-water snapshot restore for warehouse resources.
- Added self-planted crop growth.
- Added own villager movement speed controls.
- Added lightweight policy tests.

## Project Links

- `README.md` - install, development, and IL2CPP notes.
- `docs/smoke-checklist.md` - manual in-game verification checklist.
- `refs/` - Humanica-specific research notes.
