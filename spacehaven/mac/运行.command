#!/bin/bash
# Space Haven 存档修改器启动脚本 (mac)
# 直接跑 editor.py，绕过旧的 .app 冻结包

cd "$(dirname "$0")"

# 找带 tkinter 的 python
for py in "python3" "python3" "python3" "python3"; do
    if command -v "$py" >/dev/null 2>&1; then
        if "$py" -c "import tkinter" >/dev/null 2>&1; then
            exec "$py" editor.py "$@"
        fi
    fi
done

echo "错误: 找不到带 tkinter 的 python3"
echo "请安装 python 或带 tk 的 python3"
read -p "按回车关闭..."
exit 1
