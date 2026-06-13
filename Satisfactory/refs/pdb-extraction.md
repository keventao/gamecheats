# PDB symbol extraction (how the offsets in RE-notes.md were found)

Satisfactory ships **full unstripped PDBs** next to every DLL. They contain
every class layout, field offset, and global RVA. This is the authoritative
source — never guess offsets, extract them.

## Tool: `llvm-pdbutil`

`pretty` mode needs DIA (Windows-only). The **native `dump` mode works on
Linux** and is all we need.

### Getting it without sudo (WSL/Kali)

```bash
cd /tmp && mkdir -p llvmdl && cd llvmdl
apt-get download llvm-18 libllvm18          # no root needed
dpkg -x llvm-18_*.deb extracted
dpkg -x libllvm18_*.deb extracted
# wrapper:
cat > /tmp/pdb.sh <<'EOF'
#!/bin/bash
export LD_LIBRARY_PATH=/tmp/llvmdl/extracted/usr/lib/x86_64-linux-gnu:/tmp/llvmdl/extracted/usr/lib/llvm-18/lib
exec /tmp/llvmdl/extracted/usr/lib/llvm-18/bin/llvm-pdbutil "$@"
EOF
chmod +x /tmp/pdb.sh
/tmp/pdb.sh --version    # Debian LLVM version 18.1.8
```

## Recipes

Paths (this machine):

```
G="/mnt/d/Program Files (x86)/Steam/steamapps/common/Satisfactory"
FG="$G/FactoryGame/Binaries/Win64/FactoryGameSteam-FactoryGame-Win64-Shipping.pdb"
CORE="$G/Engine/Binaries/Win64/FactoryGameSteam-Core-Win64-Shipping.pdb"
CU="$G/Engine/Binaries/Win64/FactoryGameSteam-CoreUObject-Win64-Shipping.pdb"
```

### Confirm a PDB is usable

```bash
/tmp/pdb.sh dump --summary "$FG"   # want: Has Types/Globals/Publics, Is stripped: false
```

### Find a class's field offsets

```bash
# 1. find the COMPLETE class record index (last match = full def, not fwd ref)
/tmp/pdb.sh dump -types "$FG" 2>/dev/null \
  | grep -aE "LF_CLASS \[size = [0-9]+\] \`UFGWorkBench\`"
# 2. dump its members with offsets
/tmp/pdb.sh dump -types -type-index=0x1343CF -dependents "$FG" 2>/dev/null \
  | grep -aE "LF_MEMBER"
```

### Find which class owns a member (member name → class)

```bash
# member -> field-list index (streaming, single pass)
/tmp/pdb.sh dump -types "$FG" 2>/dev/null | awk '
  /LF_FIELDLIST \[/ { match($0,/0x[0-9A-Fa-f]+/); fl=substr($0,RSTART,RLENGTH) }
  /bSuppressAchievements/ { print "FIELDLIST="fl; exit }'
# field-list index -> owning class (note: ref printed as "field list: 0xXXXX")
/tmp/pdb.sh dump -types "$FG" 2>/dev/null | awk '
  /\| LF_(CLASS|STRUCTURE) \[/ { cls=$0 }
  /field list: 0x[Bb][Ee][Aa]2/ { print cls; exit }'
```

### Find a global's RVA

```bash
# section:offset
/tmp/pdb.sh dump -globals --global-name=GUObjectArray "$CU" 2>/dev/null | grep -A1 GUObjectArray
# section virtual addresses
/tmp/pdb.sh dump -section-headers "$CU" 2>/dev/null | grep -E "name|virtual address"
# RVA = sectionVA + offset   (offset is DECIMAL)
```

### Discover symbol names by substring

```bash
/tmp/pdb.sh dump -publics "$FG" 2>/dev/null \
  | grep -aoE "[A-Za-z_]*Achievement[A-Za-z_]*" | sort -u
```

## Notes

- The FactoryGame PDB is ~281 MB; `dump -types` streams for ~1–3 min. Use
  `timeout 540` and pipe straight into `grep`/`awk` — never materialize the full
  dump to disk.
- `llvm-pdbutil` is a throwaway tool kept in `/tmp` — not committed.
