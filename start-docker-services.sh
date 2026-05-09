#!/usr/bin/env bash
set -euo pipefail
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [[ ! -f "$DIR/.env" ]]; then
  echo "Файл .env не найден. Скопируй .env.example в .env и заполни STRIPE_SECRET_KEY." >&2
  exit 1
fi

if grep -qE '^STRIPE_SECRET_KEY=sk_test_replace_me' "$DIR/.env"; then
  echo "STRIPE_SECRET_KEY в .env всё ещё равен заглушке sk_test_replace_me." >&2
  echo "Замени его на свой реальный sk_test_... из https://dashboard.stripe.com/test/apikeys" >&2
  exit 1
fi

docker compose -f "$DIR/docker-compose.local.yml" up -d postgres mongo minio
docker compose -f "$DIR/docker-compose.local.yml" up -d --force-recreate stripe-listen

echo "Docker services started. Stripe webhook secret пишется автоматически в backend/src/Host/appsettings.Development.Local.json."
echo "Остановить: ./stop-docker-services.sh"
