# Arquitetura

Visão geral dos componentes e fluxos principais.

```mermaid
graph TD
  SensorSimulator[Sensor Simulator] -->|MQTT| Mosquitto[MQTT Broker (Mosquitto)]
  Mosquitto --> Subscriber[Subscriber]
  Subscriber --> SQLite[SQLite DB]
  SQLite -->|EF Core| IoTApi[ASP.NET Core API]
  IoTApi -->|HTTP| Frontend[React Dashboard]
  Frontend --> User[Operador]
```

Componentes
- Sensor Simulator: gera dados sintéticos via MQTT para demonstração/POC.
- MQTT Broker: `mosquitto` faz o transporte leve de mensagens.
- Subscriber: consome mensagens MQTT e persiste em SQLite.
- API (.NET): expõe endpoints REST para leitura de dados e integração.
- Frontend: painel em React/TypeScript exibindo status em tempo real.

Deployment
- For POC use `docker-compose.yml` (local). For produção recomendamos:
  - Broker MQTT gerenciado ou em cluster (ex.: EMQX, Mosquitto em HA)
  - API em contêineres orquestrados (Kubernetes), com RDS/SQL Server/Postgres
  - Frontend servido por CDN ou ingress NGINX

Observabilidade
- Adicionar métricas Prometheus e traces (OpenTelemetry) na API e no subscriber.
