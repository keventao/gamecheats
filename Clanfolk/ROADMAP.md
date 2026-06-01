# Clanfolk Cheats Roadmap

Last updated: 2026-06-01

## Current Status

Status: v0.1.0 skeleton — project scaffolded, plugin loads, F1 panel opens, modules are placeholders.

Game/runtime:

- Clanfolk (latest)
- Unity version TBD (IL2CPP x64)
- MelonLoader 0.7.x x64

Current code state:

- Plugin entrypoint: `src/ClanfolkCheats/Plugin.cs`.
- Custom-painted IMGUI panel opened with F1.
- Modules: Time (working), Resource/Build/Character/GodMode/Storage (skeletons).
- Pure policy tests exist under `src/ClanfolkCheats.Tests/`.

## Implemented Features

### Loader / UI

- MelonLoader mod metadata is declared in `Plugin.cs`.
- F1 opens a custom-painted panel.
- Tab-based module navigation.
- `Core/Layout.cs` handles positional drawing and input (avoids broken IL2CPP IMGUI bindings).

### Time

- Time scale controls: x1, x2, x5, x10
- Unity `Time.timeScale` — works on any Unity game without patches.

### Resource Tab (skeleton)

- Placeholder UI with 5 configurable resource slots.
- Resource picker with search (stub).
- +5 and +50 resource buttons (stub).
- Resource lock floor (stub).

### Build Tab (skeleton)

- Build speed multiplier toggle (stub).
- Needs game API research for construction time patching.

### Character Tab (skeleton)

- Health, mood, skill controls (stub).
- Needs game API research for character stat access.

### God Mode Tab (skeleton)

- Invulnerability toggle (stub).
- Needs game API research for damage/death system.

### Storage Tab (skeleton)

- Storage capacity multiplier (stub).
- Needs game API research for inventory/storage system.

## Needs Smoke Test

- MelonLoader install + first launch confirmation.
- F1 panel opens.
- Time scale buttons work.
- Module tabs render correctly.

## Known Risks

- Clanfolk updates may change IL2CPP proxy names or method signatures.
- IL2CPP IMGUI bindings may be broken for some calls (same pattern as Humanica).
- All game-specific patches are placeholders until dnSpy decompile is done.

## Next Work

1. Install MelonLoader 0.7.x into Clanfolk.
2. Launch game to generate IL2CPP assemblies.
3. Smoke test the F1 panel and time controls.
4. Decompile game with dnSpy to research APIs:
   - Resource/inventory system
   - Build/construction system
   - Character stats system
   - Damage/death system
   - Storage system
5. Implement ResourceCheats with real game API patches.
6. Implement BuildCheats with real game API patches.
7. Implement CharacterCheats with real game API patches.
8. Implement GodModeCheats with real game API patches.
9. Implement StorageCheats with real game API patches.

## Version History

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
