#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
game_root="${1:-<FARTHEST_FRONTIER_GAME_ROOT>/Farthest Frontier (Mono)}"
plugins_dir="$game_root/Plugins"
source_dll="$root/src/FastVillagers/bin/Release/KKFastVillagersPlugin_FF.dll"
target_dll="$plugins_dir/KKFastVillagersPlugin_FF.dll"
seconds="${2:-120}"

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
