# Docker demo

Este diretório contém os Dockerfiles usados pelo `docker-compose.yml` na raiz.

Como executar um demo local rápido:

1. Construa e suba os containers:

```bash
docker compose up --build
```

2. Verifique os serviços:

- Dashboard: http://localhost:8080
- API: http://localhost:5000
- MQTT Broker: localhost:1883

Notas:
- Os `Dockerfile.*` já presentes em `docker/` são usados para construir cada serviço.
- O serviço `simulator` publica dados via MQTT para o broker `mosquitto` e o `subscriber` deve persistir no banco.
- Para rodar em background: `docker compose up -d --build`.
