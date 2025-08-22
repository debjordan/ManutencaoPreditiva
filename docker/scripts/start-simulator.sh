#!/bin/bash


echo "Iniciando simulador IoT..."

echo "guardando MQTT broker..."
MQTT_HOST="${MQTT_HOST:-mosquitto}"
MQTT_PORT="${MQTT_PORT:-1883}"

test_mqtt() {
    python3 -c "
import paho.mqtt.client as mqtt
import sys
try:
    client = mqtt.Client()
    client.connect('$MQTT_HOST', $MQTT_PORT, 60)
    client.disconnect()
    sys.exit(0)
except:
    sys.exit(1)
" 2>/dev/null
}

while ! test_mqtt; do
    echo "⏳ Tentando conectar ao MQTT broker em $MQTT_HOST:$MQTT_PORT..."
    sleep 2
done
echo "MQTT broker disponível"

export DB_PATH="/app/data/iot.db"
export MQTT_HOST="$MQTT_HOST"
export MQTT_PORT="$MQTT_PORT"

echo "🔌 Iniciando subscriber MQTT..."
python subscriber.py &
SUBSCRIBER_PID=$!

sleep 5

echo "Iniciando simulador de sensores..."
python sensor_simulator.py &
SIMULATOR_PID=$!

cleanup() {
    echo "🛑 Parando simulador..."
    kill $SUBSCRIBER_PID 2>/dev/null
    kill $SIMULATOR_PID 2>/dev/null
    exit 0
}

trap cleanup SIGTERM SIGINT

wait $SUBSCRIBER_PID
wait $SIMULATOR_PID
