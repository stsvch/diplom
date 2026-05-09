#!/usr/bin/env bash
set -euo pipefail
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$DIR/frontend"
if [[ ! -d node_modules ]]; then
  npm install --legacy-peer-deps
fi
exec npm start
