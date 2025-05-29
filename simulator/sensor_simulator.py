import paho.mqtt.client as mqtt
import time
import json
import random
import logging
from datetime import datetime

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

def on_connect(client, userdata, flags, rc):
    if rc == 0:
        logging.info("Connected to MQTT Broker!")
    else:
        logging.error(f"Failed to connect, return code {rc}")

client = mqtt.Client()
client.on_connect = on_connect
client.connect("mosquitto", 1883, 60)
client.loop_start()

while True:
    sensor_data = {
        "machine_id": "M001",
        "vibration": round(random.uniform(0.1, 2.0), 2),
        "temperature": round(random.uniform(50.0, 100.0), 2),
        "timestamp": datetime.utcnow().isoformat() + "Z"
    }
    production_data = {
        "machine_id": "M001",
        "cycle_time": round(random.uniform(10.0, 30.0), 2),
        "man_time": round(random.uniform(5.0, 20.0), 2),
        "machine_time": round(random.uniform(10.0, 40.0), 2),
        "availability": round(random.uniform(0.8, 1.0), 2),
        "performance": round(random.uniform(0.7, 1.0), 2),
        "quality": round(random.uniform(0.85, 1.0), 2),
        "timestamp": datetime.utcnow().isoformat() + "Z"
    }

    client.publish("sensors/M001/data", json.dumps(sensor_data))
    logging.info(f"Published sensor data: {sensor_data}")
    client.publish("sensors/M001/production", json.dumps(production_data))
    logging.info(f"Published production data: {production_data}")
    time.sleep(5)
