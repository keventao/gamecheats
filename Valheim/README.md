# EasySpawner 汉化版 (Valheim / BepInEx mod)

对第三方开源 mod [Cooleyy/EasySpawner](https://github.com/Cooleyy/EasySpawner)（MIT）的简中本地化 fork：游戏内任意预制体（物品 / 敌人 / 特效等）生成器，按 `/` 打开菜单。

> 本仓库只做本地化与中文字体适配，不改变 mod 的生成逻辑。

## Status

**v1.7.0-zh.1 — based on upstream 1.7.0 (Valheim 1.0.x)**

- ✅ UI 固定文字全部汉化（标题、搜索、数量、等级、生成、放入背包、忽略堆叠上限、只看收藏、快捷键提示）
- ✅ 游戏内提示汉化（不存在 / 正在生成 / 已撤销生成）
- ✅ 中文字体：原 Averia Serif Libre 无汉字，替换为系统动态中文字体（微软雅黑 → 黑体回退链），不出现方块
- ✅ 物品 / 怪物名称无需翻译：上游已调用游戏 `Localization.Localize()`，游戏语言为中文时自动显示「中文名 (PrefabName)」
- ⏳ 进游戏排版复核中（如个别文本被截断需调 prefab）

详见 `ROADMAP.md`。

## 汉化对照

| 原文 | 译文 |
|---|---|
| Easy Spawner | 简易生成器 |
| Search... / Amount... / Level... | 搜索... / 数量... / 等级... |
| Spawn | 生成 |
| Put in inventory | 放入背包 |
| Ignore max stack size | 忽略堆叠上限 |
| Show favourites only | 只看收藏 |
| Open/Close / Spawn / Undo | 打开/关闭 / 生成 / 撤销 |
| `<prefab> does not exist` | `<prefab> 不存在` |
| `Spawning object <prefab>` | 正在生成 `<prefab>` |
| `Undo spawn of N <name>` | 已撤销生成 N 个 `<name>` |

## Install

1. 已安装 [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/)。
2. 自行构建（见下），或直接取构建产物 `EasySpawner.dll`。
3. 将 `EasySpawner.dll` 放到 `BepInEx/plugins/EasySpawner/` 目录下（原版 DLL 先备份或删除）。
4. 启动游戏，进入存档后按 `/`（小键盘 `/` 亦可）呼出菜单。

## Build

游戏程序集路径不写死，通过环境变量 / MSBuild 属性传入：

```bash
# Windows PowerShell
$env:VALHEIM_GAME_ROOT = "D:\SteamLibrary\steamapps\common\Valheim"
dotnet build src\EasySpawner\EasySpawner.csproj -c Release

# Linux / WSL
VALHEIM_GAME_ROOT="/mnt/d/SteamLibrary/steamapps/common/Valheim" \
  dotnet build src/EasySpawner/EasySpawner.csproj -c Release
# 也可用 -p:GameRoot="..." 覆盖
```

产物：`src/EasySpawner/bin/Release/EasySpawner.dll`（UI AssetBundle 已内嵌）。

一键安装到游戏：

```powershell
powershell tools/install.ps1 -GameRoot "D:\SteamLibrary\steamapps\common\Valheim"
```

## Validate

无需启动游戏的静态检查（校验所有汉化字符串仍在程序集内、内嵌资源完整）：

```bash
bash tools/validate-translations.sh
```

## Layout

- `src/EasySpawner/` — 汉化后的 mod 源码（保留上游目录结构，便于同步上游）
- `EasySpawnerAssetBundle` — 上游 UI AssetBundle，作为嵌入资源编译（未改动）
- `tools/install.ps1` — 复制 DLL 到 `BepInEx/plugins/EasySpawner/`
- `tools/validate-translations.sh` — 构建后静态校验汉化与资源
- `refs/localization-notes.md` — 汉化改动点与字体方案说明
- `docs/smoke-checklist.md` — 游戏内人工检查清单
- `ROADMAP.md` — 状态、已知问题、上游同步计划

## Tested game version

Valheim `1.0.x`（Unity 2022.3.x / Mono 后端，BepInEx 5）。游戏大版本更新可能导致上游 patch 失效，与本汉化无关。

## License

继承上游 [MIT License](LICENSE)，Copyright (c) 2021 Cooleyy。本地化改动同协议发布。
