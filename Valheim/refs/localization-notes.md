# 本地化改动笔记

记录对上游 `Cooleyy/EasySpawner@1.7.0` 的全部改动，便于上游升级后重新套用。

## 改动文件清单（共 3 个源文件 + 工程文件）

### 1. `src/EasySpawner/UI/Style.cs`

- **字体替换**：
  - 原：`averiaSerif` / `averiaSerifBold`（`Resources.FindObjectsOfTypeAll<Font>()` 中找 `AveriaSerifLibre-Regular/Bold`）
  - 新：`cjkFont`，先在已加载字体中按 `PreferredCjkFontNames` 顺序找**动态**字体，找不到则
    `Font.CreateDynamicFontFromOSFont(PreferredCjkFontNames, 16)`
  - 字体回退链：Microsoft YaHei / 微软雅黑 / SimHei / 黑体 / Microsoft JhengHei / PingFang SC / Noto Sans CJK SC / Arial Unicode MS / Arial
  - `ApplyText` 不再区分粗体（动态字体由系统合成样式）
- **新增 `Localize(GameObject root)`**：在 `ApplyAll` 末尾调用，遍历所有 `Text`，按 `TextTranslations` 字典精确匹配（Trim 后匹配）替换。
- 词条表：

  | key（英文原文，Trim 后） | value |
  |---|---|
  | Easy Spawner | 简易生成器 |
  | Hotkeys: | 快捷键: |
  | Search... | 搜索... |
  | Amount... | 数量... |
  | Level... | 等级... |
  | Spawn | 生成 |
  | Put in inventory | 放入背包 |
  | Ignore max stack size | 忽略堆叠上限 |
  | Show favourites only | 只看收藏 |
  | Options: | 选项: |

### 2. `src/EasySpawner/UI/EasySpawnerMenu.cs`

- 三处热键文本前缀：`Spawn:` → `生成:`、`Undo:` → `撤销:`、`Open:` → `打开/关闭:`
- 修饰键经新增的 `TranslateModifier()` 翻译：left/right ctrl → Ctrl、shift → Shift、alt → Alt、command → Win；未命中原样返回
- 键名本身（如 `z`、`=`）不翻译，保持与实际按键一致

### 3. `src/EasySpawner/EasySpawnerPlugin.cs`

三处面向玩家的 `Message`：
- `<prefab> does not exist` → `<prefab> 不存在`
- `Spawning object <prefab>` → `正在生成 <prefab>`
- `Undo spawn of N <name>` → `已撤销生成 N 个 <name>`
  （对应 `objectName` 默认值由 `objects` 改为 `个物体`，实际取被生成物体的去后缀名）

日志 `Debug.Log(...)` 一律保留英文，方便排错。

### 4. 工程/构建（非汉化内容，仅为跨平台构建）

- 原 `EasySpawner.csproj` 为老式非 SDK 格式（依赖 Windows 上 MSBuild + NETFX 工具链），替换为 SDK 风格 csproj（net472 + `Microsoft.NETFramework.ReferenceAssemblies.net472`），Linux/macOS 可构建
- 新增 `Directory.Build.props`（`GameRoot`/`GameManaged`/`BepInExPath`/`PluginInstallDir`，约定环境变量 `VALHEIM_GAME_ROOT`）
- 嵌入资源逻辑名固定为 `EasySpawnerAssetBundle`，资源字节与上游一致（19378 bytes）

## 不需要改的部分

- **prefab 列表名**：`DungeonDBStartPatch` 中
  - 物品：`Localization.instance.Localize(itemDrop.m_itemData.m_shared.m_name)`
  - 可建造物：`Localization.instance.Localize(piece.m_name)`
  - 结果存入 `PrefabState.localizedName`；`PrefabItem.SetName` 显示 `localizedName (itemName)`
  - 游戏语言为中文时自动本地化，搜索也同时匹配英文名与中文名
