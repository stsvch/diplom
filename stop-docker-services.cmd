@echo off
setlocal
docker compose -f "%~dp0docker-compose.local.yml" down
endlocal
