# Changelog

Todas as mudanças notáveis neste projeto são documentadas aqui.

Formato: [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/)

---

## [Unreleased]

### Planejado
- WebSocket / SSE no dashboard para substituir polling HTTP
- Modelo ML para predição de falha (XGBoost sobre séries históricas)
- Autenticação JWT com RBAC (operator / engineer)
- Dashboards Grafana pré-configurados para taxa MQTT e latência de API

---

## [0.3.0] - 2026-04-23

### Adicionado
- Dashboard dark UI completo com KPI bar, cards por máquina e painel de alertas
- Gráficos históricos por máquina via recharts (últimas 60 leituras reais)
- Endpoint `GET /api/iot/machine/{id}/readings` com `?limit=N` (1–200)
- Endpoint `GET /api/iot/alerts` com classificação CRÍTICO / ALERTA por threshold
- Endpoint `GET /api/iot/stats` e `GET /api/iot/stats/{id}` com OEE, risco e agregações
- Cálculo de OEE em tempo real (disponibilidade × performance × qualidade)
- Score de risco composto (vibração + temperatura + pressão)
- Validação de `machineId` por regex no controller (prevenção de injection)
- Clamp de `limit` entre 1 e 200 (prevenção de abuso de memória)
- Integração Sentry via `SENTRY_DSN` na API
- `DefinePlugin` no webpack para injetar `API_BASE_URL` no build do frontend

### Alterado
- `ARCHITECTURE.md` expandido com fluxo detalhado, decisões técnicas e plano de produção
- `README.md` reescrito com seção Problema, Decisões Técnicas e Evoluções

---

## [0.2.0] - 2026-03-15

### Adicionado
- Clean Architecture na API: separação em `Domain`, `Application`, `Infrastructure`, `Host`
- `IIoTDataRepository` e `IIoTDataService` como contratos de inversão de dependência
- DTOs tipados: `SensorReadingDto`, `MachineStatsDto`, `AlertDto`
- Simulador com 6 máquinas industriais e ranges de sensor por tipo de equipamento
- Máquina de estados de degradação probabilística (normal → degrading → critical)
- Subscriber Python com persistência em SQLite via `paho-mqtt`
- Docker Compose completo: simulator, mosquitto, subscriber, api, frontend, prometheus, grafana
- Makefile com targets para build, up, down, logs e testes
- CI GitHub Actions (build .NET + build frontend)
- `README-Docker.md` com instruções específicas de Docker

### Alterado
- Simulador atualizado para incluir `machine_name`, `machine_type` e `area` no payload MQTT

---

## [0.1.0] - 2026-02-18

### Adicionado
- Estrutura inicial do projeto (.NET solution, React app, simulador Python)
- Broker MQTT Mosquitto em Docker
- API ASP.NET Core básica com endpoint `GET /api/iot`
- `CONTRIBUTING.md` e estrutura de documentação
- Licença MIT
