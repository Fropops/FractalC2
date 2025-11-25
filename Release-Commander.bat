@echo off
powershell -NoProfile -ExecutionPolicy Bypass -Command "& { . .\Release-FractalC2.ps1; Release-FractalC2 -Target Commander }"
PAUSE
