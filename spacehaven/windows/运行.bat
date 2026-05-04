@echo off
chcp 65001 >nul
setlocal

rem Find Python (try py launcher first, then python)
where py >nul 2>&1
if %ERRORLEVEL%==0 (
    py -3 "%~dp0editor.py" %*
    goto :end
)
where python >nul 2>&1
if %ERRORLEVEL%==0 (
    python "%~dp0editor.py" %*
    goto :end
)

echo 未检测到 Python。请先安装 Python 3.10+（勾选 Add to PATH，默认带 Tkinter）：
echo     https://www.python.org/downloads/windows/
pause
exit /b 1

:end
if %ERRORLEVEL% NEQ 0 pause
