# Manutenção Preditiva — Industry 4.0

> Plataforma de **monitoramento preditivo de máquinas industriais** em tempo real: pipeline MQTT → API REST (.NET 8) → Dashboard React. Detecta degradação antes da falha, exibe OEE e dispara alertas por threshold de sensor.

[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![Docker Compose](https://img.shields.io/badge/docker-compose-ready-brightgreen)](#executar-com-docker-compose)
[![.NET 8](https://img.shields.io/badge/.NET-8-purple)](https://dotnet.microsoft.com/)
[![React 18](https://img.shields.io/badge/React-18-blue)](https://react.dev/)

---

## Problema

Em plantas industriais tradicionais, a manutenção ainda é predominantemente **corretiva ou preventiva por calendário**. Isso gera dois custos ocultos:

- **Corretiva**: a máquina quebra → linha para → prejuízo de parada não planejada (downtime)
- **Preventiva por calendário**: troca-se peças antes do necessário → custo de material e mão de obra desnecessários

A manutenção **preditiva** resolve isso monitorando o estado real do equipamento via sensores e agindo **quando os dados indicam deterioração**, não antes, não depois. O desafio é coletar dados de sensores heterogêneos em tempo real, persistir, calcular indicadores e expor tudo de forma acionável para o operador de chão de fábrica.

Este projeto simula e resolve exatamente esse pipeline.

---

## Solução

```
Python Simulator ──MQTT──▶ Mosquitto ──▶ Python Subscriber ──▶ SQLite DB
                                                                     │
                                                             ASP.NET Core API
                                                               /api/iot/*
                                                                     │
                                                          React/TS Dashboard
                                                        (recharts, Tailwind)
                                                                     │
                                               Prometheus ◀──┤ Grafana ◀──┘
```

O simulador gera leituras realistas de **6 máquinas industriais** com degradação probabilística. A API agrega, calcula OEE e classifica alertas. O dashboard apresenta o estado da fábrica em tempo real com dark UI otimizado para telas industriais.

---

## Máquinas Simuladas

| ID  | Máquina                  | Área                |
|-----|--------------------------|---------------------|
| M1  | Router CNC               | Usinagem            |
| M2  | Prensa Hidráulica        | Prensagem           |
| M3  | Cabine de Pintura        | Acabamento          |
| M4  | Esteira Transportadora   | Logística Interna   |
| M5  | Compressor de Ar         | Utilidades          |
| M6  | Serra Circular Industrial| Corte               |

Cada máquina tem ranges de sensores específicos para seu tipo. O simulador usa transições probabilísticas entre estados (`normal → degrading → critical`) com chance de recuperação simulando intervenção de manutenção.

---

## Funcionalidades

### Dashboard em Tempo Real
- **Dark UI** otimizado para chão de fábrica (telas industriais)
- **KPI bar** de fábrica: OEE médio, risco médio, máquinas críticas/degradando
- Cards por máquina: estado (Normal / Degradando / Crítico), OEE individual, risco, potência e 6 sensores
- **Painel de Alertas** colapsável com alertas críticos e de atenção

### Gráficos Históricos (recharts)
- Ao abrir os detalhes de uma máquina, busca as **últimas 60 leituras reais** da API
- Gráfico de linha multissérie: Vibração, Temperatura, Pressão
- Linhas de referência dos limites críticos por sensor

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

Calculado em tempo real com base nos sensores — sem necessidade de dados externos:

| Componente | Derivação |
|---|---|
| **Disponibilidade** | Degradada conforme estado da máquina (normal: 97%, degrading: 82%, critical: 60%) |
| **Performance** | `(1 - vibração/20) × 100` — vibração excessiva reduz cadência |
| **Qualidade** | `(1 - temperatura/80) × 100` — temperatura alta aumenta refugo |

### Simulador de Degradação

Cada máquina evolui via máquina de estados probabilística:
- **normal → degrading**: 4% de chance por ciclo
- **degrading → critical**: quando degradação acumulada ≥ 0.8
- **critical → normal**: 12% de chance por ciclo (simula intervenção)
- **degrading → normal**: 8% de chance por ciclo (simula ajuste)

O sensor de vibração é o principal indicador de degradação (`stress_factor=1.8`), seguido de temperatura e pressão — refletindo o comportamento real de equipamentos mecânicos.

### Observabilidade
- **Prometheus** + **Grafana** pré-configurados com scrape da API
- **Sentry** para rastreamento de erros em produção (via `SENTRY_DSN`)
- Health endpoint funcional para healthchecks de container e orquestradores

---

## Arquitetura

**API — Clean Architecture:**
```
IoTDataApi (Host)           ← Controllers, Program.cs, DI config
├── IoTDataApi.Domain       ← Entidades, interfaces de repositório
├── IoTDataApi.Application  ← Services, DTOs, interfaces de serviço
└── IoTDataApi.Infrastructure ← EF Core/SQLite, implementação dos repositórios
```

A separação de camadas garante que a lógica de negócio (cálculo de OEE, thresholds, classificação de alertas) fique em `Application`, totalmente desacoplada do banco. Trocar SQLite por PostgreSQL requer apenas reimplementar `IIoTDataRepository` em `Infrastructure`.

Para detalhes de fluxo de dados, decisões de tecnologia e plano de evolução, veja [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

---

## Executar com Docker Compose

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

## Executar Localmente (modo desenvolvimento)

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

## Variáveis de Ambiente

| Variável       | Descrição                        | Padrão              |
|----------------|----------------------------------|---------------------|
| `MQTT_HOST`    | Host do broker MQTT              | `localhost`         |
| `MQTT_PORT`    | Porta do broker                  | `1883`              |
| `SENTRY_DSN`   | DSN do Sentry (opcional)         | —                   |
| `API_BASE_URL` | URL base da API (frontend build) | `http://localhost:5002` |

---

## Estrutura do Projeto

```
src/
├── api/                         # ASP.NET Core 8 (Clean Architecture)
│   ├── IoTDataApi/              # Host — Controllers, Program.cs
│   ├── IoTDataApi.Application/  # Services, DTOs, Interfaces
│   ├── IoTDataApi.Domain/       # Entities, Repository Interfaces
│   └── IoTDataApi.Infrastructure/  # EF Core, SQLite, Repositories
├── client/iot-dashboard/        # React 18 + TypeScript + recharts + Tailwind
└── simulator/                   # Python — sensor_simulator.py, subscriber.py
docker/                          # Dockerfiles, nginx, mosquitto, grafana, prometheus
docs/                            # Arquitetura detalhada
tests/                           # Testes de integração (health, MQTT pub/sub)
```

---

## Decisões Técnicas

### Por que MQTT e não HTTP polling?

MQTT é o protocolo padrão de IoT industrial (SCADA, PLCs, sensores embarcados usam MQTT nativamente). O modelo publish/subscribe desacopla completamente o simulador (produtor) do subscriber (consumidor), permitindo múltiplos consumidores sem mudança no produtor. HTTP polling introduz latência variável e overhead de conexão a cada ciclo — inaceitável para monitoramento a 5s de intervalo com dezenas de máquinas.

### Por que SQLite e não PostgreSQL?

SQLite foi escolhido deliberadamente para este POC: zero configuração, sem processo de banco separado, fácil de inspecionar localmente. O código está isolado em `IoTDataRepository` — a migração para PostgreSQL ou SQL Server em produção é uma troca de implementação de repositório sem tocar em `Domain` ou `Application`. Num ambiente de produção, migraria para PostgreSQL com particionamento de tabela por `machine_id` e índice em `received_at`.

### Por que Clean Architecture na API?

A lógica de negócio (cálculo de OEE, thresholds, classificação de alertas) muda independentemente da forma de persistência. Com Clean Architecture, o `IoTDataService` é testável em isolamento — basta mockar `IIoTDataRepository`. Sem isso, os testes precisariam de banco real ou mocks frágeis acoplados ao EF Core. Para uma API deste tamanho, o overhead de camadas é pequeno e o ganho em testabilidade e substituibilidade compensa.

### Por que Python para o simulador?

Python tem o ecossistema mais maduro para prototipagem de IoT (`paho-mqtt` é a biblioteca de referência). O simulador é um componente auxiliar de geração de dados — não precisa das garantias de um serviço de produção. Se fosse um agente de coleta em borda (edge), usaria C ou C++ por restrição de memória.

### Por que Prometheus + Grafana e não um APM proprietário?

Prometheus + Grafana é o stack open-source padrão de observabilidade — sem lock-in, sem custo por host monitorado, extensível com exporters para qualquer infraestrutura. Em produção adicionaria OpenTelemetry para traces distribuídos, que se integra ao mesmo Grafana via Tempo.

---

## Possíveis Evoluções (produção)

- **Broker MQTT em cluster**: EMQX ou HiveMQ para alta disponibilidade e ACL por tópico
- **Banco relacional**: PostgreSQL com particionamento por `machine_id` e retenção configurável
- **Streaming**: substituir polling da API por WebSocket ou SSE para o dashboard, reduzindo latência de exibição de 5s para <1s
- **ML preditivo**: treinar modelo de classificação (ex.: XGBoost) com os dados históricos para prever falha X horas antes, em vez de reagir a threshold pontual
- **Autenticação**: JWT na API + RBAC (operador lê, engenheiro configura thresholds)
- **Orquestração**: Kubernetes com HPA no subscriber e na API, dado que o volume de mensagens é proporcional ao número de máquinas

---

## Licença

MIT — veja [LICENSE](LICENSE).
