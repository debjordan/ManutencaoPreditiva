"""Simple seeder that publishes synthetic MQTT messages for demo/POC."""
import time
import json
import random
import os
import paho.mqtt.client as mqtt

BROKER = os.getenv("MQTT_BROKER", "mosquitto")
PORT = int(os.getenv("MQTT_PORT", 1883))

client = mqtt.Client()
client.connect(BROKER, PORT, 60)

def make_message(machine_id: str):
    return {
        "machine_id": machine_id,
        "vibration": round(random.uniform(8.0, 15.0), 2),
        "temperature": round(random.uniform(45.0, 70.0), 1),
        "pressure": round(random.uniform(4.0, 6.0), 2),
        "timestamp": time.time(),
    }

try:
    while True:
        for m in ["M1", "M2", "M3"]:
            topic = f"sensors/{m}/data"
            payload = json.dumps(make_message(m))
            client.publish(topic, payload)
            print(f"Published to {topic}: {payload}")
            time.sleep(0.2)
        time.sleep(1)
except KeyboardInterrupt:
    client.disconnect()
