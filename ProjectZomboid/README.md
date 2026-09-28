# Project Zomboid 在 Linux 上挂 Mod 后中文变方块的排查与解决

> 测试环境：Bazzite Linux（ostree/composefs 只读系统）、KDE Plasma（Wayland + XWayland）、NVIDIA 显卡、Steam 官方客户端、Project Zomboid **原生 Linux 版**（appid 108600，B42，自带 JRE + LWJGL）。
> 现象：**不挂任何创意工坊 Mod 时中文完全正常**；一旦启用 Mod（尤其是含中文目录名的汉化 Mod），**整个界面的中文全部变成方块（▯▯▯）**。

---

## 一、结论速览（TL;DR）

- Steam / 官方启动脚本把 **`LC_ALL=C`** 注入了游戏进程，覆盖掉系统的 UTF-8 locale。
- 在 C locale 下，JVM 的文件路径编码 `sun.jnu.encoding` 退化为 **ASCII**。
- 当创意工坊 Mod 的**目录名含中文**（典型：`More Traits 简中汉化`）时，JVM 无法编码该路径，抛出：

  ```
  java.nio.file.InvalidPathException: Malformed input or input contains unmappable characters:
  .../workshop/content/108600/2672560941/mods/More Traits ▒▒▒▒
  ```

- 该异常导致 Mod 文件系统事务连环失败、字体贴图加载失败 → **整个界面中文变方块**。
- **最终解决办法：在 Steam「启动选项」里强制使用 UTF-8 locale 与 JVM 编码**，变量直接注入游戏进程，无论从 Steam 还是图标启动都生效。

---

## 二、关键事实与证据

### 1. 游戏是原生 Linux 版

游戏目录包含原生启动脚本与自带 64 位 JRE：

```
projectzomboid.sh                 # 官方启动脚本
projectzomboid/ProjectZomboid64   # 原生可执行文件
projectzomboid/jre64/bin/java     # 自带 JRE
```

`projectzomboid.sh` 关键片段：

```sh
if "${INSTDIR}/jre64/bin/java" -version > /dev/null 2>&1; then
    export PATH="${INSTDIR}/jre64/bin:$PATH"
    XMODIFIERS= LD_PRELOAD="...:libPZXInitThreads64.so" ./ProjectZomboid64 "$@"
fi
```

### 2. 语言设置本身正确

`~/Zomboid/options.ini`：

```ini
language=CN
```

游戏自带完整中文位图字体（AngelCode `.fnt` + PNG 贴图），位于：

```
projectzomboid/media/fonts/CN/{1x,2x,3x,4x}/zomboid{Small,Medium,Large}Chinese.fnt
projectzomboid/media/fonts/CH/...
```

不挂 Mod 时中文显示正常，证明语言包与字体文件都没问题。

### 3. 决定性证据：游戏进程里是 `LC_ALL=C`

查看运行中游戏进程的环境变量：

```bash
PID=$(ps -eo pid,comm | awk '$2=="ProjectZomboid6"{print $1}')
tr '\0' '\n' < /proc/$PID/environ | grep -E "LANG|LC_"
```

输出（注意最后一行）：

```
LANG=zh_CN.UTF-8
LC_ADDRESS=zh_CN.UTF-8
...
LC_NUMERIC=zh_CN.UTF-8
LC_ALL=C                ← 罪魁祸首，优先级最高，覆盖一切
```

### 4. 日志里的 InvalidPathException

游戏 DebugLog（`~/Zomboid/Logs/*_DebugLog.txt`）中：

```
ERROR ... ZomboidFileSystem.getAllModFoldersAux> Exception thrown
java.nio.file.InvalidPathException: Malformed input or input contains unmappable characters:
  .../workshop/content/108600/2672560941/mods/More Traits ▒▒▒▒
    at UnixPath.encode(...)
```

随后连带出现大量字体贴图加载失败：

```
ERROR ... AngelCodeFont.parseFnt> AngelCodeFont failed to load page 0 texture
  .../media/fonts/CN/2x/zomboidSmallChinese_00.png
```

中文目录名对应的 workshop id `2672560941` 即「More Traits 简中汉化」。

---

## 三、走过的弯路（避坑）

排查初期一度误判为 **PNG 贴图格式问题**，请注意不要重蹈覆辙：

