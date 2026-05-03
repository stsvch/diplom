#!/bin/sh
set -eu

if [ -n "${STRIPE_SECRET_KEY:-}" ] && [ -z "${Stripe__SecretKey:-}" ]; then
  export Stripe__SecretKey="$STRIPE_SECRET_KEY"
fi

if [ -n "${Stripe__SecretKey:-}" ] && [ -z "${Stripe__WebhookSecret:-}" ]; then
  attempts=0
  while [ ! -f /stripe/webhook-secret ] && [ "$attempts" -lt 60 ]; do
    attempts=$((attempts + 1))
    sleep 1
  done

  if [ -f /stripe/webhook-secret ]; then
    export Stripe__WebhookSecret="$(tr -d '\r\n' < /stripe/webhook-secret)"
  fi
fi

exec dotnet EduPlatform.Host.dll
