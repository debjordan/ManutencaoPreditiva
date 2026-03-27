# ⚙️ Manutenção Preditiva — Industry 4.0

> Plataforma de **monitoramento preditivo de máquinas industriais** para o contexto de **Industry 4.0**, adaptável a qualquer setor que utilize equipamentos de manufatura.

[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![Docker Compose](https://img.shields.io/badge/docker-compose-ready-brightgreen)](#executar-com-docker-compose)
[![.NET 8](https://img.shields.io/badge/.NET-8-purple)](https://dotnet.microsoft.com/)
[![React 18](https://img.shields.io/badge/React-18-blue)](https://react.dev/)

---

## 🏭 Contexto

A adoção de **Industry 4.0** — sensoriamento IoT, MQTT, monitoramento em tempo real e manutenção preditiva — é a principal alavanca competitiva de fábricas frente à concorrência global, reduzindo downtime e custos de manutenção corretiva.

Esta plataforma simula e monitora **6 máquinas representativas** de uma planta industrial:

| ID  | Máquina                  | Área                |
|-----|--------------------------|---------------------|
| M1  | Router CNC               | Usinagem            |
| M2  | Prensa Hidráulica        | Prensagem           |
| M3  | Cabine de Pintura        | Acabamento          |
| M4  | Esteira Transportadora   | Logística Interna   |
| M5  | Compressor de Ar         | Utilidades          |
| M6  | Serra Circular Industrial| Corte               |

---

## 🔑 Funcionalidades

### Dashboard em Tempo Real
- **Dark UI** otimizado para chão de fábrica (telas industriais)
- **KPI bar** de fábrica: OEE médio, risco médio, máquinas críticas/degradando
- Cards por máquina: estado (Normal / Degradando / Crítico), OEE individual, risco, potência e 6 sensores
- **Painel de Alertas** colapsável com alertas críticos e de atenção

### Gráficos Reais (recharts)
- Ao abrir os detalhes de uma máquina, busca as **últimas 60 leituras reais** da API
- Gráfico de linha multissérie: Vibração, Temperatura, Pressão
- Linhas de referência dos limites críticos

### API REST (ASP.NET Core 8 — Clean Architecture)
| Endpoint | Descrição |
|---|---|
| `GET /api/iot` | Últimos 100 registros brutos |
| `GET /api/iot/machine/{id}` | Últimos 100 registros da máquina |
| `GET /api/iot/machine/{id}/readings` | Leituras tipadas (DTO) — suporta `?limit=N` |
| `GET /api/iot/stats` | Estatísticas agregadas de todas as máquinas (min/max/avg/last, OEE, risco) |
| `GET /api/iot/stats/{id}` | Estatísticas de uma máquina específica |
| `GET /api/iot/alerts` | Alertas ativos por threshold (CRÍTICO / ALERTA) |
| `GET /health` | Health check para container |
| `GET /metrics` | Métricas Prometheus |
| `GET /swagger` | Swagger UI |

### OEE (Overall Equipment Effectiveness)
Calculado em tempo real com base nos sensores:
- **Disponibilidade**: degradado conforme estado da máquina
- **Performance**: derivado do nível de vibração
- **Qualidade**: derivado da temperatura operacional

### Simulador Realista
- Cada máquina tem **ranges de sensor específicos** para seu tipo
- **3 estados**: Normal → Degradando → Crítico (com transições probabilísticas)
- Simula intervenção de manutenção (retorno ao estado normal)

### Observabilidade
- **Prometheus** + **Grafana** pré-configurados
- **Sentry** para rastreamento de erros (via `SENTRY_DSN`)
- Health endpoint funcional para healthchecks de container

---

## 🏗️ Arquitetura

```
Python Simulator ──MQTT──▶ Mosquitto ──▶ Python Subscriber ──▶ SQLite DB
                                                                     │
                                                             ASP.NET Core API
                                                               /api/iot/*
                                                                     │
                                                          React/TS Dashboard
                                                        (recharts, Tailwind)
                                                                     │
                                                       Prometheus ◀──┤
                                                       Grafana    ◀──┘
```

**API — Clean Architecture:**
```
IoTDataApi (Host)
├── IoTDataApi.Domain        (Entities, Interfaces)
├── IoTDataApi.Application   (Services, DTOs, Interfaces)
└── IoTDataApi.Infrastructure (EF Core/SQLite, Repositories)
```

---

## 🚀 Executar com Docker Compose

```bash
git clone <repository-url>
cd ManutencaoPreditiva

# Subir todo o stack
docker compose up -d

# Verificar saúde dos serviços
docker compose ps
```

| Serviço    | URL                           |
|------------|-------------------------------|
| Dashboard  | http://localhost:8080         |
| API        | http://localhost:5002/swagger |
| Grafana    | http://localhost:3000         |
| Prometheus | http://localhost:9090         |

---

## 💻 Executar Localmente (modo desenvolvimento)

### 1. Broker MQTT
```bash
docker run -d -p 1883:1883 eclipse-mosquitto:2.0 \
  sh -c "echo 'listener 1883\nallow_anonymous true' > /mosquitto/config/mosquitto.conf && mosquitto -c /mosquitto/config/mosquitto.conf"
```

### 2. Simulador + Subscriber
```bash
cd src/simulator
pip install -r requirements.txt
python subscriber.py &   # persiste dados no SQLite
python sensor_simulator.py
```

### 3. API
```bash
cd src/api
dotnet run --project IoTDataApi
# Swagger: http://localhost:5000/swagger
```

### 4. Frontend
```bash
cd src/client/iot-dashboard
npm install
npm start
# Dashboard: http://localhost:8080
```

---

## 🌍 Variáveis de Ambiente

| Variável       | Descrição                        | Padrão              |
|----------------|----------------------------------|---------------------|
| `MQTT_HOST`    | Host do broker MQTT              | `localhost`         |
| `MQTT_PORT`    | Porta do broker                  | `1883`              |
| `SENTRY_DSN`   | DSN do Sentry (opcional)         | —                   |
| `API_BASE_URL` | URL base da API (frontend build) | `http://localhost:5002` |

---

## 📁 Estrutura do Projeto

```
src/
├── api/                     # ASP.NET Core 8 (Clean Architecture)
│   ├── IoTDataApi/          # Host — Controllers, Program.cs
│   ├── IoTDataApi.Application/  # Services, DTOs, Interfaces
│   ├── IoTDataApi.Domain/   # Entities, Repository Interfaces
│   └── IoTDataApi.Infrastructure/  # EF Core, SQLite, Repositories
├── client/iot-dashboard/    # React 18 + TypeScript + recharts
└── simulator/               # Python — sensor_simulator.py, subscriber.py
docker/                      # Dockerfiles, nginx, mosquitto, grafana, prometheus
```

---

## 📜 Licença

MIT — veja [LICENSE](LICENSE).

---

*Desenvolvido com foco em ambientes industriais reais.*
