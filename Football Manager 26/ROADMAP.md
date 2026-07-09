# Football Manager 26 Trainer - Roadmap / Status

Target observed locally: **26.3.2.2329565**, Unity IL2CPP on macOS.

## Status legend

- done: implemented and verified in-game
- code: implemented, needs live game confirmation
- research: target evidence found, implementation pending
- todo: not started

## Features

| Feature | Status | Notes |
|---|---|---|
| Local app fingerprint | done | Bundle id, version, Unity/IL2CPP layout, hardened runtime noted in `refs/local-app-and-ce-notes.md`. |
| CE table target mapping | research | Three requested target families identified from extracted CT entries. |
| Mac process discovery | code | Finds likely FM26 process by name/path. |
| Mach memory I/O | blocked | Wrapper exists, but live FM26 returns `task_for_pid` kern return `5` as user and root. |
| CLI probe/read/write | code | Manual commands only; no fake cheat toggles. |
| External debugger attach | blocked | `/usr/bin/lldb -p <fm pid>` reports "Not allowed to attach to process." |
| BepInEx IL2CPP on macOS | blocked | BepInEx v6 docs state IL2CPP builds are only for Windows/Wine. |
| Unlimited fitness | research | Need live `ptrPlayer`/`ptrClub` address resolution and value verification. |
| No injuries | research | CE helper body is encoded; need equivalent Mac-side write plan or runtime call path. |
| Players always happy | research | Need live happiness/morale address resolution and max-value verification. |

## Next

1. Run `dotnet test` for the trainer base.
2. Launch FM26, load a disposable save, run `probe`.
3. Decide whether to re-sign a local FM26 app copy/original with debug allowance,
   accepting that this modifies the app signature and may break Steam updates.
4. If debug allowance works, resolve live pointers for club/person/player
   objects on the Mac IL2CPP build.
5. Verify one field at a time with `read` and `write --yes` in a disposable save.
6. Only after verified addresses exist, add the three user-facing toggles.

## Later, only if needed

- Minimal native menu or tray UI for the three toggles.
- Per-version address cache in ignored `addresses.local.json`.
- Hotkeys once the memory targets are proven.
