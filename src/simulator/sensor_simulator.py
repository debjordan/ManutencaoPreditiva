import paho.mqtt.client as mqtt
import time
import json
import random
import logging
from datetime import datetime
import os

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

host = os.getenv("MQTT_HOST", "localhost")
port = int(os.getenv("MQTT_PORT", 1883))

def on_connect(client, userdata, flags, rc):
    if rc == 0:
        logging.info("Connected to MQTT Broker!")
    else:
        logging.error(f"Failed to connect, return code {rc}")

def connect_with_retry(client, host, port, keepalive, max_retries=5, delay=5):
    for attempt in range(max_retries):
        try:
            client.connect(host, port, keepalive)
            return True
        except Exception as e:
            logging.error(f"Connection attempt {attempt + 1} failed: {e}")
            time.sleep(delay)
    logging.error("Max retries reached. Could not connect to broker.")
    return False

client = mqtt.Client(protocol=mqtt.MQTTv311)
client.on_connect = on_connect

if not connect_with_retry(client, host, port, 60):
    exit(1)

client.loop_start()

machines = ["M1", "M2", "M3", "M4", "M5", "M6"]

try:
    while True:
        for machine_id in machines:
            sensor_data = {
                "machine_id": machine_id,
                "vibration": round(random.uniform(8.0, 15.0), 2),
                "temperature": round(random.uniform(45.0, 70.0), 2),
                "humidity": round(random.uniform(30.0, 80.0), 2),
                "pressure": round(random.uniform(4.0, 6.0), 2),
                "voltage": round(random.uniform(220.0, 240.0), 2),
                "current": round(random.uniform(5.0, 20.0), 2),
                "power": round(random.uniform(1.0, 5.0), 2),
                "timestamp": datetime.utcnow().isoformat() + "Z"
            }
            topic = f"sensors/{machine_id}/data"
            client.publish(topic, json.dumps(sensor_data))
            logging.info(f"Published data for {machine_id}: vibration={sensor_data['vibration']}, temp={sensor_data['temperature']}, pressure={sensor_data['pressure']}")
        time.sleep(5)
except KeyboardInterrupt:
    logging.info("Stopping sensor simulator...")
finally:
    client.loop_stop()
    client.disconnect()
