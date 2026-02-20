#!/usr/bin/env bash
set -euo pipefail

URL=${1:-http://localhost:5000/health}

echo "Checking API health at $URL"
if curl --fail --silent --show-error "$URL" -o /dev/null; then
  echo "API health OK"
  exit 0
else
  echo "API health FAILED"
  exit 2
fi
