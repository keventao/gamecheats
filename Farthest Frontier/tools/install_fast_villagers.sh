#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
game_root="${1:-<FARTHEST_FRONTIER_GAME_ROOT>/Farthest Frontier (Mono)}"
project="$root/src/FastVillagers/FastVillagers.csproj"
plugins_dir="$game_root/Plugins"
dll="$root/src/FastVillagers/bin/Release/KKFastVillagersPlugin_FF.dll"
target="$plugins_dir/KKFastVillagersPlugin_FF.dll"

dotnet build "$project" -c Release --no-restore \
  "/p:GameRoot=$game_root" \
  "/p:PluginInstallDir=$plugins_dir"

if [[ ! -f "$target" ]]; then
  mkdir -p "$plugins_dir"
  cp -f "$dll" "$target"
fi

ls -l "$target"
