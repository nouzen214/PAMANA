@echo off
echo Starting PAMANA WebGL Server...
start "" http://localhost:8000/index.html
python serve_webgl.py
pause
