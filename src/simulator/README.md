# Simulador IoT - Manutenção Preditiva

Este diretório contém os scripts Python responsáveis por simular sensores IoT de máquinas industriais e armazenar os dados em um banco SQLite.

## Componentes

### 1. `sensor_simulator.py`
Simula sensores máquinas que geram dados de:
- **Vibração** (8.0-15.0)
- **Temperatura** (45.0-70.0°C)
- **Umidade** (30.0-80.0%)
- **Pressão** (4.0-6.0 bar)
- **Tensão** (220.0-240.0V)
- **Corrente** (5.0-20.0A)
- **Potência** (1.0-5.0kW)

### 2. `subscriber.py`
Consome mensagens MQTT e armazena no banco SQLite local.

## Pré-requisitos

### Dependências Python
```bash
pip install paho-mqtt
```

### MQTT Broker
```bash
# Ubuntu/Debian
sudo apt update
sudo apt install mosquitto mosquitto-clients

# Iniciar o serviço
sudo systemctl start mosquitto
sudo systemctl enable mosquitto
```

### Verificar se o MQTT está funcionando
```bash
sudo systemctl status mosquitto
```

## Como Executar

### 1. Iniciar o Subscriber (Terminal 1)
```bash
cd src/simulator
python subscriber.py
```

### 2. Iniciar o Sensor Simulator (Terminal 2)
```bash
cd src/simulator
python sensor_simulator.py
```

## Estrutura de Dados

### Tópicos MQTT
- `sensors/<Machine>/data`

### Payload JSON
```json
{
  "machine_id": "M1",
  "vibration": 10.25,
  "temperature": 52.3,
  "humidity": 65.8,
  "pressure": 4.8,
  "voltage": 230.5,
  "current": 12.3,
  "power": 2.8,
  "timestamp": "2025-08-19T19:58:30.769425Z"
}
```

### Banco de Dados SQLite
**Tabela:** `iot_data`

| Campo | Tipo | Descrição |
|-------|------|-----------|
| id | INTEGER | Chave primária (auto increment) |
| topic | TEXT | Tópico MQTT |
| message | TEXT | Payload JSON |
| received_at | TEXT | Timestamp de recebimento |

## Verificar Dados

### Consultar banco SQLite
```bash
sqlite3 iot.db "SELECT COUNT(*) FROM iot_data;"
sqlite3 iot.db "SELECT * FROM iot_data ORDER BY received_at DESC LIMIT 5;"
```

### Testar MQTT manualmente
```bash
# Subscrever
mosquitto_sub -h localhost -t "sensors/+/data"

# Publicar teste
mosquitto_pub -h localhost -t "sensors/TEST/data" -m '{"test": true}'
```

## Thresholds de Alerta

Os dados são gerados dentro de faixas que podem gerar diferentes níveis de alerta(Usei esses critérios, fique a vontade para adaptá-los a sua vontade):

| Sensor | Normal | Alerta | Crítico |
|--------|--------|--------|---------|
| Vibração | ≤ 10.0 | 10.1-12.0 | > 12.0 |
| Temperatura | ≤ 55°C | 55.1-60°C | > 60°C |
| Pressão | ≤ 5.0 bar | 5.1-5.5 bar | > 5.5 bar |

## Troubleshooting

### Erro de conexão MQTT
```bash
# Verificar se mosquitto está rodando
sudo systemctl status mosquitto

# Reiniciar se necessário
sudo systemctl restart mosquitto
```

### Banco não criado
- Verifique permissões de escrita no diretório
- Execute primeiro o `subscriber.py`
- Verifique logs de erro no terminal

### Dados não aparecem
- Confirme que ambos os scripts estão rodando
- Verifique se há dados no banco com SQLite
- Confirme conexão com MQTT broker
