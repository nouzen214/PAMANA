@echo off
echo =======================================================
echo  Launching PAMANA WebGL Game in your default browser...
echo =======================================================
start "" http://localhost:8000/index.html
cd /d "%~dp0Builds\WebGL"
python serve_webgl.py
pause
