# Satisfactory — Reverse-Engineering Notes

All offsets/RVAs below were extracted from the **shipping PDBs** that ship with
the game (full, unstripped — `Has Types/Globals/Publics`, `Is stripped: false`).
**Nothing here is guessed.** Re-extract after any game update (see
`pdb-extraction.md`).

## Game build

| Field | Value |
|---|---|
| BuildId | **493833** |
| Engine | Unreal Engine **5.3.2** (custom Coffee Stain fork) |
| Packaging | Modular (per-module DLLs), IoStore (`.ucas`/`.utoc` + `.pak`) |
| Game module | `FactoryGameSteam-FactoryGame-Win64-Shipping.dll` |
| Install (this machine) | `D:\Program Files (x86)\Steam\steamapps\common\Satisfactory` |

> RVAs are **per-module** and **build-specific**. They change on every game
> patch. The struct field offsets are more stable (only change if CSS edits the
> class), but must still be re-verified per build.

## Module map (where each symbol lives)

| DLL | Location | Holds |
|---|---|---|
| `...-CoreUObject-...dll` | `Engine/Binaries/Win64/` | `GUObjectArray`, UObject layout |
| `...-Core-...dll` | `Engine/Binaries/Win64/` | name pool (`NamePoolData`, `GNameBlocksDebug`) |
| `...-FactoryGame-...dll` | `FactoryGame/Binaries/Win64/` | game classes (`UFGWorkBench`, `UOnlineIntegrationBackend`) |

The game is **modular**, so engine globals are NOT in the FactoryGame DLL —
`GUObjectArray` shows there only as an `__imp_` thunk. Resolve each RVA against
its **owning** module's base address at runtime.

## Engine globals (RVAs)

Section math: `RVA = section.VirtualAddress + symbol.offset` (offset printed by
`llvm-pdbutil` is decimal).

### CoreUObject DLL

| Symbol | section:offset | section VA | **RVA** |
|---|---|---|---|
| `GUObjectArray` (`FUObjectArray`) | `0004:267808` | `.data` `0x562000` | **`0x5A3620`** |

### Core DLL

| Symbol | section:offset | section VA | **RVA** |
|---|---|---|---|
| `NamePoolData` (`FNamePool`) | `0004:628928` | `.data` `0x795000` | **`0x82E980`** |
| `GNameBlocksDebug` (`uint8**`) | `0004:144640` | `.data` `0x795000` | **`0x7B8500`** |

`GNameBlocksDebug` holds a pointer to the name pool's `Blocks[]` base — the
simplest entry point for FName decoding.

## UObject runtime layout (CoreUObject PDB, verified)

### `UObjectBase`

| Field | Offset | Type |
|---|---|---|
| vtable | 0 | ptr |
| `ObjectFlags` | 8 | uint32 |
| `InternalIndex` | 12 | int32 |
| `ClassPrivate` | 16 | `UClass*` |
| `NamePrivate` | 24 | `FName` (8 bytes) |
| `OuterPrivate` | 32 | `UObject*` |

`FName` = `{ uint32 ComparisonIndex; uint32 Number; }` → `ComparisonIndex` at
`NamePrivate+0`.

### `FUObjectArray` (at `GUObjectArray`)

| Field | Offset |
|---|---|
| `ObjObjects` (`FChunkedFixedUObjectArray`) | **16** |

### `FChunkedFixedUObjectArray` (at `GUObjectArray+16`)

| Field | Offset | Type |
|---|---|---|
| `Objects` (`FUObjectItem**`, chunk table) | 0 | ptr |
| `PreAllocatedObjects` | 8 | ptr |
| `MaxElements` | 16 | int32 |
| `NumElements` | 20 | int32 |
| `MaxChunks` | 24 | int32 |
| `NumChunks` | 28 | int32 |

`NumElementsPerChunk` is a compile-time constant (not in symbols). **Derive at
runtime**: `elementsPerChunk = MaxElements / MaxChunks` (avoids hardcoding).

### `FUObjectItem` (stride **24**)

| Field | Offset |
|---|---|
| `Object` (`UObject*`) | 0 |
| `Flags` | 8 |
| `ClusterRootIndex` | 12 |
| `SerialNumber` | 16 |
| `RefCount` | 20 |

### Object iteration

```
elementsPerChunk = MaxElements / MaxChunks
for i in [0, NumElements):
    chunkPtr = read_ptr(Objects + (i / elementsPerChunk) * 8)
    item     = chunkPtr + (i % elementsPerChunk) * 24
    obj      = read_ptr(item + 0)   // FUObjectItem.Object
    if obj == 0: continue
```

### FName decode (UE5.3, case-preserving OFF — default)

```
id      = read_u32(obj + 24)              // NamePrivate.ComparisonIndex
blocks  = read_ptr(coreBase + 0x7B8500)   // *GNameBlocksDebug = Blocks[] base
block   = read_ptr(blocks + (id >> 16) * 8)
entry   = block + (id & 0xFFFF) * 2       // stride = 2
header  = read_u16(entry)                 // FNameEntryHeader
bIsWide = header & 1
len     = header >> 6                     // 5-bit probe hash + 10-bit len
str     = read(entry + 2, len[*2 if wide])
```

> The header shift (`>> 6`) assumes `WITH_CASE_PRESERVING_NAME` is OFF (Shipping
> default). The trainer self-verifies on attach by decoding a known class name;
> if it reads garbage it retries with `>> 1` (case-preserving layout).

## Cheat levers (FactoryGame PDB, verified)

### 1. Achievement enable (re-enable Steam achievements with Advanced Game Settings ON)

- Class: **`UOnlineIntegrationBackend`** (field list `0xBEA2`)
- Field: **`bSuppressAchievements`** — `bool` @ **offset 320 (0x140)**
- Lever: force to **0** (and hold it 0 each tick, in case the game re-sets it
  when AGS state changes).
- One live instance expected. Find via GObjects: object whose class name ==
  `OnlineIntegrationBackend`.

### 2. Instant manual craft (Craft Bench / Equipment Workshop finish instantly)

- Class: **`UFGWorkBench`** (component on the manual-craft station; class index
  `0x1343CF`)
- Relevant fields:

  | Field | Offset | Type | Note |
  |---|---|---|---|
  | `mCurrentRecipe` | 600 | ptr | active recipe (null when idle) |
  | `mCurrentManufacturingProgress` | **608 (0x260)** | float | 0..1 |
  | `mManufacturingSpeed` | **612 (0x264)** | float | progress rate multiplier |
  | `mIsProducing` | **628 (0x274)** | bool | true while crafting |
  | `mActiveManufacturingTime` | 640 | float | |
  | `mRecipeDuration` | 724 | float | total craft time |

- Lever options (decide in impl, prefer least destructive):
  - **A:** while `mIsProducing`, write `mManufacturingSpeed` = large constant
    (e.g. 1e6) → each hold tick completes instantly. Restore on toggle off.
  - **B:** while `mIsProducing` && `mCurrentRecipe != 0`, write
    `mCurrentManufacturingProgress` = 1.0 → game finalizes the craft.
- Many instances (one per placed station). Apply to ALL `UFGWorkBench` each tick.

## Cross-checks / open items

- Confirm `bSuppressAchievements` is the flag AGS actually sets (name is
  unambiguous; verify in-game: enable AGS, toggle cheat, confirm an achievement
  fires). No setter/global exists → set directly in code path; runtime hold is
  the robust approach.
- `NumElementsPerChunk` derived, not hardcoded.
- FName header shift self-verified at runtime.
