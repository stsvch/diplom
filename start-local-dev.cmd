@echo off
setlocal
pwsh -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\dev\start-local-dev.ps1" %*
endlocal
