@echo off
title PAMANA WebGL Game Launcher
echo =======================================================
echo  Starting PAMANA WebGL Local Game Server...
echo =======================================================
echo.
echo Launching local server and opening your web browser...
echo (Keep this window open while playing. Close it when finished.)
echo.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0serve_webgl.ps1"

pause
