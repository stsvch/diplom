@echo off
setlocal
if not exist "%~dp0tmp" mkdir "%~dp0tmp"
set "TEMP=%~dp0tmp"
set "TMP=%~dp0tmp"
dotnet run --project "%~dp0backend\src\Host\EduPlatform.Host.csproj" --launch-profile http
endlocal
