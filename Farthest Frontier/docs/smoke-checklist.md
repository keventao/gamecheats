# Farthest Frontier Smoke Checklist

Use a disposable save.

## Fast Villagers

1. Build `KKFastVillagersPlugin_FF.dll`.
2. Confirm the DLL exists in:
   `<FARTHEST_FRONTIER_MONO_ROOT>\Plugins\`.
3. Launch the Mono game executable.
4. Confirm MelonLoader loads `KK Fast Villagers` under `Loading Plugins`.
5. Confirm log lines:
   - `[KK Fast Villagers] patched Character.Awake`
   - `[KK Fast Villagers] patched Character.get_movementSpeed`
   - `[KK Fast Villagers] patched TransportWagon.Awake`
6. Confirm no `[KK Fast Villagers] WARN:` lines.
7. Load a settlement.
8. Watch several villagers walk between home, work, and storage.
9. Confirm villager speed is faster than vanilla and pathing remains stable.
10. Build or inspect transport wagons.
11. Confirm wagons move faster and still complete pickup/dropoff tasks.
12. Change `UserData/MelonPreferences.cfg` values, restart, and confirm the new
    values are logged.
