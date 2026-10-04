# ZEDZONE Cheats Roadmap

Last updated: 2026-10-05

## Current Status

Status: v0.4.7 in-game verified except weapon durability/ammo-full check.

Game/runtime:

- 隔离区-丧尸末日生存（Steam 1211600），BETA 0.9.99.7
- Unity 2023.1.18f1 IL2CPP x64
- BepInEx 6.0.0-be.733
- net6.0, no Harmony patches (polling + direct calls only)

## Implemented / Verified

- HP lock (`hp→maxhp` per frame, `maxhp` untouched): verified, no one-shot penetration at 1200 HP
- Stamina lock (`energy→maxEnergy`, `fatigue→0`): verified, direction confirmed in-game
- F1 uGUI panel (runtime-built, draggable, dark, CJK via bundled SourceHanSans): verified
- Item browser: 17 categories + search + real ScrollRect + paged add at `stackNumber` via `GameController.AddItemToPlayer`: verified (`via=AddItemToPlayer` in logs)

## Needs Smoke Test

- Added ranged weapons: full durability + full magazine check (v0.4.7 installed, awaiting user test)
- Behavior after a ZED ZONE game update (interop Sandbox: `BepInEx/interop` regenerates; explicit-signature patches only)

## Next (user wishlist, in order discussed)

1. Combat enhance (one-hit / damage scaling — needs Harmony, explicit signatures only; `BasicRangedWeapon.CanFire/Fire`, `BasicMeleeWeapon.OnMeleeAttack`, `BasicCharacterController.OnHitByCharacter` are mapped in `refs/api.md`)
2. ESP/assist (enemy/item/vehicle markers)
3. Split `src/` into `Core/` + `Modules/` per repo layout; add xUnit policy tests + `tools/tail-log.ps1`

## Dead Ends (do not retry without new evidence)

- Jim97 ScriptTrainer v1.0.3: dead on current game version (Harmony ambiguous `RefreshTotalItemWeight` overload kills whole plugin load; static IL fix trips ConfuserEx anti-tamper in module `.cctor`). Kept as reference sample only.
- IMGUI panel: `FlexibleSpace`/`ScrollView`/`GUIStyle.font` setter stripped in this build; OS dynamic fonts stripped (`CreateDynamicFontFromOSFont` → `TypeLoadException`).
- `AddComponent<RectTransform>` on objects that already have a Transform returns null; construct UI objects with `NewGO(name)` (ctor with `Il2CppType.Of<RectTransform>()`).
