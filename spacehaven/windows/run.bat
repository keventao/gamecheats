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

echo Python was not found. Install Python 3.10+ and enable Add to PATH:
echo     https://www.python.org/downloads/windows/
pause
exit /b 1

:end
if %ERRORLEVEL% NEQ 0 pause
