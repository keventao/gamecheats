# RimWorld 在 Linux 上无法显示中文的排查与解决

> 测试环境：Bazzite Linux（ostree/composefs 只读系统）、KDE Plasma（Wayland + XWayland）、NVIDIA 显卡、Steam 官方客户端、RimWorld **原生 Linux 版**（Unity 2022.3.35f1）。
> 现象：Steam 启动游戏后，主菜单/设置里的**中文全部空白**（不是方框，也不是乱码，而是完全没有字），英文与数字正常。

---

## 一、结论速览（TL;DR）

- RimWorld 原生 Linux 版被 Steam 放进 **pressure-vessel 容器**（scout-on-soldier）运行。
- Unity 在 Linux 上**枚举系统字体时只硬编码扫描 `/usr/share/fonts` 一个目录**（`UnityEngine.Font::GetPathsToOSFonts`）。
- 容器内的 `/usr/share/fonts` 是 soldier 运行时提供的**只读精简目录，里面只有 DejaVu 字体**；宿主机的全套字体只挂在容器的 `/run/host/fonts`，Unity **不会扫描那里**。
- DejaVu 不含中文字形，内嵌的 Arial 也只有拉丁字符 → 中文渲染为空白。
- **fontconfig 配置（`~/.config/fontconfig`）、`~/.fonts`、`~/.local/share/fonts` 全部无效**，因为该 Unity 版本动态字体根本不经过系统 fontconfig。
- **最终解决办法：绕过 Steam 容器，直接在宿主机运行 `RimWorldLinux`**，此时 `/usr/share/fonts` 含完整 CJK 字体，中文立即正常。Steam 仍检测到游戏运行，**Auto Cloud 云存档照常同步**。

---

## 二、关键事实与证据

### 1. 游戏是原生 Linux 版，不是 Proton/Wine

游戏目录包含原生二进制与启动脚本：

```
RimWorldLinux              # 原生 ELF 可执行文件
UnityPlayer.so
start_RimWorld.sh          # 官方启动器
steam_appid.txt
```

官方启动器最后一行是：

```sh
LC_ALL=C ./$GAMEFILE "$@"
```

因此 Wine 的 `winetricks corefonts` 那类方案不适用。

### 2. 语言设置本身是对的

配置文件 `~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Config/Prefs.xml`：

```xml
<langFolderName>ChineseSimplified (简体中文)</langFolderName>
```

语言包以 tar 形式存放，游戏选语言后加载：

```
Data/Core/Languages/ChineseSimplified (简体中文).tar
```

`Player.log` 也显示简体中文翻译数据被正常加载，**没有语言文件缺失或字体报错**。所以问题不在“语言包”，而在“字形渲染”。

### 3. Unity 在 Linux 上只扫描 `/usr/share/fonts`

从 `UnityPlayer.so` 提取的关键符号/字符串：

```
UnityEngine.Font::GetPathsToOSFonts
UnityEngine.Font::Internal_CreateFontFromPath
/usr/share/fonts        ← 唯一硬编码的字体目录
```

没有任何 fontconfig / freetype 的初始化痕迹。

### 4. 进程根本不加载 fontconfig / freetype

对游戏进程做检查：

- `LD_DEBUG=libs` 完整库加载日志中，**从未尝试加载 `libfontconfig` 或 `libfreetype`**。
- `/proc/<pid>/maps` 中也找不到这两个库。

这证明改 fontconfig 配置对它无效。

### 5. 容器 vs 宿主机：字体文件对比（决定性差异）

**直接在宿主机启动（中文正常）**，Unity 打开的字体文件：

```
/usr/share/fonts/wqy-microhei-fonts/wqy-microhei.ttc        ← 文泉驿微米黑（首选）
/usr/share/fonts/google-noto-sans-cjk-fonts/NotoSansCJK-Regular.ttc
/usr/share/fonts/google-droid-sans-fonts/DroidSansFallbackFull.ttf
/usr/share/fonts/liberation-sans-fonts/LiberationSans-Regular.ttf
```

**Steam 容器内启动（中文空白）**，Unity 只能打开：

```
/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf
```

容器 `mountinfo` 关键挂载：

```
.../SteamLinuxRuntime_soldier/var/tmp-XXXXXX/usr  →  /usr
/usr/share/fonts                                  →  /run/host/fonts        (ro, composefs)
~/.local/share/fonts                             →  /run/host/user-fonts   (ro)
```

即：宿主机字体在容器里只存在于 `/run/host/...`，而 Unity 死盯 `/usr/share/fonts`（容器侧只有 DejaVu）。

---

## 三、尝试过但无效的办法（避坑）

