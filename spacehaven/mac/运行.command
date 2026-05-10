#!/bin/bash
# Space Haven save editor launcher (mac)
# Runs editor.py directly.

cd "$(dirname "$0")"

if [ -n "$SPACEHAVEN_PYTHON" ]; then
    candidates=("$SPACEHAVEN_PYTHON")
else
    candidates=("python3" "python")
fi

for py in "${candidates[@]}"; do
    if command -v "$py" >/dev/null 2>&1; then
        if "$py" -c "import tkinter" >/dev/null 2>&1; then
            exec "$py" editor.py "$@"
        fi
    fi
done

echo "ERROR: Could not find python3 with tkinter."
echo "Set SPACEHAVEN_PYTHON or install a Python 3 build that includes tkinter."
read -p "Press Enter to close..."
exit 1
