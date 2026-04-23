# Arquitetura — Manutenção Preditiva

Este documento descreve os componentes do sistema, o fluxo de dados, as decisões de design e o plano de evolução para produção.

---

## Visão Geral

```
┌─────────────────────────────────────────────────────────────────────┐
│                          BORDA / SIMULAÇÃO                          │
│                                                                     │
│   sensor_simulator.py                                               │
│   (6 máquinas, ciclo 5s,      ──MQTT──▶  Mosquitto (broker)        │
│    estados: normal/degrading/critical)     :1883                    │
└──────────────────────────────────────────────┬──────────────────────┘
                                               │ tópico: sensors/{id}/data
                                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        CAMADA DE INGESTÃO                           │
│                                                                     │
│   subscriber.py                                                     │
│   - subscreve sensors/+/data                                        │
│   - persiste JSON cru em IoTData (SQLite)                           │
│   - não faz parsing nem validação — responsabilidade da API         │
└──────────────────────────────────────────────┬──────────────────────┘
                                               │ SQLite (iot.db)
                                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    API — ASP.NET Core 8 (Clean Architecture)        │
│                                                                     │
│  IoTDataApi (Host)                                                  │
│  ├── Controllers/IoTController   ← HTTP, validação de entrada       │
│  ├── Application/Services        ← lógica de negócio               │
│  │   ├── OEE computation                                            │
│  │   ├── risk score                                                 │
│  │   ├── threshold / alert classification                           │
│  │   └── JSON parsing dos payloads crus                             │
│  ├── Domain/                     ← entidades + interfaces           │
│  └── Infrastructure/Repositories ← EF Core + SQLite                │
│                                                                     │
│  Endpoints: /api/iot/*, /health, /metrics, /swagger                 │
└──────────────────────────────────────────────┬──────────────────────┘
                                               │ HTTP (JSON)
                                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       FRONTEND (React 18 + TS)                      │
│                                                                     │
│  App.tsx — polling a cada 5s                                        │
│  ├── /api/iot/stats  → KPI bar + MachineCards                       │
│  ├── /api/iot/alerts → AlertPanel                                   │
│  └── /api/iot/machine/{id}/readings → MachineDetailsModal (recharts)│
└─────────────────────────────────────────────────────────────────────┘

Observabilidade:
  API /metrics ──▶ Prometheus ──▶ Grafana
  Sentry SDK (opcional, via SENTRY_DSN)
```

---

## Fluxo de Dados Detalhado

### 1. Geração (simulador)

`sensor_simulator.py` mantém um estado de degradação por máquina. A cada ciclo de 5s:

1. Evolui o estado via `evolve_state()` — máquina de estados com probabilidades ajustadas por tipo de máquina
2. Calcula os valores de sensor com `simulate_sensor()`, aplicando um fator de estresse proporcional à degradação
3. Publica o payload JSON no tópico `sensors/{machine_id}/data` via MQTT

O payload inclui todos os campos necessários para o pipeline downstream, evitando joins:

```json
{
  "machine_id": "M1",
  "machine_name": "Router CNC",
  "machine_type": "cnc_router",
  "area": "Usinagem",
  "state": "degrading",
  "vibration": 9.42,
  "temperature": 58.1,
  "pressure": 4.87,
  "humidity": 38.5,
  "voltage": 221.3,
  "current": 16.2,
  "power": 3.9,
  "timestamp": "2026-04-23T14:32:01Z"
}
```

### 2. Ingestão (subscriber)

`subscriber.py` é intencionalmente simples: subscreve `sensors/+/data` e insere o JSON cru na tabela `IoTData` com um `received_at` do banco. Não faz parsing nem validação — isso é responsabilidade da API, que tem tipagem forte e tratamento de erros.

Essa separação de responsabilidades é importante: se o formato do payload mudar, só a API precisa ser atualizada.

### 3. API — camadas

**Domain** (`IoTDataApi.Domain`):
- `IoTData`: entidade com `Id`, `Topic`, `Message` (JSON cru), `ReceivedAt`
- `IIoTDataRepository`: contrato de acesso a dados — `GetAllAsync()`, `GetByMachineIdAsync()`

**Application** (`IoTDataApi.Application`):
- `IoTDataService`: toda a lógica de negócio
  - `ParseReading()`: deserializa o JSON cru → `SensorReadingDto`
  - `BuildStats()`: calcula min/max/avg/last + OEE + risco
  - `GetActiveAlertsAsync()`: verifica thresholds, classifica CRÍTICO / ALERTA
- DTOs: `SensorReadingDto`, `MachineStatsDto`, `AlertDto`

**Infrastructure** (`IoTDataApi.Infrastructure`):
- `IoTDataContext`: EF Core com SQLite
- `IoTDataRepository`: implementa `IIoTDataRepository`

