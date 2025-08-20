import React from 'react';
import './MachineDetailsModal.scss';

interface SensorData {
  id: number;
  topic: string;
  message: string;
  receivedAt: string;
}

interface ParsedSensorData {
  machine_id: string;
  vibration: number;
  temperature: number;
  pressure: number;
  humidity?: number;
  voltage?: number;
  current?: number;
  power?: number;
  timestamp: string;
}

interface MachineDetailsModalProps {
  isOpen: boolean;
  onClose: () => void;
  machineId: string;
  parsedData: ParsedSensorData | null;
  latestData: SensorData | null;
}

const MachineDetailsModal: React.FC<MachineDetailsModalProps> = ({
  isOpen,
  onClose,
  machineId,
  parsedData,
  latestData,
}) => {
  const getStatus = () => {
    if (!parsedData) return { level: 'DESCONHECIDO', color: 'bg-gray-200 text-gray-800' };

    const { vibration = 0, temperature = 0, pressure = 0 } = parsedData;

    if (vibration > 12 || temperature > 60 || pressure > 5.5) {
      return { level: 'CRÍTICO', color: 'bg-red-200 text-red-800 border-red-300' };
    } else if (vibration > 10 || temperature > 55 || pressure > 5.0) {
      return { level: 'ALERTA', color: 'bg-yellow-200 text-yellow-800 border-yellow-300' };
    } else {
      return { level: 'NORMAL', color: 'bg-green-200 text-green-800 border-green-300' };
    }
  };

  const getRiskScore = () => {
    if (!parsedData) return 0;
    const { vibration = 0, temperature = 0, pressure = 0 } = parsedData;

    const vibrationRisk = Math.min((vibration / 15) * 100, 100);
    const temperatureRisk = Math.min((temperature / 70) * 100, 100);
    const pressureRisk = Math.min((pressure / 6) * 100, 100);

    return Math.round((vibrationRisk + temperatureRisk + pressureRisk) / 3);
  };

  const getMaintenanceRecommendation = () => {
    const status = getStatus();
    const riskScore = getRiskScore();

    if (status.level === 'CRÍTICO') {
      return {
        urgency: 'IMEDIATA',
        action: 'Parar máquina e inspecionar componentes',
        timeframe: 'Próximas 2 horas',
        cost: 'R$ 2.500 - R$ 5.000',
        color: 'text-red-600',
      };
    } else if (status.level === 'ALERTA') {
      return {
        urgency: 'PRIORITÁRIA',
        action: 'Agendar manutenção preventiva',
        timeframe: 'Próximos 7 dias',
        cost: 'R$ 800 - R$ 1.500',
        color: 'text-yellow-600',
      };
    } else {
      return {
        urgency: 'PROGRAMADA',
        action: 'Manutenção de rotina',
        timeframe: 'Próximos 30 dias',
        cost: 'R$ 200 - R$ 500',
        color: 'text-green-600',
      };
    }
  };

  const generateRiskTrendData = () => {
    const baseRisk = getRiskScore();
    const data = [];
    for (let i = 23; i >= 0; i--) {
      const variation = Math.random() * 20 - 10;
      const risk = Math.max(0, Math.min(100, baseRisk + variation));
      data.push({
        time: `${String(new Date().getHours() - i).padStart(2, '0')}:00`,
        risk: Math.round(risk),
      });
    }
    return data;
  };

  const status = getStatus();
  const riskScore = getRiskScore();
  const maintenance = getMaintenanceRecommendation();
  const riskTrendData = generateRiskTrendData();

  if (!isOpen) return null;

  return (
    <div className="modal-container">
      <div className="modal-content">
        <div className="modal-header">
          <div className="modal-header-content">
            <h2 className="modal-title">Detalhes - Máquina {machineId}</h2>
            <span className={`modal-status ${status.color}`}>
              {status.level}
            </span>
          </div>
          <button onClick={onClose} className="modal-close-button">
            ×
          </button>
        </div>

        <div className="modal-body">
          <div className="executive-summary">
            <h3 className="section-title">📊 Resumo Executivo</h3>
            <div className="summary-grid">
              <div className="summary-item">
                <div className="summary-value text-blue-600">{riskScore}%</div>
                <div className="summary-label">Score de Risco</div>
              </div>
              <div className="summary-item">
                <div className="summary-value text-green-600">
                  {parsedData?.power ? `${parsedData.power.toFixed(1)}kW` : 'N/A'}
                </div>
                <div className="summary-label">Consumo Atual</div>
              </div>
              <div className="summary-item">
                <div className="summary-value text-purple-600">98.2%</div>
                <div className="summary-label">Eficiência Operacional</div>
              </div>
            </div>
          </div>

          <div className="risk-trend">
            <h3 className="section-title">📈 Tendência de Risco (24h)</h3>
            <div className="chart-container">
              <svg className="chart-svg" viewBox="0 0 800 200">
                <defs>
                  <linearGradient id="riskGradient" x1="0%" y1="0%" x2="0%" y2="100%">
                    <stop offset="0%" style={{ stopColor: '#f59e0b', stopOpacity: 0.3 }} />
                    <stop offset="100%" style={{ stopColor: '#f59e0b', stopOpacity: 0.1 }} />
                  </linearGradient>
                </defs>

                {[0, 25, 50, 75, 100].map((y) => (
                  <line
                    key={y}
                    x1="50"
                    y1={200 - y * 1.5}
                    x2="750"
                    y2={200 - y * 1.5}
                    stroke="#e5e7eb"
                    strokeWidth="1"
                  />
                ))}

                <polyline
                  fill="url(#riskGradient)"
                  stroke="#f59e0b"
                  strokeWidth="3"
                  strokeLinejoin="round"
                  strokeLinecap="round"
                  points={riskTrendData
                    .map((point, index) => `${50 + index * 29},${200 - point.risk * 1.5}`)
                    .join(' ')}
                />

                {[0, 25, 50, 75, 100].map((y) => (
                  <text
                    key={y}
                    x="30"
                    y={205 - y * 1.5}
                    textAnchor="middle"
                    className="chart-label-y"
                  >
                    {y}
                  </text>
                ))}

                {riskTrendData
                  .filter((_, index) => index % 4 === 0)
                  .map((point, index) => (
                    <text
                      key={index}
                      x={50 + index * 116}
                      y="220"
                      textAnchor="middle"
                      className="chart-label-x"
                    >
                      {point.time}
                    </text>
                  ))}
              </svg>
            </div>
          </div>

          <div className="details-grid">
            <div className="sensors-details">
              <h3 className="section-title">🔧 Sensores Detalhados</h3>
              {parsedData ? (
                <div className="sensors-content">
                  {[
                    { label: 'Vibração', value: parsedData.vibration?.toFixed(2), unit: 'mm/s', threshold: 12, warning: 10 },
                    { label: 'Temperatura', value: parsedData.temperature?.toFixed(1), unit: '°C', threshold: 60, warning: 55 },
                    { label: 'Pressão', value: parsedData.pressure?.toFixed(2), unit: 'bar', threshold: 5.5, warning: 5.0 },
                    { label: 'Umidade', value: parsedData.humidity?.toFixed(1), unit: '%', threshold: 80, warning: 70 },
                    { label: 'Tensão', value: parsedData.voltage?.toFixed(0), unit: 'V', threshold: 250, warning: 240 },
                    { label: 'Corrente', value: parsedData.current?.toFixed(1), unit: 'A', threshold: 20, warning: 18 },
                  ].map((sensor, index) => {
                    const value = parseFloat(sensor.value || '0');
                    const isHigh = value > sensor.threshold;
                    const isWarning = value > sensor.warning && value <= sensor.threshold;

                    return (
                      <div key={index} className="sensor-item">
                        <span className="sensor-label">{sensor.label}</span>
                        <div className="sensor-value-container">
                          <span
                            className={`sensor-value ${
                              isHigh ? 'text-red-600' : isWarning ? 'text-yellow-600' : 'text-gray-800'
                            }`}
                          >
                            {sensor.value || 'N/A'} {sensor.unit}
                          </span>
                          <div
                            className={`sensor-indicator ${
                              isHigh ? 'bg-red-500' : isWarning ? 'bg-yellow-500' : 'bg-green-500'
                            }`}
                          ></div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              ) : (
                <p className="sensors-empty">Dados não disponíveis</p>
              )}
            </div>

            <div className="maintenance-recommendations">
              <h3 className="section-title">🔨 Recomendações de Manutenção</h3>
              <div className="maintenance-content">
                <div className="maintenance-item">
                  <div className="maintenance-urgency">
                    <span className="maintenance-label">Urgência:</span>
                    <span className={`maintenance-value ${maintenance.color}`}>
                      {maintenance.urgency}
                    </span>
                  </div>
                  <div className="maintenance-action">{maintenance.action}</div>
                  <div className="maintenance-details">
                    <div>
                      <span className="maintenance-label">Prazo:</span>
                      <div className="maintenance-value">{maintenance.timeframe}</div>
                    </div>
                    <div>
                      <span className="maintenance-label">Custo Estimado:</span>
                      <div className="maintenance-value">{maintenance.cost}</div>
                    </div>
                  </div>
                </div>

                <div className="maintenance-history">
                  <h4 className="maintenance-history-title">Histórico Recente</h4>
                  <div className="history-items">
                    <div className="history-item">
                      <span className="history-label">Última manutenção:</span>
                      <span className="history-value">15/08/2025</span>
                    </div>
                    <div className="history-item">
                      <span className="history-label">Tipo:</span>
                      <span className="history-value">Preventiva</span>
                    </div>
                    <div className="history-item">
                      <span className="history-label">Próxima programada:</span>
                      <span className="history-value">20/09/2025</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div className="kpis">
            <h3 className="section-title">💼 KPIs de Negócio</h3>
            <div className="kpis-grid">
              <div className="kpi-item bg-blue-50">
                <div className="kpi-value text-blue-600">99.2%</div>
                <div className="kpi-label">Disponibilidade</div>
              </div>
              <div className="kpi-item bg-green-50">
                <div className="kpi-value text-green-600">R$ 1.2M</div>
                <div className="kpi-label">Receita Gerada (mês)</div>
              </div>
              <div className="kpi-item bg-yellow-50">
                <div className="kpi-value text-yellow-600">R$ 8.5K</div>
                <div className="kpi-label">Custo Manutenção (mês)</div>
              </div>
              <div className="kpi-item bg-purple-50">
                <div className="kpi-value text-purple-600">2.1h</div>
                <div className="kpi-label">MTTR Médio</div>
              </div>
            </div>
          </div>

          <div className="alerts">
            <h3 className="section-title">🔔 Alertas Ativos</h3>
            <div className="alerts-content">
              {status.level === 'CRÍTICO' && (
                <div className="alert-item bg-red-50 border-red-200">
                  <div className="alert-indicator bg-red-500"></div>
                  <span className="alert-text text-red-800">
                    Vibração acima do limite crítico - Ação imediata necessária
                  </span>
                </div>
              )}
              {status.level === 'ALERTA' && (
                <div className="alert-item bg-yellow-50 border-yellow-200">
                  <div className="alert-indicator bg-yellow-500"></div>
                  <span className="alert-text text-yellow-800">
                    Parâmetros em zona de atenção - Monitoramento intensivo
                  </span>
                </div>
              )}
              {status.level === 'NORMAL' && (
                <div className="alert-item bg-green-50 border-green-200">
                  <div className="alert-indicator bg-green-500"></div>
                  <span className="alert-text text-green-800">
                    Todos os parâmetros dentro da normalidade
                  </span>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default MachineDetailsModal;
