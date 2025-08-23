.PHONY: help build build-frontend build-api up up-dev down logs clean restart status

COMPOSE_FILE=docker-compose.yml
PROJECT_NAME=manutencaopreditiva

help:
	@echo ""
	@echo "Comandos disponíveis para IoT Dashboard:"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}'
	@echo ""

build: 
	@echo "Construindo todas as imagens Docker..."
	docker-compose -f $(COMPOSE_FILE) build

build-frontend:
	@echo "Construindo frontend..."
	docker-compose -f $(COMPOSE_FILE) build frontend

build-api:
	@echo "Construindo API..."
	docker-compose -f $(COMPOSE_FILE) build api

build-dev:
	@echo "Construindo imagens de desenvolvimento..."
	docker-compose -f $(COMPOSE_FILE) build frontend-dev

up:
	@echo "Iniciando aplicação IoT (produção)..."
	docker-compose -f $(COMPOSE_FILE) up -d frontend api mosquitto simulator subscriber
	@echo ""
	@echo "Aplicação iniciada!"
	@echo "Dashboard: http://localhost:3000"
	@echo "API: http://localhost:5000"
	@echo "MQTT: localhost:1883"
	@echo ""

up-dev:
	@echo "Iniciando aplicação IoT (desenvolvimento)..."
	docker-compose -f $(COMPOSE_FILE) up -d frontend-dev api mosquitto simulator subscriber
	@echo ""
	@echo "Aplicação iniciada em modo desenvolvimento!"
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

logs-frontend-dev:
	docker-compose -f $(COMPOSE_FILE) logs -f frontend-dev

logs-simulator:
	docker-compose -f $(COMPOSE_FILE) logs -f simulator

logs-mqtt:
	docker-compose -f $(COMPOSE_FILE) logs -f mosquitto

logs-subscriber:
	docker-compose -f $(COMPOSE_FILE) logs -f subscriber

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
	docker-compose -f $(COMPOSE_FILE) exec api /bin/sh

shell-simulator:
	docker-compose -f $(COMPOSE_FILE) exec simulator /bin/sh

shell-frontend:
	docker-compose -f $(COMPOSE_FILE) exec frontend /bin/sh

shell-frontend-dev:
	docker-compose -f $(COMPOSE_FILE) exec frontend-dev /bin/sh

db-query:
	docker-compose -f $(COMPOSE_FILE) exec api sqlite3 /app/simulator/iot.db "SELECT COUNT(*) as total_records FROM iot_data;"

db-latest:
	docker-compose -f $(COMPOSE_FILE) exec api sqlite3 /app/simulator/iot.db "SELECT topic, message, received_at FROM iot_data ORDER BY received_at DESC LIMIT 5;"

test-api:
	@echo "Testando API..."
	curl -s http://localhost:5000/api/iot | head -c 200
	@echo ""
	@echo "Testando API por máquina..."
	curl -s http://localhost:5000/api/iot/machine/M1 | head -c 200
	@echo ""

test-frontend:
	@echo "Testando frontend..."
	curl -s http://localhost:3000 | head -c 100 || echo "Frontend não está respondendo"
	@echo ""

test-mqtt:
	@echo "Testando MQTT..."
	timeout 5s mosquitto_sub -h localhost -t "sensors/+/data" -v || echo "MQTT teste concluído"

monitor:
	@echo "Monitorando dados MQTT..."
	mosquitto_sub -h localhost -t "sensors/+/data" -v

publish-test:
	@echo "Publicando mensagem de teste..."
	mosquitto_pub -h localhost -t "sensors/test/data" -m '{"temperature": 25.5, "vibration": 0.2}'

dev: build-dev up-dev logs

dev-logs: logs-frontend-dev logs-api

frontend-logs: logs-frontend-dev

api-logs: logs-api
