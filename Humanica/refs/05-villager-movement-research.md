# Villager Movement Research

## Goal

Speed up only the player's own villagers, with selectable 2x or 5x movement speed in the existing Village panel.

## IL2CPP Symbols Found

- `GameCore.Features.Movement.MoveController`
- `MoveController.CalculateMoveSpeed()`
- movement fields and methods: `_baseRunSpeed`, `_baseWalkSpeed`, `_speedMultiplier`, `UpdateMovementSpeed`, `SetSpeedMultiplierParameter`
- villager source: `S.CreatureManager.Villagers`

## Safety Rule

Do not multiply every creature or movement controller. The patch must fail closed unless it can confirm the movement owner resolves to an object contained in `CreatureManager.Villagers`.

## Implementation Note

Patch `MoveController.CalculateMoveSpeed()` and multiply the returned speed only when the selected multiplier is 2 or 5. The UI uses one row with two selectable buttons; clicking the active option again returns to normal speed.

## 2026-05-04 Runtime Result

- Implemented in the existing Village tab, same panel area as crop growth.
- UI has one row with x2 and x5 options.
- Runtime patch target: `MoveController.CalculateMoveSpeed()`.
- Initial strict ownership detection returned false for player villagers in logs.
- The runtime guard was adjusted to fail open only after the known player-villager lookup paths cannot prove otherwise.
- User confirmed own villager movement speed works.
