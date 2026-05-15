# Timberborn Research Checklist

Record confirmed game API notes here before changing patches.

## Install Facts

- Game path: record locally during research; do not commit personal absolute paths.
- Game version/build:
- Executable name:
- Runtime/engine evidence:
- Loader/modding framework chosen:

## Files To Inspect

- Game executable and data folder layout.
- Managed assemblies or runtime metadata.
- Existing loader/mod folders, if already installed.
- Save/config locations, using disposable saves only.

## API Notes To Capture

For each planned cheat target, record:

- Assembly name.
- Type name.
- Method or field name.
- Why this hook or data path is safe enough.
- Smoke test that proves behavior changed.

## Local-Only Output

Put large decompile output under:

```text
Timberborn/refs/decompiled/
```

That path is ignored by git through the repo-wide `**/decompiled/` rule.