| 办法 | 为什么无效 |
|---|---|
| 改 `~/.config/fontconfig/conf.d/*.conf`，把 Arial → Noto Sans SC | 命令行 `fc-match` 显示正确，但 Unity 不调用 fontconfig |
| 把字体放进 `~/.local/share/fonts` | 容器内该目录可见，但 Unity 只扫 `/usr/share/fonts` |
| 把字体放进老式 `~/.fonts` | 同上，Unity 不扫描 |
| `<dir>/run/host/fonts</dir>` 让 fontconfig 扫描宿主机目录 | 该目录在容器内属主是 `65534(nobody)`，扫描时触发异常，游戏直接崩溃（assert dump） |
| 启动选项 `PRESSURE_VESSEL_OFFLOAD=1 %command%` | 变量加在容器内部命令上，时机太晚，offload 不生效 |
| `PRESSURE_VESSEL_FILESYSTEMS_RW=/usr/share/fonts` | 容器内 `/usr/share/fonts` 挂载点已被 soldier 占用，绑定被忽略 |
| 往 soldier 运行时 `files/share/fonts` 塞字体 | 容器 `/usr` 是按运行时清单在 `var/tmp-XXXXXX/usr` **重建的临时副本**，手动加进 `files/` 的文件被忽略，且每次启动临时目录都会重新生成，无法持久 |

---

## 四、最终解决方案：宿主机直接启动

### 1. 启动脚本 `~/.local/bin/rimworld`

```bash
#!/bin/bash
# 绕过 Steam pressure-vessel 容器，在宿主机直接启动，中文正常。

GAME_DIR="$HOME/.local/share/Steam/steamapps/common/RimWorld"
GAME_BIN="RimWorldLinux"

cd "$GAME_DIR" || { echo "找不到游戏目录: $GAME_DIR"; exit 1; }
[ -x "$GAME_BIN" ] || chmod +x "$GAME_BIN" 2>/dev/null

# 中文输入法（可选）
export GTK_IM_MODULE=fcitx
export QT_IM_MODULE=fcitx
export XMODIFIERS=@im=fcitx

nohup env LC_ALL=C ./"$GAME_BIN" >/dev/null 2>&1 &
echo "已启动 (pid $!)"
```

### 2. 桌面快捷方式

```ini
[Desktop Entry]
Type=Application
Name=环世界 RimWorld
Comment=直接在宿主机启动 RimWorld（中文正常，绕过 Steam 容器）
Exec=/home/你的用户名/.local/bin/rimworld
Icon=rimworld
Terminal=false
Categories=Game;
StartupNotify=true
```

赋予可执行权限：

```bash
chmod +x ~/.local/bin/rimworld ~/Desktop/rimworld.desktop
```

### 3. 云存档仍然有效

- 游戏进程仍通过 `steamclient.so` 与 Steam 通信，`Player.log` 中有：

```
[S_API] SteamAPI_Init(): Loaded '.../Steam/linux64/steamclient.so' OK.
SteamInternal_SetMinidumpSteamID: Caching Steam ID: 76561198... [API loaded no]
```

- Steam 客户端会正常显示 RimWorld “运行中”。
- RimWorld 使用 **Auto Cloud（autocloud）**，由 Steam 客户端自动同步 `Saves/` 目录，与启动路径无关。
- 退出游戏后留意 Steam 的“同步中 / 同步完成”提示即可确认。

存档目录：

```
~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Saves/
```

---

## 五、如果不想绕过容器的备选思路

1. **改用 Proton 运行 Windows 版**：Steam → RimWorld 属性 → 兼容性 → 勾选强制使用特定 Steam Play 兼容层（选最新 Proton）。Windows 版字体处理路径不同，通常可直接显示中文，代价是牺牲原生版。
2. 等待 Unity / Steam Runtime 更新动态字体枚举逻辑（根本性修复，但不可控）。

---

## 六、排查命令速查

```bash
# 系统字体
fc-list | grep -iE "wenquanyi|wqy|noto.*cjk"

# 某字符被哪些字体覆盖（如“环” U+73AF）
fc-list :charset=73af

# fontconfig 匹配结果
fc-match Arial
fc-match "sans-serif:lang=zh"

# 游戏进程是否加载 fontconfig/freetype
awk '{print $6}' /proc/$(pgrep -x RimWorldLinux)/maps | grep -iE "fontconfig|freetype"

# 追踪库加载过程
LD_DEBUG=libs ./RimWorldLinux 2>lddebug.txt
grep -iE "fontconfig|freetype" lddebug.txt

# 容器视角下的字体目录
ls /proc/$(pgrep -x RimWorldLinux)/root/usr/share/fonts/

# 游戏日志
less ~/.config/unity3d/Ludeon\ Studios/RimWorld\ by\ Ludeon\ Studios/Player.log
```

---

*记录日期：2026-09-28*
