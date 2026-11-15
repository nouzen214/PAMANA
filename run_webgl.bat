@echo off
title PAMANA WebGL Game
echo =======================================================
echo  Launching PAMANA WebGL Game in your default browser...
echo =======================================================
echo.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0docs\serve_webgl.ps1"

pause
