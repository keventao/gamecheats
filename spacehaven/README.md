# Space Haven 存档修改器

## 目录结构

```
spacehaven/
├── mac/
│   └── SpaceHavenEditor.app   ← 双击运行（macOS）
├── windows/
│   ├── 运行.bat                ← 双击运行（Windows，需 Python）
│   ├── editor.py
│   ├── extract_names.py
│   ├── resource_names.json
│   └── README.txt
└── README.md
```

## macOS 使用

双击 `mac/SpaceHavenEditor.app`。

首次打开可能被 Gatekeeper 拦截（未签名）。两种解法：

1. 右键 .app → **打开** → 弹窗点**打开**
2. 或在终端解除隔离：
   ```bash
   xattr -dr com.apple.quarantine "mac/SpaceHavenEditor.app"
   ```

工具会自动扫描以下存档位置：

- `~/<APP_SUPPORT>/Spacehaven/savegames/`（Mac 原生安装）
- `~/<APP_SUPPORT>/compatibility layer/...`（compatibility layer/Wine）

## Windows 使用

见 `windows/README.txt`。需先安装 Python 3.10+（勾选 Add to PATH），再双击 `运行.bat`。

## 功能

- **Bank**：信用币 / 科研点 / 建造点
- **Crew**：每位船员的血量/饥饿/心情/属性/14 项技能
- **Resources**：玩家船上所有存储物资（含武器/药品），一键填满或单独设值，共 182 种条目来自 `spacehaven.jar` 实时抽取

## 安全

- 每次保存前自动备份原存档为 `game.bak-时间戳`
- 修改前**必须退出游戏**（文件锁）
- 崩档把备份改名回 `game` 即可

## 更新资源名表

游戏升级后 `elementaryId → 名称` 映射可能变。重生成：

```bash
# macOS
python3 extract_names.py /path/to/spacehaven.jar

# Windows
python extract_names.py "<SPACEHAVEN_GAME_ROOT>\spacehaven.jar"
```