**Host** (`IoTDataApi`):
- `IoTController`: recebe HTTP, valida `machineId` com regex `^[A-Za-z0-9_-]{1,20}$`, delega para o serviço
- `Program.cs`: configura DI, CORS, Swagger, Prometheus, Sentry

### 4. Frontend

O dashboard faz polling da API a cada 5s. Não há estado de servidor no React — cada poll substitui o estado local. Isso simplifica o código, mas significa que alertas que desaparecem entre polls ficam invisíveis. Em produção, migraria para WebSocket ou SSE.

---

## Cálculo de OEE

OEE = Disponibilidade × Performance × Qualidade / 10.000

| Componente | Fórmula | Justificativa |
|---|---|---|
| Disponibilidade | `97%` (normal), `82%` (degrading), `60%` (critical) | Baseia-se no estado declarado pelo simulador, que reflete probabilidade de parada |
| Performance | `max(0, (1 - vibração/20) × 100)` | Vibração excessiva reduz velocidade de operação segura |
| Qualidade | `max(0, (1 - temperatura/80) × 100)` | Temperatura alta aumenta taxa de defeito/refugo |

O resultado é um OEE simplificado — em produção, os três componentes seriam derivados de dados reais de produção (peças produzidas, tempo disponível, peças com defeito).

---

## Classificação de Alertas

Thresholds definidos em `IoTDataService`:

| Sensor | Alerta (Warning) | Crítico |
|---|---|---|
| Vibração (mm/s) | ≥ 10.0 | ≥ 12.0 |
| Temperatura (°C) | ≥ 55.0 | ≥ 60.0 |
| Pressão (bar) | ≥ 5.0 | ≥ 5.5 |
| Umidade (%) | ≥ 70.0 | ≥ 80.0 |
| Corrente (A) | ≥ 18.0 | ≥ 21.0 |

Alertas são calculados na leitura **mais recente** de cada máquina. A severidade é comparada como string (`CRÍTICO > ALERTA`) para ordenação — em produção usaria enum.

---

## Segurança

- `machineId` é validado por regex antes de qualquer acesso a dados (`^[A-Za-z0-9_-]{1,20}$`), prevenindo SQL injection por path parameter
- `limit` é fixado entre 1 e 200 via `Math.Clamp`, prevenindo abuso de memória
- CORS configurado em `Program.cs` — em produção restringiria à origem do dashboard
- MQTT sem autenticação (aceitável para POC em rede local; em produção usaria TLS + username/password ou certificados de cliente)

---

## Observabilidade

### Prometheus
A API expõe `/metrics` via `prometheus-net.AspNetCore`. O `prometheus.yml` do stack Docker scrape esse endpoint a cada 15s.

### Grafana
Pré-configurado com datasource Prometheus em `docker/grafana/`. Em produção, adicionaria dashboards para:
- Taxa de mensagens MQTT por máquina
- Latência dos endpoints da API (p50, p95, p99)
- Contagem de alertas críticos ao longo do tempo

### Sentry
Captura exceções não tratadas na API. Ativado via `SENTRY_DSN`. Em POC local pode ser omitido.

### Health Check
`GET /health` retorna 200 quando a API está operacional. Usado pelo Docker Compose para `healthcheck` e por orquestradores (K8s liveness/readiness probe).

---

## Plano de Evolução para Produção

### Banco de Dados
SQLite é adequado para POC local. Em produção:
- **PostgreSQL**: particionamento de tabela por `machine_id`, índice composto em `(machine_id, received_at)`, retenção via `pg_partman` (ex.: 90 dias de dados brutos, agregados indefinidamente)
- A troca requer apenas reimplementar `IoTDataRepository` e ajustar a connection string — `Domain` e `Application` não mudam

### Broker MQTT
- **EMQX** ou **HiveMQ** em cluster para HA
- TLS + autenticação por certificado de cliente para cada dispositivo
- ACL: cada máquina só publica em seu próprio tópico (`sensors/M1/*`)

### Dashboard — Tempo Real
Substituir polling HTTP por **Server-Sent Events (SSE)**: a API mantém uma conexão aberta e envia eventos quando novos dados chegam. Reduz latência de ~5s para <500ms e elimina polling desnecessário quando nada muda.

### Predição por ML
O modelo atual detecta anomalias por threshold estático. Com histórico suficiente, é possível treinar um modelo de classificação (XGBoost ou LSTM) para prever falha N horas antes, usando as séries temporais de vibração, temperatura e corrente como features.

### Autenticação e RBAC
- **JWT** na API com dois papéis: `operator` (leitura) e `engineer` (configura thresholds, aciona manutenção)
- Frontend: refresh token silencioso, sem reautenticação no meio do turno

### Implantação
- **Kubernetes**: `Deployment` para API e subscriber, `StatefulSet` para o broker MQTT, `HPA` baseado em lag de fila MQTT
- **CI/CD**: GitHub Actions já configurado (`.github/workflows/ci.yml`) — adicionar push de imagem para registry e deploy via `helm upgrade`
