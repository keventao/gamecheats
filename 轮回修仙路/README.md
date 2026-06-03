# 轮回修仙路 Cheats

BepInEx 6 IL2CPP cheat panel for *轮回修仙路* (Steam AppID 1993150), an open-world 3D xianxia (修仙) Unity game.

> Personal use first. Framework ready; modules require reverse-engineering before implementation.

## Game Info

- **Path:** `<STEAM>\steamapps\common\轮回修仙路`
- **Engine:** Unity IL2CPP (confirmed: `GameAssembly.dll` present)
- **Save Location:** `%AppData%\..\LocalLow\烟水寒\轮回修仙路\GameDataSave_Steam\` (JSON-based)
- **Existing Mod Ecosystem:** BepInEx 6 IL2CPP English localization mod proven working

## Status

**v0.0.1 — framework ready, awaiting reverse-engineering**

- ✅ IL2CPP engine confirmed
- ✅ BepInEx 6 IL2CPP project scaffold (Plugin, GuiManager, ModuleRegistry, CheatsRunner)
- ✅ Save backup utility
- ✅ Time module placeholder (Unity `Time.timeScale`)
- ⏳ Game assembly reverse-engineering (dnSpy/Il2CppDumper)
- ⏳ Player stats module (lifespan, spirit root, realm)
- ⏳ Inventory module
- ⏳ Combat/god mode module

## Install

### Step 1 — Install BepInEx 6 IL2CPP

BepInEx 6 IL2CPP x64 (build 755) is included in `tools/BepInEx-Unity.IL2CPP-win-x64.zip`.
The installer verifies the bundled archive before extraction:

- `tools/BepInEx-Unity.IL2CPP-win-x64.zip`
  - SHA256: `3616d6a67f5f595973ec4aa7bd7edaf7f799d5bb9926f7146a6dcc7b4abf478f`

Do not replace this archive without updating the checksum and recording the
source/version in this section.

**一键安装：**
```powershell
powershell tools/install-bepinex.ps1 -GameRoot "<STEAM>\steamapps\common\轮回修仙路"
```

**手动安装：** 解压 `tools/BepInEx-Unity.IL2CPP-win-x64.zip`，把 `winhttp.dll`、`.doorstop_version`、`doorstop_config.ini`、`BepInEx/` 文件夹放到游戏根目录（跟游戏 `.exe` 同目录）。

### Bundled trainer provenance

`《轮回修仙路》基础功能库1.0.4_BepInEx版.zip` is a third-party compatibility
reference package used by `tools/try-trainer-current.ps1`. The script prints
its SHA256 before use:

- SHA256: `03beffe324e6dfde5e2da64ce003995f0e0a8c26be2c46a0c9b16b75b23319ab`

Prefer the sandbox mode. Direct real-install testing with `-NoCopy` refuses to
overwrite existing loader artifacts unless `-AllowRealInstallOverwrite` is also
passed; existing artifacts are moved to a timestamped backup first.

### Step 2 — 首次启动游戏

从 Steam 启动游戏一次，BepInEx 会自动生成 IL2CPP 代理程序集，然后退出。这是正常的。

### Step 3 — 编译并安装修改器

在 **Windows PowerShell** 里运行：

```powershell
cd "你的项目路径\gamecheats\轮回修仙路"
powershell tools\build-and-install.ps1
```

或者手动编译：

```powershell
$env:LUNHUI_GAME_ROOT = "<STEAM>\steamapps\common\轮回修仙路"
cd src/LunHuiCheats
dotnet build -c Release
powershell ..\..\tools\install.ps1
```

### Step 4 — 启动游戏

从 Steam 启动游戏，按 **P**（默认）打开修改面板。

## Development

```bash
cd src/LunHuiCheats
dotnet build -c Release
powershell ../../tools/install.ps1
powershell ../../tools/tail-log.ps1   # in separate terminal
```

Run tests:
```bash
dotnet test ../LunHuiCheats.Tests/LunHuiCheats.Tests.csproj
```

## Layout

- `src/LunHuiCheats/` — mod source
  - `Core/` — Plugin, ICheatModule, ModuleRegistry, GuiManager, ModConfig, CheatsRunner, GameRefs
  - `Modules/` — cheat modules (Time, Player, Cultivation, etc.)
  - `Util/` — HarmonyHelpers, SaveBackup
- `src/LunHuiCheats.Tests/` — xUnit tests (no game required)
- `tools/` — `install.ps1`, `tail-log.ps1`
- `refs/` — reverse-engineering notes and checklists
- `docs/smoke-checklist.md` — manual in-game verification

## Reverse-Engineering Priority

See `refs/00-research-checklist.md`.

1. Dump `global-metadata.dat` with Il2CppDumper
2. Identify game manager, player stats, inventory, cultivation, combat types
3. Verify method/field names before writing patches
4. Mark unverified names with "占位,待确认"

## Cheat Scope

### Planned Runtime Modules
- Time speed control (placeholder already working)
- Player stats (HP, lifespan 寿元, spirit root 灵根, realm 境界)
- God mode / no damage
- Instant alchemy/crafting
- Cultivation speed multiplier
- NPC relationship editing
- Inventory add/remove

## Safety

- Save auto-backup runs on every plugin startup (max 5 backups kept).
- Test destructive behavior on disposable saves.

## License

Personal use, no warranty.
