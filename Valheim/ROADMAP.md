# Valheim / EasySpawner 汉化 — Roadmap

Last updated: 2026-09-26

汉化 fork，非自研 mod。核心逻辑全部来自上游，只动「显示层」。

## Current status

**v1.7.0-zh.1（基于上游 1.7.0，对应 Valheim 1.0.x）**

| 项 | 状态 |
|---|---|
| UI 静态文字汉化（AssetBundle 内 prefab 文本） | ✅ 运行时替换，无需重建 AssetBundle |
| 热键提示汉化（Open/Spawn/Undo + 修饰键名） | ✅ |
| 游戏内 HUD/居中提示汉化 | ✅ |
| 中文字体（动态字体，OS 字体回退链） | ✅ |
| Release 构建通过、嵌入资源完整 | ✅ |
| 静态翻译校验脚本 | ✅ |
| 游戏内排版/字宽人工复核 | ⏳ 待用户确认 |

## 汉化实现方式（为什么不直接改 AssetBundle）

1. **静态文字**：上游 prefab 文本在 AssetBundle 内。重建 AssetBundle 需要匹配版本的 Unity Editor，成本高且易错。改为在 `Style.Localize()` 中于菜单实例化后按文本精确匹配替换，仅增加一个字典，不触碰二进制资源。
2. **热键文字**：本就由 `EasySpawnerMenu.CreateMenu()` 用代码拼接，直接改代码；修饰键（left ctrl 等）经 `TranslateModifier()` 转成 Ctrl/Shift/Alt。
3. **字体**：`Style.FindFonds()` 原取游戏内 Averia Serif Libre（无 CJK 字形）。改为优先从已加载字体中找动态中文字体（微软雅黑等），找不到则 `Font.CreateDynamicFontFromOSFont` 在运行时向操作系统取字体。
4. **物品/怪物名**：无需处理。`DungeonDBStartPatch` 已通过 `Localization.instance.Localize` 取当前语言名，游戏设中文即中文。

## Known limitations

- 中文文本比英文短或长，个别固定宽度控件可能出现留白或截断；发现后需要调整 prefab 布局（届时评估是否转为重建 AssetBundle 方案）。
- 仅保证简体中文环境；繁体只做了字体回退，未做文案翻译。
- 不翻译 mod 配置文件（BepInEx config）中的描述项，仅 UI 与游戏内提示。
- 依赖 Windows 系统字体存在（微软雅黑 / 黑体）；非 Windows 用户需自行安装 CJK 字体。

## Upstream sync

- 上游：https://github.com/Cooleyy/EasySpawner ，tag 基线 `1.7.0`。
- 保留上游源码目录命名与结构；同步时 merge 上游后，重新套用三类改动：
  1. `UI/Style.cs`（字体 + `Localize` 字典）
  2. `UI/EasySpawnerMenu.cs`（热键文案 / `TranslateModifier`）
  3. `EasySpawnerPlugin.cs`（三处玩家提示）
- 上游若新增 UI 文本，需在 `Style.TextTranslations` 补词条。

## Version history

- **1.7.0-zh.1** (2026-09-26)：首个汉化版本，跟随上游 1.7.0。
