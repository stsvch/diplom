@echo off
setlocal
docker compose -f "%~dp0docker-compose.full.yml" down
endlocal
