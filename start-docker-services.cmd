@echo off
setlocal
docker compose -f "%~dp0docker-compose.local.yml" up -d postgres mongo minio
docker compose -f "%~dp0docker-compose.local.yml" up -d --force-recreate stripe-listen
echo Docker services started. Use stop-docker-services.cmd to stop them.
endlocal
