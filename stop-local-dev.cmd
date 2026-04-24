@echo off
setlocal
pwsh -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\dev\stop-local-dev.ps1" %*
endlocal
