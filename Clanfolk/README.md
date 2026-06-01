# Clanfolk Cheats

MelonLoader + HarmonyX cheat panel for Clanfolk.

The panel is opened with F1. UI rendering is fully custom-painted IMGUI because
the generated IL2CPP Unity IMGUI bindings may not reliably return values from
several standard GUI calls.

## Current Status

Tested with:

- Clanfolk (latest)
- Unity (version TBD — needs MelonLoader install to confirm)
- MelonLoader 0.7.x x64
- Game type: IL2CPP x64

Current features:

- Time scale: x1, x2, x5, x10
- Resource tab: skeleton — needs game API research
- Build tab: skeleton — needs game API research
- Character tab: skeleton — needs game API research
- God Mode tab: skeleton — needs game API research
- Storage tab: skeleton — needs game API research

## Install

1. Install MelonLoader 0.7.x x64 into the game folder so `version.dll` sits next
   to `Clanfolk.exe`.
2. Launch the game once so MelonLoader generates IL2CPP proxy assemblies under
   `<game>/MelonLoader/Il2CppAssemblies/`.
3. Build the mod:

```bash
dotnet build Clanfolk/src/ClanfolkCheats/ClanfolkCheats.csproj -c Release /p:GameRoot="<CLANFOLK_GAME_ROOT>"
```

4. Copy or let the build target copy `ClanfolkCheats.dll` into:

```text
<game>/Mods/
```

5. Restart the game and press F1.

## Development

Pass `GameRoot` explicitly or set `CLANFOLK_GAME_ROOT`:

```bash
export CLANFOLK_GAME_ROOT=<CLANFOLK_GAME_ROOT>
dotnet build Clanfolk/src/ClanfolkCheats/ClanfolkCheats.csproj -c Release
```

Run lightweight policy tests:

```bash
dotnet run --project Clanfolk/src/ClanfolkCheats.Tests/ClanfolkCheats.Tests.csproj
```

## Project Layout

- `src/ClanfolkCheats/` - mod source
- `src/ClanfolkCheats/Core/` - GUI, module registry, shared helpers
- `src/ClanfolkCheats/Modules/` - cheat modules and pure policy helpers
- `src/ClanfolkCheats.Tests/` - no-game lightweight policy tests
- `refs/` - IL2CPP research notes (decompiled game types)
- `docs/smoke-checklist.md` - manual in-game verification checklist
- `ROADMAP.md` - current status and version notes

## IL2CPP IMGUI Notes

Avoid these APIs in this game until verified working:

- `GUILayout.*`
- `GUI.Window`
- `GUI.Button` return value
- `GUI.Toggle` return value
- `GUI.TextField` return value

Use positional `GUI.Box` / `GUI.Label`, then handle input with `Event.current`
and `Rect.Contains`. The helper code lives in `Core/Layout.cs`.

## Safety

- Test experimental behavior on disposable saves first.
- Do not commit save files, game binaries, generated loader files, logs, or backups.
