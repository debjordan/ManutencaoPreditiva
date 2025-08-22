#!/bin/bash

echo "Iniciando API IoT Data..."

echo "Aguardando banco de dados..."
DB_PATH="${DATABASE_PATH:-/app/data/iot.db}"

while [ ! -f "$DB_PATH" ]; do
  echo "Aguardando banco SQLite em $DB_PATH..."
  sleep 5
done

echo "Banco de dados encontrado em: $DB_PATH"

while [ $(sqlite3 "$DB_PATH" "SELECT COUNT(*) FROM iot_data;" 2>/dev/null || echo "0") -lt 1 ]; do
  echo "⏳ Aguardando primeiros dados no banco..."
  sleep 5
done

echo "✅ Banco com dados disponível"
echo "🌐 API starting on http://localhost:5000"

exec dotnet IoTDataApi.dll
