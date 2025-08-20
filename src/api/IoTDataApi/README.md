# API IoT Data - ASP.NET Core

Esta API fornece endpoints REST para acessar dados de sensores IoT armazenados em banco SQLite.

## Tecnologias

- **ASP.NET Core 8.0**
- **Entity Framework Core**
- **SQLite**
- **Swagger/OpenAPI**

## Pré-requisitos

### .NET SDK 8.0+
```bash
# Verificar versão  
dotnet --version

# Ubuntu/Debian
sudo apt update
sudo apt install dotnet-sdk-8.0
```

## Estrutura do Projeto

```
IoTDataApi/
├── Controllers/
│   └── IoTController.cs      # Endpoints da API
├── Data/
│   └── IoTDataContext.cs     # Contexto do Entity Framework
├── Program.cs                # Configuração da aplicação
├── IoTDataApi.csproj        # Configurações do projeto
└── appsettings.json         # Configurações
```

## Endpoints

### Base URL
```
http://localhost:5000
```

### 1. Obter todos os dados
```http
GET /api/iot
```

**Response:**
```json
[
  {
    "id": 1,
    "topic": "sensors/M1/data",
    "message": "{\"machine_id\":\"M1\",\"vibration\":10.25,...}",
    "receivedAt": "2025-08-19T19:58:30.804525Z"
  }
]
```

### 2. Obter dados de uma máquina específica
```http
GET /api/iot/machine/{machineId}
```

**Exemplo:**
```http
GET /api/iot/machine/M1
```

**Response:**
```json
[
  {
    "id": 769,
    "topic": "sensors/M1/data",
    "message": "{\"machine_id\":\"M1\",\"vibration\":13.35,\"temperature\":48.14,\"humidity\":77.17,\"pressure\":4.25,\"voltage\":221.38,\"current\":12.64,\"power\":1.44,\"timestamp\":\"2025-08-19T19:58:30.769425Z\"}",
    "receivedAt": "2025-08-19T19:58:30.804525Z"
  }
]
```

## Configuração

### Connection String
A API está configurada para acessar o banco SQLite criado pelo simulador:
```csharp
var dbPath = Path.GetFullPath("../../../simulator/iot.db");
```

### CORS
Configurado para aceitar requisições de:
- `http://localhost:8080` (Frontend)
- `http://localhost:3000` (Frontend alternativo)

## Como Executar

### 1. Navegar para o diretório
```bash
cd src/api/IoTDataApi
```

### 2. Restaurar dependências
```bash
dotnet restore
```

### 3. Executar a aplicação
```bash
dotnet run
```

**Output esperado:**
```
Looking for database at: /path/to/simulator/iot.db
Database found at: /path/to/simulator/iot.db
API starting on http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

### 4. Acessar documentação Swagger
```
http://localhost:5000/swagger
```

## Teste da API

### cURL
```bash
# Testar endpoint geral
curl http://localhost:5000/api/iot

# Testar endpoint específico
curl http://localhost:5000/api/iot/machine/M1
```

### Browser
- Todos os dados: http://localhost:5000/api/iot
- Dados M1: http://localhost:5000/api/iot/machine/M1

## Modelos de Dados

### IoTData Entity
```csharp
public class IoTData
{
    public int Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ReceivedAt { get; set; } = string.Empty;
}
```

### Mapeamento de Tabela
- **Tabela SQLite:** `iot_data`
- **Colunas:** `id`, `topic`, `message`, `received_at`

## Logs e Monitoramento

### Níveis de Log
- **Information:** Logs gerais da aplicação
- **Warning:** Avisos do ASP.NET Core
- **Error:** Erros de execução

### Logs importantes
```bash
# Localização do banco
"Looking for database at: /path/..."

# Confirmação de inicialização
"API starting on http://localhost:5000"

# Erros de banco
"Database error: ..."
```

## Troubleshooting

### Erro: Database file not found
**Solução:**
1. Certifique-se de que o simulador está rodando
2. Verifique se o arquivo `iot.db` existe em `src/simulator/`
3. Execute primeiro o `subscriber.py`

### Erro: SQLite Error 1: 'no such table'
**Solução:**
1. O banco foi criado mas não tem a tabela correta
2. Execute o `subscriber.py` que cria a tabela automaticamente

### Erro de CORS
**Solução:**
1. Verifique se o frontend está rodando na porta correta
2. Atualize as origens CORS no `Program.cs` se necessário

### Performance
- A API limita a 100 registros mais recentes por consulta
- Ordenação por `ReceivedAt` descendente para dados mais atuais

## Desenvolvimento

### Adicionar nova funcionalidade
1. Adicione métodos no `IoTController`
2. Configure rotas com `[HttpGet]`, `[HttpPost]`, etc.
3. Atualize documentação Swagger se necessário

### Exemplo de novo endpoint
```csharp
[HttpGet("machine/{machineId}/latest")]
public async Task<IActionResult> GetLatestMachineData(string machineId)
{
    var data = await _context.IoTData
        .Where(d => d.Topic.Contains(machineId))
        .OrderByDescending(d => d.ReceivedAt)
        .FirstOrDefaultAsync();

    return data != null ? Ok(data) : NotFound();
}
```
