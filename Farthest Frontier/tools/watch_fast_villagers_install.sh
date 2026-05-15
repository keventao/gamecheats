#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
game_root="${1:-${FARTHEST_FRONTIER_GAME_ROOT:-}}"
plugins_dir="$game_root/Plugins"
source_dll="$root/src/FastVillagers/bin/Release/KKFastVillagersPlugin_FF.dll"
target_dll="$plugins_dir/KKFastVillagersPlugin_FF.dll"
seconds="${2:-120}"

if [[ -z "$game_root" ]]; then
  echo "usage: $0 <FARTHEST_FRONTIER_MONO_ROOT> [seconds]" >&2
  echo "or set FARTHEST_FRONTIER_GAME_ROOT" >&2
  exit 2
fi

if [[ ! -f "$source_dll" ]]; then
  "$root/tools/install_fast_villagers.sh" "$game_root"
fi

mkdir -p "$plugins_dir"
echo "watching $target_dll for ${seconds}s"

end=$((SECONDS + seconds))
while (( SECONDS < end )); do
  if [[ ! -f "$target_dll" ]] || ! cmp -s "$source_dll" "$target_dll"; then
    cp -f "$source_dll" "$target_dll"
    echo "installed $(date '+%H:%M:%S')"
  fi
  sleep 0.5
done

ls -l "$target_dll"
