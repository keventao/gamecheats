# Fast Villagers Research

## Source Reference

Reference repo:

```text
https://github.com/Krasipeace/FF-GameMods/tree/main/FastVillagers
```

Reference behavior:

- `Character.Awake` postfix:
  - `_shoeBonusBase = VillagerSpeed`
  - `_turningSpeed = VillagerSpeed * 50`
- `TransportWagon.Awake` postfix:
  - `_movementSpeed = WagonMoveSpeed`
  - `_turningSpeed = WagonMoveSpeed * 50`
  - `carryCapacity = WagonCarryCapacity`

Reference defaults:

- `VillagerSpeed = 1.0`
- `WagonMoveSpeed = 8.0`
- `WagonCarryCapacity = 400.0`

## Local Build Observed

From `MelonLoader/Latest.log`:

```text
MelonLoader v0.7.0 Open-Beta
Game Type: MonoBleedingEdge
Game Arch: x64
Runtime Type: net35
Unity Version: 2022.3.62f3
Game Version: v1.1.1b (Mono)
```

Mono game root is provided locally through `FARTHEST_FRONTIER_GAME_ROOT`.

## Current Assembly Findings

Inspected:

```text
Farthest Frontier_Data/Managed/Assembly-CSharp.dll
```

Relevant `Character` members still exist:

```text
TYPE Character base=CEMonoBehaviour
FIELD System.Single _movementSpeedBase
FIELD System.Single _movementSpeedBaseRun
FIELD System.Single _shoeBonusBase
FIELD System.Single _turningSpeed
METHOD System.Void Awake()
METHOD System.Single get_movementSpeed()
METHOD System.Single get_turningSpeed()
```

Relevant `Villager` members:

```text
TYPE Villager base=Character
METHOD System.Void Awake()
METHOD System.Single GetCarryCapacity()
```

Relevant `TransportWagon` members still exist:

```text
TYPE TransportWagon base=CEMonoBehaviour
FIELD System.Single carryCapacity
FIELD System.Single _movementSpeed
FIELD System.Single _turningSpeed
METHOD System.Void Awake()
METHOD System.Single get_movementSpeed()
METHOD System.Single get_turningSpeed()
METHOD System.Void CalculateCarryCapacity()
METHOD System.Single GetCarryCapacity()
```

## Compatibility Choice

Compile against MelonLoader and Harmony only. Resolve game types by name at
runtime:

- `AccessTools.TypeByName("Character")`
- `AccessTools.TypeByName("TransportWagon")`

Then set fields by reflection. This avoids stale direct references if the game
assembly changes, and makes missing members visible in the MelonLoader log.

The patch filters `Character.Awake` to runtime type `Villager`, instead of
blindly applying villager speed fields to all `Character` subclasses.

2026-05-15 update: the first default test value is intentionally obvious:
`VillagerMoveSpeedMultiplier = 10.0`. After in-game confirmation, lower it to
`3.0`.

2026-05-15 result: x10 was confirmed in-game. Default and local preferences were
lowered to x3.

2026-05-15 follow-up: the observed run did not load `KKFastVillagers_FF.dll`.
The next build also patches `Character.get_movementSpeed`, so the multiplier is
applied when the game queries speed instead of only during `Awake`.

2026-05-15 Steam follow-up: Steam Workshop sync for app `1044720` rebuilds
`Farthest Frontier (Mono)/Mods` from subscribed workshop folders:

```text
workshop/content/1044720/3672460924/Pangu.dll
workshop/content/1044720/3697995619/ForageableTransplantation.dll
workshop/content/1044720/3699319030/FFModSettingsManager.dll
workshop/content/1044720/3700248692/FFAutomation.dll
workshop/content/1044720/3700248711/FFEnableAchievements.dll
workshop/content/1044720/3702895739/AddItemMono.dll
workshop/content/1044720/3712359812/ManifestDelivery.dll
```

Local DLLs placed only under `Mods` are removed on the next Steam launch. The
local speed patch installs as a MelonLoader plugin under `Plugins/`.
