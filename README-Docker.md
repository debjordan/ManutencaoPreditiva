# 🐳 Dockerização da Aplicação IoT

Esta documentação cobre a containerização completa do sistema de monitoramento IoT usando Docker Compose.

## 🏗️ Arquitetura Docker

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │      API        │    │   Simulador     │
│   (React)       │────│   (.NET Core)   │────│   (Python)      │
│   Port: 8080    │    │   Port: 5000    │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                                        │
                       ┌─────────────────┐             │
                       │   MQTT Broker   │─────────────┘
                       │  (Mosquitto)    │
                       │   Port: 1883    │
                       └─────────────────┘
                                │
                       ┌─────────────────┐
                       │  SQLite Data    │
                       │   (Volume)      │
                       └─────────────────┘
```

## 📋 Pré-requisitos

### Docker & Docker Compose
```bash
# Ubuntu/Debian
sudo apt update
sudo apt install docker.io docker-compose

# Verificar instalação
docker --version
docker-compose --version
```

### Estrutura de Arquivos Necessária

Criar a seguinte estrutura de diretórios:

```
MANUTENCAOPREDITIVA/
├── docker/
│   ├── scripts/
│   │   ├── start-simulator.sh
│   │   └── configure-api.sh
│   ├── mosquitto/
│   │   └── mosquitto.conf
│   └── nginx/
│       ├── nginx.conf
│       └── default.conf
├── data/                    # Volume para SQLite
├── src/
│   ├── api/IoTDataApi/
│   ├── client/iot-dashboard/
│   └── simulator/
├── docker-compose.yml
└── Makefile
```

## 🚀 Como Usar

### 1. Preparar Ambiente
```bash
# Clonar/navegar para o projeto
cd MANUTENCAOPREDITIVA

# Criar diretórios necessários
mkdir -p docker/{scripts,mosquitto,nginx} data

# Copiar arquivos de configuração (dos artifacts acima)
```

### 2. Build e Inicialização Rápida
```bash
# Usando Makefile (recomendado)
make build
make up

# Ou usando Docker Compose diretamente
docker-compose build --no-cache
docker-compose up -d
```

### 3. Verificar Status
```bash
# Ver status dos containers
make status

# Ver logs em tempo real
make logs

# Testar API
make test-api
```

### 4. Acessar Aplicações

| Serviço | URL | Descrição |
|---------|-----|-----------|
| **Dashboard** | http://localhost:8080 | Interface web principal |
| **API** | http://localhost:5000 | Endpoints REST |
| **Swagger** | http://localhost:5000/swagger | Documentação API |
| **MQTT** | localhost:1883 | Broker MQTT |

## 🔧 Comandos Úteis

### Gerenciamento Básico
```bash
make up          # Iniciar todos os serviços
make down        # Parar todos os serviços
make restart     # Reiniciar tudo
make status      # Ver status
make logs        # Ver todos os logs
```

### Logs Específicos
```bash
make logs-api        # Logs da API
make logs-frontend   # Logs do frontend
make logs-simulator  # Logs do simulador
make logs-mqtt       # Logs do MQTT
```

### Debug e Manutenção
```bash
make shell-api       # Acessar container da API
make shell-simulator # Acessar container do simulador
make db-query        # Consultar banco SQLite
make db-latest       # Ver últimos registros
```

### Limpeza
```bash
make clean       # Limpar containers parados
make clean-all   # Remover tudo (cuidado!)
```

## 🔍 Monitoramento

### Verificar Saúde dos Serviços

1. **MQTT Broker**
   ```bash
   # Testar conexão MQTT
   mosquitto_sub -h localhost -t "sensors/+/data" -v
   ```

2. **Simulador Python**
   ```bash
   # Ver logs do simulador
   make logs-simulator

   # Verificar dados no banco
   make db-query
   ```

3. **API .NET Core**
   ```bash
   # Testar endpoint
   curl http://localhost:5000/api/iot/machine/M1

   # Ver logs da API
   make logs-api
   ```

4. **Frontend React**
   ```bash
   # Acessar no browser
   open http://localhost:8080

   # Ver logs do Nginx
   make logs-frontend
   ```

## ⚡ Sequência de Inicialização

O Docker Compose está configurado com dependências corretas:

1. **Mosquitto** (MQTT Broker) inicia primeiro
2. **Simulador** aguarda MQTT e inicia subscriber + sensor
3. **API** aguarda dados no SQLite aparecerem
4. **Frontend** aguarda API estar disponível

## 🐛 Troubleshooting

### Container não inicia
```bash
# Ver logs detalhados
docker-compose logs [service-name]

# Verificar imagens
docker images | grep iot
```

### Banco SQLite vazio
```bash
# Verificar se simulador está rodando
make logs-simulator

# Verificar dados no banco
make db-query
```

### API não responde
```bash
# Verificar se API encontrou o banco
make logs-api

# Testar endpoint
curl -v http://localhost:5000/api/iot
```

### Frontend não carrega
```bash
# Verificar se API está respondendo
make test-api

# Ver logs do Nginx
make logs-frontend
```

### MQTT não conecta
```bash
# Verificar se Mosquitto está rodando
docker-compose ps mosquitto

# Testar conexão
mosquitto_sub -h localhost -t "test" -v
```

## 🔒 Segurança e Produção

Para ambiente de produção, considere:

### 1. MQTT com Autenticação
```bash
# Gerar arquivo de senhas
mosquitto_passwd -c passwords.txt username
```

### 2. API com HTTPS
```yaml
# No docker-compose.yml
environment:
  - ASPNETCORE_URLS=https://+:5000
  - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/cert.pfx
```

### 3. Frontend com SSL
```nginx
# No nginx/default.conf
server {
    listen 443 ssl http2;
    ssl_certificate /etc/ssl/cert.pem;
    ssl_certificate_key /etc/ssl/key.pem;
}
```

## 📊 Performance

### Recursos Recomendados
- **CPU**: 2 cores mínimo
- **RAM**: 4GB mínimo
- **Disk**: 10GB para dados/logs

### Monitoramento
```bash
# Ver uso de recursos
docker stats

# Ver logs de performance
docker-compose logs | grep -E "(error|warning|performance)"
```

## 🔄 Backup e Restore

### Backup dos Dados
```bash
# Backup do banco SQLite
cp data/iot.db backup/iot_$(date +%Y%m%d_%H%M%S).db

# Backup completo
docker-compose down
tar -czf backup/iot_full_$(date +%Y%m%d).tar.gz data/ docker/
```

### Restore
```bash
# Restaurar banco
cp backup/iot_20250821_120000.db data/iot.db

# Reiniciar serviços
make restart
```

---

## 🎯 Resumo de URLs

Após execução bem-sucedida:

| Serviço | URL | Status |
|---------|-----|--------|
| Dashboard | http://localhost:8080 | ✅ Principal |
| API | http://localhost:5000 | ✅ Backend |
| Swagger | http://localhost:5000/swagger | ✅ Docs |
| MQTT | localhost:1883 | ✅ Messaging |

**Comando único para iniciar tudo:**
```bash
make build && make up
```
