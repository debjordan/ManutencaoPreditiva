# Dashboard IoT - Frontend React

Dashboard web responsivo para monitoramento em tempo real de máquinas industriais com dados de sensores IoT.

## Tecnologias

- **React 18** com TypeScript
- **Tailwind CSS** para estilização
- **Webpack 5** para bundling
- **Fetch API** para consumo da API

## Funcionalidades

- 📊 **Monitoramento em Tempo Real** - Atualização automática a cada 5 segundos
- 🚨 **Sistema de Alertas** - Status visual baseado em thresholds dos sensores
- 📱 **Design Responsivo** - Layout adaptativo para diferentes telas
- 🎨 **Interface Moderna** - UI clean com Tailwind CSS
- ⚡ **Performance** - Componentes otimizados com React

## Status dos Sensores

### Thresholds de Alerta

| Sensor | Normal | Alerta | Crítico |
|--------|--------|--------|---------|
| **Vibração** | ≤ 10.0 | 10.1-12.0 | > 12.0 |
| **Temperatura** | ≤ 55°C | 55.1-60°C | > 60°C |
| **Pressão** | ≤ 5.0 bar | 5.1-5.5 bar | > 5.5 bar |

### Cores dos Status
- 🟢 **NORMAL** - Verde
- 🟡 **ALERTA** - Amarelo
- 🔴 **CRÍTICO** - Vermelho

## Pré-requisitos

### Node.js 18+
```bash
# Verificar versão
node --version
npm --version

# Ubuntu/Debian
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs
```

## Instalação

### 1. Navegar para o diretório
```bash
cd src/client/iot-dashboard
```

### 2. Instalar dependências
```bash
npm install
```

### 3. Iniciar o servidor de desenvolvimento
```bash
npm start
```

### 4. Acessar a aplicação
```
http://localhost:8080
```

## Scripts Disponíveis

```bash
# Desenvolvimento
npm start

# Build para produção
npm run build

# Servidor de desenvolvimento
npm run dev
```

## Estrutura do Projeto

```
src/
├── components/
│   └── MachineCard.tsx      # Componente de card da máquina
├── App.tsx                  # Componente principal
├── index.tsx               # Ponto de entrada
└── styles.css              # Estilos Tailwind
```

## Componentes

### App.tsx
- **Gerenciamento de Estado** - Controla dados das 5 máquinas
- **Polling** - Atualização automática a cada 5 segundos
- **Error Handling** - Tratamento de erros e estados de loading
- **Layout Responsivo** - Grid adaptativo

### MachineCard.tsx
- **Exibição de Dados** - Sensores principais e secundários
- **Status Visual** - Cores condicionais baseadas em thresholds
- **Timestamp** - Última atualização formatada
- **Parsing JSON** - Processamento dos dados da API

## Dados Exibidos

### Sensores Principais (Grandes)
- **Vibração** - Valor com cores condicionais
- **Temperatura** - °C com cores condicionais
- **Pressão** - bar com cores condicionais
- **Umidade** - % valor neutro

### Sensores Secundários (Pequenos)
- **Tensão** - V (Volts)
- **Corrente** - A (Amperes)
- **Potência** - kW (Quilowatts)

## API Integration

### Endpoint Consumido
```typescript
const machines = ['M1', 'M2', 'M3', 'M4', 'M5'];

for (const machine of machines) {
  const response = await fetch(
    `http://localhost:5000/api/iot/machine/${machine}`
  );
  const data = await response.json();
}
```

### Formato de Dados Esperado
```typescript
interface SensorData {
  id: number;
  topic: string;
  message: string; // JSON string
  receivedAt: string;
}
```

### Formato do Message (JSON)
```json
{
  "machine_id": "M1",
  "vibration": 10.25,
  "temperature": 52.3,
  "humidity": 65.8,
  "pressure": 4.8,
  "voltage": 230.5,
  "current": 12.3,
  "power": 2.8,
  "timestamp": "2025-08-19T19:58:30.769425Z"
}
```

## Estados da Aplicação

### Loading
- Tela de carregamento com spinner
- Exibido durante a primeira carga

### Error
- Tela de erro com botão "Tentar Novamente"
- Exibido quando API não responde

### Success
- Dashboard com cards das máquinas
- Atualizações automáticas em background

### Empty
- Mensagem quando não há dados
- Orientação para verificar simulador/API

## Customização

### Alterar Intervalo de Atualização
```typescript
// Em App.tsx, linha do setInterval
const interval = setInterval(fetchData, 5000); // 5 segundos
```

### Alterar Thresholds de Alerta
```typescript
// Em MachineCard.tsx, função getStatus()
if (vibration > 12 || temperature > 60 || pressure > 5.5) {
  return { level: 'CRÍTICO', color: 'bg-red-200 text-red-800' };
}
```

### Adicionar Novos Sensores
1. Atualizar interface `ParsedSensorData`
2. Adicionar campos no JSX do `MachineCard`
3. Incluir na lógica de status se necessário

## Responsividade

### Breakpoints Tailwind
- **Mobile** - `grid-cols-1` (1 coluna)
- **Tablet** - `md:grid-cols-2` (2 colunas)
- **Desktop** - `lg:grid-cols-3` (3 colunas)

### Layout Adaptativo
```css
<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
```

## Troubleshooting

### Tela em branco
**Possíveis causas:**
1. API não está rodando (porta 5000)
2. Erro de CORS na API
3. Erro de JavaScript no console

**Verificar:**
```bash
# Console do navegador (F12)
# Network tab para ver requisições
# Verificar se API responde
curl http://localhost:5000/api/iot/machine/M1
```

### Cards mostram "Aguardando dados..."
**Possíveis causas:**
1. Simulador não está rodando
2. Banco SQLite vazio
3. API retornando array vazio

**Verificar:**
```bash
# Verificar dados no banco
sqlite3 src/simulator/iot.db "SELECT COUNT(*) FROM iot_data;"

# Verificar resposta da API
curl http://localhost:5000/api/iot/machine/M1
```

### Erro "Cannot read properties of undefined"
**Possível causa:** Mudança no formato dos dados da API

**Verificar:** Console do navegador para ver estrutura exata dos dados

### Status sempre "DESCONHECIDO"
**Possível causa:** Erro no parsing do JSON da mensagem

**Verificar:** Logs do console para erros de JSON.parse()

## Performance

### Otimizações Implementadas
- **useEffect** com cleanup para evitar memory leaks
- **Conditional rendering** para melhor UX
- **Error boundaries** implícitos via try/catch
- **Minimal re-renders** via proper state management

### Monitoramento
- Todos os erros são logados no console
- Network requests visíveis na aba Network (F12)
- Performance pode ser monitorada via React DevTools
