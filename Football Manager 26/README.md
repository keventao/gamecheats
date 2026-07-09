# Football Manager 26 Trainer

Mac external realtime trainer workspace for **Football Manager 26**.

Scope for this project is runtime memory editing only:

- no save editing
- no game file modification
- no DLL injection
- no DRM or ownership bypass work

## Target observed locally

- App: `/Users/zhuangtao/Library/Application Support/Steam/steamapps/common/Football Manager 26/fm.app`
- Bundle id: `com.SportsInteractive.FootballManager26`
- Game version observed: `26.3.2.2329565`
- Engine observed: Unity `6000.0.52f1-fm26-05f1`
- Runtime: Unity IL2CPP, universal Mach-O (`x86_64` + `arm64`)

## Requested cheats

| Cheat | Status | Current evidence |
|---|---|---|
| Unlimited fitness | Research target identified | CE table calls `improveTeamCondition(..., 10000, ...)` and exposes `ptrPlayer + plao.Popc` / `ptrPlayer + plao.Pftg`. Live address resolution is pending. |
| No injuries | Research target identified | CE table exposes `removeTeamInjuries` and `removePlayerInjuries`. Function body is encoded in the CE table; live equivalent is pending. |
| Players always happy | Research target identified | CE table exposes morale/happiness fields and `removeTeamUnhappiness` / `maxTeamTrainingHappiness`. Live address resolution is pending. |

The code in `src/` is the Mac realtime memory I/O base. It can find the FM26
process, try to attach through Mach APIs, and manually read/write explicit
addresses. It does not yet claim the three requested toggles are complete.

## Build / test

From this project:

```bash
cd src
dotnet test FM26Trainer.slnx
```

## Runtime probes

Print target notes:

```bash
dotnet run --project FM26Trainer/FM26Trainer.csproj -- targets
```

Find the running game and try to get a task port:

```bash
dotnet run --project FM26Trainer/FM26Trainer.csproj -- probe
```

Read explicit bytes after a successful attach:

```bash
dotnet run --project FM26Trainer/FM26Trainer.csproj -- read 0x12345678 16
```

Write explicit bytes after a successful attach:

```bash
dotnet run --project FM26Trainer/FM26Trainer.csproj -- write 0x12345678 00 00 --yes
```

`write` requires `--yes` on purpose. Until the live FM26 pointer chain is
resolved, all writes must be explicit and manually verified.

## Known limits

- macOS may deny `task_for_pid` against a hardened runtime game process. The
  `probe` command reports the exact Mach return code.
- CE table offsets are symbolic (`ptrClub`, `ptrPlayer`, `ptrPerson`,
  `plao.*`, `pero.*`) and must be resolved for the Mac build before safe
  one-click toggles exist.
- "Done" for a cheat means verified in a disposable in-game save, not only that
  code compiles.

