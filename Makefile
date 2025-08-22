.PHONY: help build up down logs clean restart status

COMPOSE_FILE=docker-compose.yml
PROJECT_NAME=iot-dashboard

help:
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}'
	@echo ""

build:
	@echo "Construindo imagens Docker..."
	docker-compose -f $(COMPOSE_FILE) build --no-cache

up:
	@echo "Iniciando aplicação IoT..."
	docker-compose -f $(COMPOSE_FILE) up -d
	@echo ""
	@echo "Aplicação iniciada!"
	@echo "Dashboard: http://localhost:8080"
	@echo "API: http://localhost:5000"
	@echo "MQTT: localhost:1883"
	@echo ""

up-logs:
	@echo "Iniciando aplicação IoT com logs..."
	docker-compose -f $(COMPOSE_FILE) up

down:
	@echo "Parando aplicação IoT..."
	docker-compose -f $(COMPOSE_FILE) down

restart: down up

status:
	@echo "Status dos containers:"
	docker-compose -f $(COMPOSE_FILE) ps

logs:
	docker-compose -f $(COMPOSE_FILE) logs -f

logs-api:
	docker-compose -f $(COMPOSE_FILE) logs -f api

logs-frontend:
	docker-compose -f $(COMPOSE_FILE) logs -f frontend

logs-simulator:
	docker-compose -f $(COMPOSE_FILE) logs -f simulator

logs-mqtt:
	docker-compose -f $(COMPOSE_FILE) logs -f mosquitto

clean:
	@echo "Limpando containers parados..."
	docker container prune -f
	@echo "Limpando redes não utilizadas..."
	docker network prune -f
	@echo "Limpando volumes não utilizados..."
	docker volume prune -f

clean-all: down
	@echo "Removendo tudo..."
	docker-compose -f $(COMPOSE_FILE) down -v --remove-orphans
	docker system prune -af --volumes

shell-api:
	docker-compose -f $(COMPOSE_FILE) exec api /bin/bash

shell-simulator:
	docker-compose -f $(COMPOSE_FILE) exec simulator /bin/bash

shell-frontend:
	docker-compose -f $(COMPOSE_FILE) exec frontend /bin/sh

db-query:
	docker-compose -f $(COMPOSE_FILE) exec simulator sqlite3 /app/data/iot.db "SELECT COUNT(*) as total_records FROM iot_data;"

db-latest:
	docker-compose -f $(COMPOSE_FILE) exec simulator sqlite3 /app/data/iot.db "SELECT topic, message, received_at FROM iot_data ORDER BY received_at DESC LIMIT 5;"

test-api:
	@echo "Testando API..."
	curl -s http://localhost:5000/api/iot/machine/M1 | head -c 200
	@echo ""

test-mqtt:
	@echo "Testando MQTT..."
	timeout 5s mosquitto_sub -h localhost -t "sensors/+/data" -v || echo "MQTT teste concluído"
