"""
Simulador de Sensores IoT — Industry 4.0
Simula 6 máquinas industriais com degradação gradual e estados realistas.
"""
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

# Máquinas representativas de uma planta industrial de manufatura
MACHINES = {
    "M1": {
        "name": "Router CNC",
        "type": "cnc_router",
        "area": "Usinagem",
        "vibration_base": (3.0, 8.0),
        "temperature_base": (35.0, 52.0),
        "pressure_base": (4.2, 5.2),
        "humidity_base": (30.0, 50.0),
        "voltage_base": (215.0, 225.0),
        "current_base": (8.0, 14.0),
        "power_base": (2.5, 4.5),
    },
    "M2": {
        "name": "Prensa Hidráulica",
        "type": "hydraulic_press",
        "area": "Prensagem",
        "vibration_base": (2.0, 6.0),
        "temperature_base": (40.0, 58.0),
        "pressure_base": (4.0, 5.8),
        "humidity_base": (25.0, 45.0),
        "voltage_base": (218.0, 228.0),
        "current_base": (12.0, 18.0),
        "power_base": (3.5, 5.5),
    },
    "M3": {
        "name": "Cabine de Pintura",
        "type": "paint_booth",
        "area": "Acabamento",
        "vibration_base": (1.0, 4.0),
        "temperature_base": (25.0, 42.0),
        "pressure_base": (3.5, 4.8),
        "humidity_base": (40.0, 65.0),
        "voltage_base": (220.0, 230.0),
        "current_base": (5.0, 10.0),
        "power_base": (1.5, 3.0),
    },
    "M4": {
        "name": "Esteira Transportadora",
        "type": "conveyor",
        "area": "Logística Interna",
        "vibration_base": (4.0, 9.0),
        "temperature_base": (30.0, 48.0),
        "pressure_base": (3.8, 5.0),
        "humidity_base": (30.0, 55.0),
        "voltage_base": (215.0, 225.0),
        "current_base": (6.0, 12.0),
        "power_base": (1.8, 3.5),
    },
    "M5": {
        "name": "Compressor de Ar",
        "type": "air_compressor",
        "area": "Utilidades",
        "vibration_base": (5.0, 9.0),
        "temperature_base": (45.0, 62.0),
        "pressure_base": (4.8, 5.8),
        "humidity_base": (20.0, 40.0),
        "voltage_base": (218.0, 228.0),
        "current_base": (14.0, 19.0),
        "power_base": (3.0, 5.0),
    },
    "M6": {
        "name": "Serra Circular Industrial",
        "type": "circular_saw",
        "area": "Corte",
        "vibration_base": (6.0, 10.0),
        "temperature_base": (38.0, 55.0),
        "pressure_base": (4.2, 5.3),
        "humidity_base": (25.0, 45.0),
        "voltage_base": (216.0, 226.0),
        "current_base": (10.0, 16.0),
        "power_base": (3.0, 4.8),
    },
}

# Estado de degradação por máquina: mode ∈ {normal, degrading, critical}
machine_state: dict[str, dict] = {
    m: {"mode": "normal", "degradation": 0.0} for m in MACHINES
}


def evolve_state(machine_id: str) -> float:
    """Evolui aleatoriamente o estado de degradação da máquina."""
    state = machine_state[machine_id]
    r = random.random()
    if state["mode"] == "normal":
        if r < 0.04:  # 4% de chance de iniciar degradação
            state["mode"] = "degrading"
            state["degradation"] = 0.1
    elif state["mode"] == "degrading":
        state["degradation"] = min(1.0, state["degradation"] + random.uniform(0.04, 0.12))
        if state["degradation"] >= 0.8:
            state["mode"] = "critical"
        elif r < 0.08:  # 8% de recuperação espontânea (reinício/ajuste)
            state["mode"] = "normal"
            state["degradation"] = 0.0
    elif state["mode"] == "critical":
        if r < 0.12:  # 12% de recuperação (simula intervenção de manutenção)
            state["mode"] = "normal"
            state["degradation"] = 0.0
    return state["degradation"]


def simulate_sensor(base_range: tuple, degradation: float, stress_factor: float = 1.6) -> float:
    """Gera leitura de sensor com ruído e influência da degradação."""
    lo, hi = base_range
    value = random.uniform(lo, hi)
    if degradation > 0:
        stress = (hi - lo) * degradation * stress_factor
        value += stress * random.uniform(0.6, 1.0)
    return value


def on_connect(client, userdata, flags, rc):
    if rc == 0:
        logging.info("Conectado ao broker MQTT!")
    else:
        logging.error(f"Falha ao conectar, código {rc}")


def connect_with_retry(client, host, port, keepalive, max_retries=5, delay=5):
    for attempt in range(max_retries):
        try:
            client.connect(host, port, keepalive)
            return True
        except Exception as e:
            logging.error(f"Tentativa {attempt + 1} falhou: {e}")
            time.sleep(delay)
    logging.error("Número máximo de tentativas atingido. Encerrando.")
    return False


client = mqtt.Client(protocol=mqtt.MQTTv311)
client.on_connect = on_connect

if not connect_with_retry(client, host, port, 60):
    exit(1)

client.loop_start()

try:
    while True:
        for machine_id, cfg in MACHINES.items():
            degradation = evolve_state(machine_id)
            state_label = machine_state[machine_id]["mode"]

            sensor_data = {
                "machine_id": machine_id,
                "machine_name": cfg["name"],
                "machine_type": cfg["type"],
                "area": cfg["area"],
                "state": state_label,
                "vibration":   round(simulate_sensor(cfg["vibration_base"],   degradation, 1.8), 2),
                "temperature": round(simulate_sensor(cfg["temperature_base"], degradation, 1.5), 2),
                "humidity":    round(simulate_sensor(cfg["humidity_base"],    0.0,         0.0), 2),
                "pressure":    round(simulate_sensor(cfg["pressure_base"],    degradation, 1.2), 2),
                "voltage":     round(simulate_sensor(cfg["voltage_base"],     degradation * 0.3, 0.8), 2),
                "current":     round(simulate_sensor(cfg["current_base"],     degradation, 1.4), 2),
                "power":       round(simulate_sensor(cfg["power_base"],       degradation, 1.3), 2),
                "timestamp":   datetime.utcnow().isoformat() + "Z",
            }

            topic = f"sensors/{machine_id}/data"
            client.publish(topic, json.dumps(sensor_data))
            logging.info(
                f"[{machine_id}] {cfg['name']:<26} | state={state_label:<9} | "
                f"vib={sensor_data['vibration']:>5.2f}mm/s  "
                f"temp={sensor_data['temperature']:>5.1f}°C  "
                f"press={sensor_data['pressure']:>4.2f}bar"
            )
        time.sleep(5)
except KeyboardInterrupt:
    logging.info("Simulador encerrado.")
finally:
    client.loop_stop()
    client.disconnect()
