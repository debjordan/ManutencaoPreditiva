import React, { useState } from 'react';
import MachineDetailsModal from './MachineDetailsModal';
import './MachineCard.scss';

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

interface MachineCardProps {
  machineId: string;
  data: SensorData[];
}

const MachineCard: React.FC<MachineCardProps> = ({ machineId, data }) => {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const latestData = data && data.length > 0 ? data[0] : null;

  let parsedData: ParsedSensorData | null = null;

  if (latestData && latestData.message) {
    try {
      parsedData = JSON.parse(latestData.message);
    } catch (error) {
      console.error(`Error parsing data for ${machineId}:`, error);
    }
  }

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

  const formatTime = (timestamp: string) => {
    try {
      const date = new Date(timestamp);
      return date.toLocaleTimeString('pt-BR');
    } catch {
      return 'N/A';
    }
  };

  const status = getStatus();

  return (
    <>
      <div className="machine-card">
        <div className="machine-card-header">
          <h3 className="machine-card-title">Máquina {machineId}</h3>
          <span className={`machine-card-status ${status.color}`}>
            {status.level}
          </span>
        </div>

        {parsedData ? (
          <div className="machine-card-content">
            <div className="machine-card-grid">
              <div className="machine-card-metric">
                <p className="machine-card-metric-label">Vibração</p>
                <p className={`machine-card-metric-value ${parsedData.vibration > 12 ? 'text-red-600' : parsedData.vibration > 10 ? 'text-yellow-600' : 'text-gray-800'}`}>
                  {parsedData.vibration?.toFixed(2) || 'N/A'}
                </p>
              </div>

              <div className="machine-card-metric">
                <p className="machine-card-metric-label">Temperatura</p>
                <p className={`machine-card-metric-value ${parsedData.temperature > 60 ? 'text-red-600' : parsedData.temperature > 55 ? 'text-yellow-600' : 'text-gray-800'}`}>
                  {parsedData.temperature?.toFixed(1) || 'N/A'}°C
                </p>
              </div>

              <div className="machine-card-metric">
                <p className="machine-card-metric-label">Pressão</p>
                <p className={`machine-card-metric-value ${parsedData.pressure > 5.5 ? 'text-red-600' : parsedData.pressure > 5.0 ? 'text-yellow-600' : 'text-gray-800'}`}>
                  {parsedData.pressure?.toFixed(2) || 'N/A'} bar
                </p>
              </div>

              <div className="machine-card-metric">
                <p className="machine-card-metric-label">Umidade</p>
                <p className="machine-card-metric-value text-gray-800">
                  {parsedData.humidity?.toFixed(1) || 'N/A'}%
                </p>
              </div>
            </div>

            <div className="machine-card-secondary-grid">
              <div className="machine-card-secondary-metric">
                <p className="machine-card-secondary-label">Tensão</p>
                <p className="machine-card-secondary-value">{parsedData.voltage?.toFixed(0) || 'N/A'}V</p>
              </div>
              <div className="machine-card-secondary-metric">
                <p className="machine-card-secondary-label">Corrente</p>
                <p className="machine-card-secondary-value">{parsedData.current?.toFixed(1) || 'N/A'}A</p>
              </div>
              <div className="machine-card-secondary-metric">
                <p className="machine-card-secondary-label">Potência</p>
                <p className="machine-card-secondary-value">{parsedData.power?.toFixed(1) || 'N/A'}kW</p>
              </div>
            </div>

            <div className="machine-card-footer">
              <p className="machine-card-timestamp">
                Última atualização: {latestData ? formatTime(latestData.receivedAt) : 'N/A'}
              </p>
            </div>
          </div>
        ) : (
          <div className="machine-card-empty">
            <div className="machine-card-empty-icon">📊</div>
            <p className="machine-card-empty-text">Aguardando dados...</p>
            <p className="machine-card-empty-subtext">
              {data.length > 0 ? 'Erro ao processar dados' : 'Nenhum dado recebido'}
            </p>
          </div>
        )}

        <button
          onClick={() => setIsModalOpen(true)}
          className="machine-card-button"
          title="Ver detalhes completos"
        >
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M9 12L11 14L15 10M21 12C21 16.9706 16.9706 21 12 21C7.02944 21 3 16.9706 3 12C3 7.02944 7.02944 3 12 3C16.9706 3 21 7.02944 21 12Z" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
          </svg>
        </button>
      </div>

      <MachineDetailsModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        machineId={machineId}
        parsedData={parsedData}
        latestData={latestData}
      />
    </>
  );
};

export default MachineCard;
