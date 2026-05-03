@echo off
setlocal
docker compose -f "%~dp0docker-compose.full.yml" up --build
endlocal
