# Clanfolk Cheats Roadmap

Last updated: 2026-06-02

## Current Status

Status: v0.1.1 — framework enhanced, Resource module has real implementation, tests in place.

Game/runtime:

- Clanfolk (latest)
- Unity version TBD (IL2CPP x64)
- MelonLoader 0.7.x x64

Current code state:

- Plugin entrypoint: `src/ClanfolkCheats/Plugin.cs`.
- Custom-painted IMGUI panel opened with F1.
- Modules: Time (working), Resource (real implementation, untested), Build/Character/GodMode/Storage (skeletons with UI).
- xUnit tests for ReflectAccessor (13 passing).
- Utilities: ReflectAccessor, SaveBackup, ModConfig.

## Implemented Features

### Loader / UI

- MelonLoader mod metadata declared in `Plugin.cs`.
- F1 opens a custom-painted panel.
- Tab-based module navigation.
- `Core/Layout.cs` handles positional drawing and input.
- `Core/GameRefs.cs` provides world-ready detection and manager access.

### Time

- Time scale controls: x1, x2, x5, x10
- Unity `Time.timeScale` — works on any Unity game without patches.

### Resource Tab

- 5 configurable resource slots with item picker (searchable).
- SpawnItem / SpawnEntity fallback chain for giving items.
- Custom quantity with scroll-wheel input.
- Lock floor per slot (UI ready; enforcement needs resource query API).
- "Fill All Slots" button.
- Auto-discovers item names from EntityManager(Item) when world is loaded.

### Build Tab (skeleton)

- Instant build toggle (not patched).
- Free build toggle (not patched).
- Build speed display (not patched).

### Character Tab (skeleton)

- Health lock toggle (not patched).
- Mood lock toggle (not patched).
- No aging toggle (not patched).

### God Mode Tab (skeleton)

- Invulnerability toggle (not patched).
- No starvation toggle (not patched).
- No freezing toggle (not patched).

### Storage Tab (skeleton)

- Capacity multiplier display (not patched).
- Stack size multiplier display (not patched).

## Utilities

- `Core/ReflectAccessor.cs` — name-based reflection get/set with caching (ported from 轮回修仙路).
- `Util/SaveBackup.cs` — recursive save directory backup with prune.
- `Util/ModConfig.cs` — JSON-based persistent settings store.

## Needs Smoke Test

- MelonLoader install + first launch confirmation.
- F1 panel opens.
- Time scale buttons work.
- Module tabs render correctly.
- Resource: item discovery, spawning, picker, lock floor.
- GameRefs.IsReady detects world properly.

## Known Risks

- Clanfolk updates may change IL2CPP proxy names or method signatures.
- IL2CPP IMGUI bindings may be broken for some calls (same pattern as Humanica).
- All game-specific patches are placeholders until dnSpy decompile is done.
- Resource spawn methods (SpawnItem/SpawnEntity) are based on common Unity patterns — may need adjustment.

## Next Work

1. Install MelonLoader 0.7.x into Clanfolk (on Windows).
2. Launch game to generate IL2CPP assemblies.
3. Smoke test the F1 panel, time controls, and resource module.
4. Decompile game with dnSpy to research APIs:
   - Resource/inventory query system (for lock floor enforcement)
   - Build/construction system
   - Character stats system
   - Damage/death system
   - Storage system
5. Implement real Harmony patches for Build, Character, GodMode, Storage modules.
6. Add `OnUpdate` enforcement for resource lock floors.
7. Add Harmony-powered health/mood/starvation locking.

## Version History

### v0.1.1

Framework enhancements:

- Ported `ReflectAccessor` from 轮回修仙路 for safer IL2CPP reflection.
- Added `SaveBackup` utility for save directory backup.
- Added `ModConfig` for JSON persistent settings.
- Enhanced `ResourceCheats`: custom quantity input, lock floor per slot, "Fill All Slots".
- Enhanced `GameRefs` with actual GameManager.Instance detection.
- Converted tests to xUnit (13 ReflectAccessor tests passing).
- Updated all skeleton modules with toggle UI placeholders.

### v0.1.0

Initial skeleton:

- Project structure with Directory.Build.props, csproj, .gitignore.
- MelonLoader plugin entrypoint with F1 panel.
- 6 tab modules (Time working, 5 skeletons).
- Custom IMGUI layout helpers.
- Lightweight test project.
- docs/smoke-checklist.md placeholder.

## Project Links

- `README.md` - install, development, and IL2CPP notes.
- `docs/smoke-checklist.md` - manual in-game verification checklist.
- `refs/` - Clanfolk-specific research notes (empty — needs dnSpy decompile).
