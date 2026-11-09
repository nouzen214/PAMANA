@echo off
title PAMANA WebGL Launcher
echo =======================================================
echo  Starting PAMANA WebGL Local Game Server...
echo =======================================================
echo.

REM Try PowerShell launcher first (works on all Windows PCs)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0serve_webgl.ps1"

REM Fallback to python if powershell was cancelled
if errorlevel 1 (
    echo Falling back to Python...
    start "" http://127.0.0.1:8000/index.html
    python "%~dp0serve_webgl.py"
)

pause
