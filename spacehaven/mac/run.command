#!/bin/bash
# Space Haven save editor launcher (mac)

cd "$(dirname "$0")"

for py in "$HOME/python3/bin/python3" "python3" "python3" "python3"; do
    if command -v "$py" >/dev/null 2>&1; then
        if "$py" -c "import tkinter" >/dev/null 2>&1; then
            exec "$py" editor.py "$@"
        fi
    fi
done

echo "ERROR: Could not find python3 with tkinter."
echo "Install python or a Python 3 build that includes tkinter."
read -p "Press Enter to close..."
exit 1
