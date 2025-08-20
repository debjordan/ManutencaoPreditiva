import paho.mqtt.client as mqtt
import logging
import sqlite3
import os
from datetime import datetime

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

script_dir = os.path.dirname(os.path.abspath(__file__))
db_path = os.path.join(script_dir, "iot.db")

try:
    conn = sqlite3.connect(db_path, check_same_thread=False)
    cursor = conn.cursor()
    cursor.execute("""
    CREATE TABLE IF NOT EXISTS iot_data (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        topic TEXT NOT NULL,
        message TEXT NOT NULL,
        received_at TEXT NOT NULL
    )
    """)
    conn.commit()

    cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='iot_data';")
    result = cursor.fetchone()
    if result:
        logger.info("Table 'iot_data' confirmed to exist")
    else:
        logger.error("Table 'iot_data' was not created!")

except sqlite3.Error as e:
    logger.error(f"Failed to initialize database: {e}")
    exit(1)

def on_connect(client, userdata, flags, rc):
    if rc == 0:
        logger.info("Connected to MQTT Broker!")

        client.subscribe("sensors/+/data")
        logger.info("Subscribed to sensors/+/data")
    else:
        logger.error(f"Failed to connect to MQTT, return code {rc}")

def on_message(client, userdata, msg):
    try:
        payload = msg.payload.decode()
        timestamp = datetime.utcnow().isoformat() + "Z"

        cursor.execute(
            "INSERT INTO iot_data (topic, message, received_at) VALUES (?, ?, ?)",
            (msg.topic, payload, timestamp)
        )
        conn.commit()

        logger.info(f"Saved message from {msg.topic}: {payload[:50]}...")

    except sqlite3.Error as e:
        logger.error(f"Database error: {e}")
    except Exception as e:
        logger.error(f"Error processing message: {e}")

def on_subscribe(client, userdata, mid, granted_qos):
    logger.info(f"Subscription confirmed with QoS: {granted_qos}")

client = mqtt.Client(protocol=mqtt.MQTTv311)
client.on_connect = on_connect
client.on_message = on_message
client.on_subscribe = on_subscribe

try:
    logger.info("Connecting to MQTT broker...")
    client.connect("localhost", 1883, 60)
    logger.info("Starting MQTT loop...")
    client.loop_forever()
except Exception as e:
    logger.error(f"MQTT Error: {e}")
finally:
    logger.info("Stopping subscriber...")
    client.loop_stop()
    client.disconnect()
    conn.close()
