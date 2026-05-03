Space Haven 存档修改器 - Windows 版
========================================

使用方法
--------

双击 "运行.bat"。

首次运行前置条件
------------------

需要 Python 3.10 或更高版本（自带 Tkinter）。

下载：https://www.python.org/downloads/windows/
安装时**必须勾选** "Add Python to PATH"。

运行步骤
--------

1. 退出 Space Haven 游戏（存档文件被游戏锁定时无法写入）。
2. 双击 "运行.bat"。
3. 顶部下拉选一个存档，点 Load。
4. Bank / Crew / Resources 三个标签页改数值。
5. 点底部 "Save (with backup)"。原文件会自动备份为 game.bak-时间戳。

存档位置
--------

Windows 默认：
    %APPDATA%\Spacehaven\savegames\
    或
    <WINDOWS_USER_HOME>\AppData\Roaming\Spacehaven\savegames\

找不到时工具会自动扫描以下路径（按优先级）：
1. 命令行参数传入的路径
2. 环境变量 SPACEHAVEN_SAVES 指定的路径
3. 常见默认位置

也可点 Browse... 手动选 save\game 文件。

崩档还原
--------

把同目录下的 game.bak-YYYYMMDD-HHMMSS 改名回 game 即可。

游戏更新后
-----------

若资源 ID 有变动，用 extract_names.py 重新生成 resource_names.json：

    python extract_names.py "<SPACEHAVEN_GAME_ROOT>\spacehaven.jar"
