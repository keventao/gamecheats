#!/usr/bin/env bash
# Static validation for the EasySpawner zh build (no game launch needed).
# Checks:
#   1. Release DLL exists
#   2. All zh translation strings are compiled into the assembly
#   3. The EasySpawnerAssetBundle manifest resource is present and intact (size 19378)
#
# Usage:
#   bash tools/validate-translations.sh                 # build if DLL missing
#   VALHEIM_GAME_ROOT=/path/to/Valheim bash tools/validate-translations.sh
set -euo pipefail

# In case the host lacks libicu (minimal Linux), keep .NET usable.
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT="${DOTNET_SYSTEM_GLOBALIZATION_INVARIANT:-1}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
DLL="$PROJECT_DIR/src/EasySpawner/bin/Release/EasySpawner.dll"

if [[ ! -f "$DLL" ]]; then
    echo "DLL not found, building..."
    if [[ -z "${VALHEIM_GAME_ROOT:-}" ]]; then
        echo "ERROR: VALHEIM_GAME_ROOT is not set." >&2
        exit 1
    fi
    dotnet build "$PROJECT_DIR/src/EasySpawner/EasySpawner.csproj" -c Release
fi

python3 - "$DLL" <<'PYEOF'
import sys, re, struct

path = sys.argv[1]
data = open(path, "rb").read()

strings = [
    "简易生成器",
    "快捷键:",
    "搜索...",
    "数量...",
    "等级...",
    "生成",
    "放入背包",
    "忽略堆叠上限",
    "只看收藏",
    "选项:",
    "打开/关闭:",
    "撤销:",
    "不存在",
    "正在生成 ",
    "已撤销生成 ",
    "个物体",
]

failed = False
for s in strings:
    ok = s.encode("utf-16-le") in data
    print(("  OK  " if ok else " MISS ") + s)
    failed |= not ok

# Manifest resource name exists
res_ok = b"EasySpawnerAssetBundle" in data
print(("  OK  " if res_ok else " MISS ") + "manifest resource name EasySpawnerAssetBundle")
failed |= not res_ok

# AssetBundle payload contains UnityFS signature (bundle magic)
if b"UnityFS" in data:
    print("  OK   AssetBundle payload signature (UnityFS)")
else:
    print(" MISS  AssetBundle payload signature (UnityFS)")
    failed = True

# Exact embedded-resource size:
# CLI resources are stored as: uint32 payloadSize + payload bytes.
# Locate the pattern <uint32 size>+"UnityFS..." where size matches our AssetBundle.
import struct as _st
size = None
magic = b"UnityFS"
start = 0
while True:
    idx = data.find(magic, start)
    if idx == -1:
        break
    if idx >= 4:
        candidate = _st.unpack_from("<I", data, idx - 4)[0]
        if candidate == 19378:
            size = candidate
            break
    start = idx + 1

if size == 19378:
    print(f"  OK   embedded resource size = {size}")
else:
    print(f" MISS  embedded resource size expected 19378, got {size}")
    failed = True

# Exact bundle size check: find resource data region is complex via raw bytes;
# instead assert expected size is 19378 recorded in refs.
print()
print("PASS" if not failed else "FAIL")
sys.exit(1 if failed else 0)
PYEOF
