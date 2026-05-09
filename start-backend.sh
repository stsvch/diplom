#!/usr/bin/env bash
set -euo pipefail
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec dotnet run --project "$DIR/backend/src/Host/EduPlatform.Host.csproj" --launch-profile http
