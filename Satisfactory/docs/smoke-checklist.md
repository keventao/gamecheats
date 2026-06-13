# Smoke Checklist — Satisfactory Trainer

Manual in-game checks. This is the real "done". Run on the **target build
(493833)** on Windows, trainer launched **as Administrator**, save loaded.

## Attach / runtime sanity

- [ ] Trainer finds the process and prints `Attached to process ... (PID ...)`.
- [ ] Prints `UE runtime OK — N objects` with N in the hundreds of thousands.
      (If it says "GUObjectArray looks wrong" → offsets stale for this build.)
- [ ] No "FName decode self-check failed" message.
- [ ] On first toggle, the status line shows non-zero counts, e.g.
      `[Achievements: ON (1)] [Instant Craft: off (0)]`.

## F2 — Instant Manual Craft

- [ ] Stand at a **Craft Bench** (or Equipment Workshop) with a valid recipe and
      enough input items.
- [ ] Press **F2** → status shows `Instant Craft: ON`.
- [ ] Begin hand-crafting (hold the craft button): the item completes
      **immediately** instead of requiring the full hold/crank time.
- [ ] Output item lands in inventory; input items consumed correctly (no
      negative/garbage counts).
- [ ] Press **F2** again → crafting returns to normal speed.
- [ ] Workbench count in the status line is ≥ number of placed benches.

## F1 — Achievement Enable

- [ ] In a save with **Advanced Game Settings ENABLED** (achievements normally
      blocked).
- [ ] Press **F1** → status shows `Achievements: ON (1)` (backend count 1).
- [ ] Trigger an easy, not-yet-earned achievement (e.g. craft/unlock something
      that grants one).
- [ ] Steam achievement notification fires / the achievement shows unlocked.
- [ ] (Sanity) With the cheat OFF and AGS on, the same achievement would NOT
      fire — confirms the cheat is what re-enabled it.

## Stability

- [ ] Leave both cheats ON for several minutes of play — no crash, no stutter
      from the trainer loop.
- [ ] Close the game while trainer runs → trainer prints "Game process exited"
      and stops cleanly.
- [ ] **F10** exits the trainer without crashing the game.

## If something fails

- Wrong/zero object count, or self-check failure → game likely updated.
  Re-extract offsets per `refs/pdb-extraction.md`, update `Offsets.cs`, rebuild.
- Backend/workbench count 0 → class names changed, or no save loaded.
