"""Simple MQTT publish/subscribe test for local broker.

Usage: python tests/test_mqtt_pubsub.py
"""
import time
import json
import threading
import os
import paho.mqtt.client as mqtt

BROKER = os.getenv("MQTT_BROKER", "localhost")
PORT = int(os.getenv("MQTT_PORT", 1883))
TOPIC = "test/health"

received = None

def on_message(client, userdata, msg):
    global received
    received = msg.payload.decode()
    print("Received:", received)

def main():
    client = mqtt.Client()
    client.on_message = on_message
    client.connect(BROKER, PORT, 60)
    client.loop_start()
    client.subscribe(TOPIC)
    payload = json.dumps({"ts": time.time(), "msg": "hello"})
    client.publish(TOPIC, payload)
    # wait for message
    t0 = time.time()
    while time.time() - t0 < 5:
        if received:
            print("MQTT pub/sub succeeded")
            client.loop_stop()
            client.disconnect()
            return 0
        time.sleep(0.1)
    print("MQTT pub/sub failed")
    client.loop_stop()
    client.disconnect()
    return 2

if __name__ == "__main__":
    raise SystemExit(main())