| 怀疑点 | 现象 | 为什么是误判 |
|---|---|---|
| 中文 PNG 是 `palette(P) + tRNS` 索引透明格式 | 把单个 PNG 用 PIL 转成标准 RGBA 后，那一次启动恰好没报错 | 那次"成功"是因为**当时没挂触发问题的 Mod**，与格式无关 |
| 批量把 CN/CH 下 963 个 PNG 全转 RGBA | 转换后挂 Mod 仍然方块 | 已转成 RGBA 的文件依然 "failed to load"，证明根因不在贴图编码 |
| 某个 Mod 替换了字体表 | Mod 目录里没找到字体覆盖 | Mod 只是触发了字体重载；真正失败的是路径编码导致的文件系统异常 |

**判断口诀**：只要日志里出现 `InvalidPathException ... unmappable characters` + 进程里 `LC_ALL=C`，就是 locale/编码问题，不要再去折腾 PNG。

---

## 四、最终解决方案：Steam 启动选项强制 UTF-8

### 1. 设置启动选项

Steam 库 → 对 **Project Zomboid** 点右键 → **属性** → **启动选项 / Launch Options**，填入：

```
LC_ALL=zh_CN.UTF-8 LANG=zh_CN.UTF-8 JAVA_TOOL_OPTIONS="-Dsun.jnu.encoding=UTF-8 -Dfile.encoding=UTF-8" %command%
```

各项作用：

- `LC_ALL=zh_CN.UTF-8`：覆盖官方脚本的 `LC_ALL=C`，让整个进程使用 UTF-8。
- `LANG=zh_CN.UTF-8`：兜底设置语言。
- `-Dsun.jnu.encoding=UTF-8`：**关键**，强制 JVM 用 UTF-8 编码文件路径（解析中文 Mod 目录名）。
- `-Dfile.encoding=UTF-8`：强制 JVM 默认字符编码为 UTF-8。
- `%command%`：Steam 占位符，代表游戏原本的启动命令，必须保留在末尾。

设置后直接在 Steam 点「开始」即可，启用含中文目录名的 Mod 也不再方块。

### 2. 验证生效

启动后再次检查进程环境与编码，确认 `LC_ALL` 已变为 UTF-8：

```bash
PID=$(ps -eo pid,comm | awk '$2=="ProjectZomboid6"{print $1}')
tr '\0' '\n' < /proc/$PID/environ | grep -E "LC_ALL|JAVA_TOOL"
```

游戏内确认：

- 主菜单 / 设置中文正常；
- MODS 列表里汉化类 Mod 正常加载；
- 进入游戏世界后中文仍正常。

### 3. 云存档不受影响

该方案只改环境变量，游戏仍由 Steam 正常启动，`SteamAPI_Init()` 成功，Steam 显示游戏「运行中」，**云存档（Steam Cloud）照常同步**。退出游戏后留意「同步中 / 同步完成」即可。

---

## 五、适用范围

本问题不是 Project Zomboid 独有，凡是满足以下条件的 Linux 原生游戏都可能遇到：

- 基于 **JVM / Java**，需要读取文件系统路径；
- 启动脚本或 Steam 运行时把 **`LC_ALL=C`** 注入进程；
- 创意工坊 Mod 的安装目录里**含有非 ASCII（中文等）字符**。

通用解法都是：通过启动选项把 locale 与 JVM 编码强制为 UTF-8。

---

## 六、排查命令速查

```bash
# 游戏 DebugLog 里搜路径编码异常
grep -E "InvalidPathException|unmappable" ~/Zomboid/Logs/*_DebugLog.txt

# 查看游戏进程的 locale 相关环境变量
PID=$(ps -eo pid,comm | awk '$2=="ProjectZomboid6"{print $1}')
tr '\0' '\n' < /proc/$PID/environ | grep -E "LANG|LC_"

# 确认某个 workshop 目录的实际名称与字节（是否含中文）
ls ~/.local/share/Steam/steamapps/workshop/content/108600/<WORKSHOP_ID>/mods/

# 确认 PNG 本身是好的（用于排除贴图问题）
file ~/.local/share/Steam/steamapps/common/ProjectZomboid/projectzomboid/media/fonts/CN/2x/zomboidSmallChinese_00.png
```

---

*记录日期：2026-09-28*
