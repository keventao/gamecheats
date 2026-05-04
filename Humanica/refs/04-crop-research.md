# Crop Growth Research

## Goal

Speed up only player-planted crops, not wild plants or generic resources.

## IL2CPP Symbols Found

- `ResourceDeposit.RecoverySpeed(int)`
- `ResourceDeposit.AdditionalRecoverySpeed(int)`
- `ResourceDeposit.PlantGrowingStartTrigger(int)`
- `ResourceDeposit.HasPlantGrowingTrigger()`
- `ResourceDeposit.GetGrowProgress()`
- `Nature.Planted`
- `NatureManager.AddPlant(Entity)`
- save data field `NatureSaveData.hasPlantGrowingTrigger`

## Safety Rule

Never apply the multiplier to every grow-progress resource. The patch must fail closed unless it can confirm both:

- the target has a plant-growing trigger
- the associated nature object is `Planted == true`

If the runtime patch cannot resolve the resource deposit to a planted nature object, keep the UI warning visible and do not broaden the multiplier.

## Implementation Note

Prefer patching growth/recovery speed methods such as `RecoverySpeed` or `AdditionalRecoverySpeed`. Treat `GetGrowProgress` as a fallback only, because it may represent display/current progress rather than the rate of growth.

## 2026-05-04 Runtime Result

- Implemented and verified through the Village tab toggle.
- The active runtime patch uses `ResourceDeposit.AdditionalRecoverySpeed`.
- Multiplier is x10.
- The patch stays guarded by:
  - plant-growing trigger check
  - player-planted nature check
- Smoke result from the user: acceleration features are working.
