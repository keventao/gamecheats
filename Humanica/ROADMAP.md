# Humanica Cheats Roadmap

## Current Stable Baseline - 2026-05-04

Status: stable local build after in-game smoke testing.

Game/runtime:

- Humanica 0.8.18
- Unity 2019.4.41f2
- MelonLoader 0.7.2 Open-Beta
- IL2CPP x64

Confirmed working:

- Time scale x1, x2, x5, x10
- Resource add buttons
- Resource picker and persisted resource slots
- Warehouse x5 auto/manual expansion
- Warehouse high-water resource restore after restart
- Game-save-triggered warehouse snapshot
- Build speed x10
- Production speed x10
- Self-planted crop growth x10
- Own villager movement speed x2/x5

## Warehouse Status

Final accepted behavior:

- No always-on warehouse getter/capacity/free-space Harmony patches.
- `WarehouseCapacityPatch.Register()` remains disabled for runtime hooks.
- Expansion is one-shot only.
- Auto expansion runs once after loading a save when multiplier > x1.
- Manual expansion runs when the player clicks the expansion button.
- Expansion calls the game's own `Inventory.ResizeInventory(int)`.
- Baseline mismatch falls back to the current warehouse pack count.
- Shrink is blocked to prevent item loss.
- Resource snapshot is high-water only.
- Snapshot is saved on manual expansion, auto expansion, and game save.
- Periodic snapshot writes were removed to avoid main-thread config IO and log spam.

Expected log markers:

```text
Save snapshot hook OK (Il2CppHumanica.SaveLoading.SaveLoader, methods=1)
warehouse resource snapshot saved (game-save)
```

Not expected:

```text
warehouse resource snapshot saved (periodic)
```

## Village Status

Implemented:

- Build speed x10
- Production speed x10
- Self-planted crop growth x10
- Own villager movement speed x2/x5

Notes:

- Crop growth targets `ResourceDeposit.AdditionalRecoverySpeed`.
- Crop multiplier is guarded by plant-growing trigger and player-planted checks.
- Movement speed targets `MoveController.CalculateMoveSpeed`.
- Runtime ownership detection was loosened after strict checks returned false for
  player villagers in game logs.

## Known Risks

- Humanica updates may change IL2CPP proxy names or method signatures.
- Warehouse expansion is still experimental by nature; test on disposable saves
  before using a new multiplier on important saves.
- The game save hook currently patches `SaveLoader.StartSave(string)`. If the
  game adds another save path, snapshot triggering may need another hook.
- Unlock tab still needs deeper in-game verification.

## v0.1.1 History

Major changes:

- Rebuilt the GUI around custom-painted IMGUI.
- Replaced fixed resource slots with configurable resource slots.
- Added save backups before warehouse expansion.
- Added manual and auto one-shot warehouse expansion.
- Added high-water snapshot restore for warehouse resources.
- Added self-planted crop growth.
- Added own villager movement speed controls.
- Added lightweight policy tests.

## Next Work

- Keep the stable warehouse workflow unchanged unless new evidence requires it.
- Add a cleaner in-panel status line for save snapshot hook state.
- Add optional warning when the game save hook is not patched.
- Verify unlock behavior.
- Consider reducing startup ResourceIndex dump noise after feature work settles.
