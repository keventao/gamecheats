# Smoke Checklist - Football Manager 26 Trainer

Manual checks. Use a disposable save.

## Build

- [ ] `dotnet test FM26Trainer.slnx` passes from `Football Manager 26/src`.
- [ ] `dotnet run --project FM26Trainer/FM26Trainer.csproj -- targets` prints the three requested target families.

## Attach

- [ ] Launch Football Manager 26.
- [ ] Load a disposable save.
- [ ] Run `dotnet run --project FM26Trainer/FM26Trainer.csproj -- probe`.
- [ ] It prints the target process id and executable path.
- [ ] It either prints `Attach OK` or a Mach return code to investigate.

## Manual read/write once addresses are known

- [ ] Use `read <address> <size>` on a known readable address.
- [ ] Confirm returned bytes are stable across repeated reads.
- [ ] Use `write <address> <bytes> --yes` only on a confirmed target field.
- [ ] Confirm the in-game value changes and no unrelated value changes.

## Requested cheats, later

- [ ] Unlimited fitness keeps player condition high through time advance and match simulation.
- [ ] No injuries clears current injuries and prevents/clears newly generated injuries.
- [ ] Players always happy keeps morale/happiness/training happiness high after time advance.
- [ ] Trainer exit leaves the game stable.
- [ ] Closing the game while trainer runs exits cleanly or reports process exit cleanly.

