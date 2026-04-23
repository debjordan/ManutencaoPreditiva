import React, { useState } from 'react';
import MachineDetailsModal from './MachineDetailsModal';
import { MachineStats, SensorData, RulData, MachineTrends, DowntimeSummary } from '../App';
import './MachineCard.scss';

interface MachineCardProps {
  machineId: string;
  data: SensorData[];
  stats: MachineStats | null;
  rul: RulData | null;
  trends: MachineTrends | null;
  downtime: DowntimeSummary | null;
}

interface ParsedSensorData {
  machine_id: string;
  machine_name?: string;
  area?: string;
  state?: string;
  vibration: number;
  temperature: number;
  pressure: number;
  humidity?: number;
  voltage?: number;
  current?: number;
  power?: number;
  timestamp: string;
}

const TREND_ICON: Record<string, string> = {
  subindo: '↑',
  caindo:  '↓',
  estável: '→',
};
const TREND_COLOR: Record<string, string> = {
  subindo: 'text-red-400',
  caindo:  'text-green-400',
  estável: 'text-gray-500',
};

const MachineCard: React.FC<MachineCardProps> = ({ machineId, data, stats, rul, trends, downtime }) => {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const latestData = data && data.length > 0 ? data[0] : null;

  let parsedData: ParsedSensorData | null = null;
  if (latestData?.message) {
    try { parsedData = JSON.parse(latestData.message); } catch { /* ignore */ }
  }

  const machineName = stats?.machineName ?? parsedData?.machine_name ?? machineId;
  const area        = stats?.area        ?? parsedData?.area          ?? '—';

  const getStatus = () => {
    const state = stats?.currentState ?? parsedData?.state ?? 'normal';
    if (state === 'critical')  return { level: 'CRÍTICO',    color: 'bg-red-900/60 text-red-300 border-red-700' };
    if (state === 'degrading') return { level: 'DEGRADANDO', color: 'bg-yellow-900/60 text-yellow-300 border-yellow-700' };
    if (!parsedData) return { level: 'DESCONHECIDO', color: 'bg-gray-700 text-gray-300' };
    const { vibration = 0, temperature = 0, pressure = 0 } = parsedData;
    if (vibration > 12 || temperature > 60 || pressure > 5.5)
      return { level: 'CRÍTICO',  color: 'bg-red-900/60 text-red-300 border-red-700' };
    if (vibration > 10 || temperature > 55 || pressure > 5.0)
      return { level: 'ALERTA',   color: 'bg-yellow-900/60 text-yellow-300 border-yellow-700' };
    return { level: 'NORMAL', color: 'bg-green-900/60 text-green-300 border-green-700' };
  };

  const status    = getStatus();
  const oee       = stats?.oee       ?? null;
  const riskScore = stats?.riskScore ?? null;
  const oeeColor  = oee       !== null ? (oee >= 85      ? 'text-green-400' : oee >= 65      ? 'text-yellow-400' : 'text-red-400') : 'text-gray-500';
  const riskColor = riskScore !== null ? (riskScore < 40 ? 'text-green-400' : riskScore < 60 ? 'text-yellow-400' : 'text-red-400') : 'text-gray-500';

  // RUL badge
  const rulHours = rul?.estimatedHoursToFailure ?? null;
  const rulColor = rulHours !== null
    ? rulHours < 2  ? 'bg-red-600 text-white'
    : rulHours < 8  ? 'bg-yellow-500 text-gray-900'
    : 'bg-blue-700 text-blue-100'
    : '';

  // trend lookup helpers
  const trendFor = (sensor: string) => trends?.sensors.find(s => s.sensor === sensor);

  return (
    <>
      <div className="bg-gray-900 border border-gray-800 rounded-2xl overflow-hidden">
        {/* header */}
        <div className="px-4 pt-4 pb-3 border-b border-gray-800 flex items-start justify-between gap-2">
          <div>
            <p className="text-xs text-gray-500 uppercase tracking-wider">{area}</p>
            <h3 className="text-base font-semibold text-white leading-tight">{machineName}</h3>
            <p className="text-xs text-gray-600">{machineId}</p>
          </div>
          <div className="flex flex-col items-end gap-1">
            <span className={`px-2 py-0.5 rounded-full text-xs font-bold border ${status.color}`}>
              {status.level}
            </span>
            {rulHours !== null && (
              <span className={`px-2 py-0.5 rounded-full text-xs font-bold ${rulColor}`}>
                RUL ~{rulHours.toFixed(1)}h
              </span>
            )}
          </div>
        </div>

        {parsedData ? (
          <div className="px-4 py-3 space-y-3">
            {/* KPI row */}
            <div className="grid grid-cols-3 gap-2 text-center">
              <div className="bg-gray-800 rounded-lg py-2">
                <p className={`text-lg font-bold ${oeeColor}`}>{oee !== null ? `${oee}%` : '—'}</p>
                <p className="text-xs text-gray-500">OEE</p>
              </div>
              <div className="bg-gray-800 rounded-lg py-2">
                <p className={`text-lg font-bold ${riskColor}`}>{riskScore !== null ? `${riskScore}%` : '—'}</p>
                <p className="text-xs text-gray-500">Risco</p>
              </div>
              <div className="bg-gray-800 rounded-lg py-2">
                <p className="text-lg font-bold text-blue-400">{parsedData.power?.toFixed(1) ?? '—'}</p>
                <p className="text-xs text-gray-500">kW</p>
              </div>
            </div>

            {/* sensor grid with trend arrows */}
            <div className="grid grid-cols-2 gap-x-4 gap-y-1.5">
              {[
                { label: 'Vibração',    value: parsedData.vibration,   unit: 'mm/s', warn: 10,  crit: 12,  sensor: 'vibration' },
                { label: 'Temperatura', value: parsedData.temperature, unit: '°C',   warn: 55,  crit: 60,  sensor: 'temperature' },
                { label: 'Pressão',     value: parsedData.pressure,    unit: 'bar',  warn: 5.0, crit: 5.5, sensor: 'pressure' },
                { label: 'Umidade',     value: parsedData.humidity,    unit: '%',    warn: 70,  crit: 80,  sensor: 'humidity' },
                { label: 'Corrente',    value: parsedData.current,     unit: 'A',    warn: 18,  crit: 21,  sensor: 'current' },
                { label: 'Tensão',      value: parsedData.voltage,     unit: 'V',    warn: 235, crit: 245, sensor: null },
              ].map(s => {
                const v     = s.value ?? 0;
                const color = v > s.crit ? 'text-red-400' : v > s.warn ? 'text-yellow-400' : 'text-gray-300';
                const trend = s.sensor ? trendFor(s.sensor) : null;
                return (
                  <div key={s.label} className="flex justify-between items-center">
                    <span className="text-xs text-gray-500">{s.label}</span>
                    <span className="flex items-center gap-1">
                      {trend && (
                        <span className={`text-xs font-bold ${TREND_COLOR[trend.direction]}`}>
                          {TREND_ICON[trend.direction]}
                        </span>
                      )}
                      <span className={`text-xs font-mono font-semibold ${color}`}>
                        {s.value !== undefined
                          ? `${s.value.toFixed(s.unit === '°C' ? 1 : 2)} ${s.unit}`
                          : 'N/A'}
                      </span>
                    </span>
                  </div>
                );
              })}
            </div>

            {/* downtime strip */}
            {downtime && downtime.totalReadingsAnalyzed > 0 && (
              <div className="bg-gray-800 rounded-lg px-3 py-2 flex justify-between items-center text-xs">
                <span className="text-gray-500">Disponibilidade 24h</span>
                <span className={downtime.availabilityPct >= 95 ? 'text-green-400 font-bold' : downtime.availabilityPct >= 85 ? 'text-yellow-400 font-bold' : 'text-red-400 font-bold'}>
                  {downtime.availabilityPct}%
                </span>
                <span className="text-gray-500">Downtime</span>
                <span className="text-gray-300 font-mono">{downtime.downtimeMinutes.toFixed(0)}min</span>
                {downtime.estimatedCostBrl > 0 && (
                  <>
                    <span className="text-gray-500">Custo</span>
                    <span className="text-red-400 font-mono">R${downtime.estimatedCostBrl.toFixed(0)}</span>
                  </>
                )}
              </div>
            )}

            {/* footer */}
            <div className="flex items-center justify-between pt-1">
              <p className="text-xs text-gray-600">
                {latestData ? new Date(latestData.receivedAt).toLocaleTimeString('pt-BR') : '—'}
              </p>
              <button
                onClick={() => setIsModalOpen(true)}
                className="text-xs px-3 py-1 rounded-lg bg-blue-800 hover:bg-blue-700 text-blue-200 transition"
              >
                Ver detalhes →
              </button>
            </div>
          </div>
        ) : (
          <div className="px-4 py-8 text-center text-gray-600">
            <p className="text-3xl mb-2">📡</p>
            <p className="text-sm">{data.length > 0 ? 'Erro ao processar dados' : 'Aguardando dados...'}</p>
          </div>
        )}
      </div>

      <MachineDetailsModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        machineId={machineId}
        machineName={machineName}
        area={area}
        parsedData={parsedData}
        latestData={latestData}
        stats={stats}
        rul={rul}
        trends={trends}
        downtime={downtime}
      />
    </>
  );
};

export default MachineCard;
