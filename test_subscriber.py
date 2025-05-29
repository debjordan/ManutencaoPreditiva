import paho.mqtt.client as mqtt
import logging

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

def on_connect(client, userdata, flags, rc):
    logger.info(f"Conectado com código {rc}")
    client.subscribe("sensors/M001/data")
    logger.info("Inscrito no tópico sensors/M001/data")

def on_message(client, userdata, msg):
    logger.info(f"Recebido: {msg.payload.decode()} no tópico {msg.topic}")

client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message

try:
    client.connect("localhost", 1883, 60)
    client.loop_forever()
except Exception as e:
    logger.error(f"Erro: {e}")
