**Manutenção Preditiva (IoT)**

Uma solução completa para monitoramento de máquinas industriais voltada a manutenção preditiva. Inclui:

- Simulador de sensores (MQTT)
- Subscriber que persiste dados em SQLite
- API REST em ASP.NET Core para consulta e integração
- Dashboard web em React/TypeScript para visualização em tempo real

![](https://via.placeholder.com/1000x400?text=IoT+Dashboard+Preview)

Por que comercializar?
- Produto focado em fábricas pequenas e médias que querem reduzir downtime e custos com manutenção.
- Arquitetura simples para deploy on-premises ou em nuvem, com integração por API e MQTT.

Badges
- Build (GitHub Actions): ![CI](https://github.com/<OWNER>/<REPO>/actions/workflows/ci.yml/badge.svg)
- Release (on tags): ![Release](https://github.com/<OWNER>/<REPO>/actions/workflows/release.yml/badge.svg)
- License: ![License](https://img.shields.io/badge/license-MIT-green)
- Docker: ![Docker Compose](https://img.shields.io/badge/docker-compose-ready-brightgreen)

Screenshots
- Demo dashboard: ![Demo](https://via.placeholder.com/1000x400?text=Demo+Dashboard)


Principais recursos
- Monitoramento em tempo real com alertas por thresholds
- Painel responsivo e exportável
- Fácil integração via API REST
- Simulador para demonstrações e POC

Rápido start (resumido)

1. Clonar

```bash
git clone <repository-url>
cd ManutencaoPreditiva
```

2. Dependências principais

```bash
# .NET
dotnet --version

# Node
node --version

# Python (simulador)
python --version
```

3. Executar local (modo rápido)

Terminal 1 — Subscriber (grava dados)

```bash
cd src/simulator
pip install -r requirements.txt || pip install paho-mqtt
python subscriber.py
```

Terminal 2 — Simulador

```bash
cd src/simulator
python sensor_simulator.py
```

Terminal 3 — API

```bash
cd src/api/IoTDataApi
dotnet restore
dotnet run --urls=http://localhost:5000
```

Terminal 4 — Frontend

```bash
cd src/client/iot-dashboard
npm ci
npm start
```

Abra: http://localhost:8080

Estrutura do repositório

```
ManutencaoPreditiva/
├── src/
│   ├── simulator/                 # Simulador MQTT e subscriber
│   ├── api/                       # ASP.NET Core API
│   └── client/                    # React dashboard
├── docker/                        # Dockerfiles e configs
├── .github/workflows/ci.yml       # Pipeline CI
├── CONTRIBUTING.md                # Como contribuir
└── LICENSE
```

Próximos passos para profissionalização

- Completar pipeline de CI/CD (build -> images -> release)
- Docker Compose para demo e POC
- Adicionar testes automatizados e cobertura
- Criar documentação técnica e comercial (one-pager e screenshots)
- Preparar kit de demonstração com dados sintéticos e guia de venda

Contato comercial

Abrir uma issue com etiqueta `commercial` ou enviar email para contato@example.com

----

Se quiser, posso começar agora adicionando: `docker-compose` para demo, scripts de build e um roteiro de POC comercial. Indique quais itens priorizar.
