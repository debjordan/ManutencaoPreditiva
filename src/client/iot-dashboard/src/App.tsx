import React, { useCallback, useEffect, useState } from 'react';
import AlertPanel from './components/AlertPanel';
import MachineCard from './components/MachineCard';

declare const process: { env: { API_BASE_URL: string } };
const API_BASE = process.env.API_BASE_URL;

export interface SensorData {
  id: number;
  topic: string;
  message: string;
  receivedAt: string;
}

export interface MachineStats {
  machineId: string;
  machineName: string;
  area: string;
  currentState: string;
  recordCount: number;
  riskScore: number;
  oee: number;
  lastSeen: string;
  vibration:   { min: number; max: number; avg: number; last: number };
  temperature: { min: number; max: number; avg: number; last: number };
  pressure:    { min: number; max: number; avg: number; last: number };
  humidity:    { min: number; max: number; avg: number; last: number };
  voltage:     { min: number; max: number; avg: number; last: number };
  current:     { min: number; max: number; avg: number; last: number };
  power:       { min: number; max: number; avg: number; last: number };
}

export interface Alert {
  machineId: string;
  machineName: string;
  area: string;
  severity: string;
  sensor: string;
  value: number;
  threshold: number;
  message: string;
  detectedAt: string;
}

export interface RulData {
  machineId: string;
  machineName: string;
  limitingSensor: string | null;
  estimatedHoursToFailure: number | null;
  confidence: string;
  confidenceR2: number;
  interpretation: string;
}

export interface SensorTrend {
  sensor: string;
  direction: 'subindo' | 'caindo' | 'estável';
  delta: number;
  last5Avg: number;
  prev5Avg: number;
}

export interface MachineTrends {
  machineId: string;
  machineName: string;
  sensors: SensorTrend[];
}

export interface DowntimeSummary {
  machineId: string;
  machineName: string;
  area: string;
  totalReadingsAnalyzed: number;
  periodHours: number;
  downtimeMinutes: number;
  availabilityPct: number;
  failureEvents: number;
  mttrMinutes: number;
  mtbfHours: number;
  costPerHour: number;
  estimatedCostBrl: number;
}

export interface MaintenanceEvent {
  id: number;
  machineId: string;
  machineName: string;
  type: string;
  startedAt: string;
  resolvedAt: string | null;
  durationMinutes: number | null;
  notes: string | null;
  technicianName: string | null;
  isOpen: boolean;
}

const MACHINES = ['M1', 'M2', 'M3', 'M4', 'M5', 'M6'];

const App: React.FC = () => {
  const [rawData,   setRawData]   = useState<Record<string, SensorData[]>>({});
  const [stats,     setStats]     = useState<Record<string, MachineStats>>({});
  const [alerts,    setAlerts]    = useState<Alert[]>([]);
  const [rulMap,    setRulMap]    = useState<Record<string, RulData>>({});
  const [trendsMap, setTrendsMap] = useState<Record<string, MachineTrends>>({});
  const [downtimeMap, setDowntimeMap] = useState<Record<string, DowntimeSummary>>({});
  const [loading,   setLoading]   = useState(true);
  const [error,     setError]     = useState<string | null>(null);
  const [lastSync,  setLastSync]  = useState<Date | null>(null);

  const fetchAll = useCallback(async () => {
    try {
      const [statsList, alertsList, rulList, trendsList, downtimeList, ...machineResponses] = await Promise.all([
        fetch(`${API_BASE}/api/iot/stats`).then(r => r.json() as Promise<MachineStats[]>),
        fetch(`${API_BASE}/api/iot/alerts`).then(r => r.json() as Promise<Alert[]>),
        fetch(`${API_BASE}/api/iot/rul`).then(r => r.json() as Promise<RulData[]>).catch(() => [] as RulData[]),
        fetch(`${API_BASE}/api/iot/trends`).then(r => r.json() as Promise<MachineTrends[]>).catch(() => [] as MachineTrends[]),
        fetch(`${API_BASE}/api/iot/downtime`).then(r => r.json() as Promise<DowntimeSummary[]>).catch(() => [] as DowntimeSummary[]),
        ...MACHINES.map(m =>
          fetch(`${API_BASE}/api/iot/machine/${m}`)
            .then(r => r.ok ? r.json() as Promise<SensorData[]> : Promise.resolve([]))
            .catch(() => [] as SensorData[])
        ),
      ]);

      const newRaw: Record<string, SensorData[]> = {};
      MACHINES.forEach((m, i) => { newRaw[m] = machineResponses[i] ?? []; });

      const statsMap: Record<string, MachineStats> = {};
      (statsList as MachineStats[]).forEach(s => { statsMap[s.machineId] = s; });

      const newRulMap: Record<string, RulData> = {};
      (rulList as RulData[]).forEach(r => { newRulMap[r.machineId] = r; });

      const newTrendsMap: Record<string, MachineTrends> = {};
      (trendsList as MachineTrends[]).forEach(t => { newTrendsMap[t.machineId] = t; });

      const newDowntimeMap: Record<string, DowntimeSummary> = {};
      (downtimeList as DowntimeSummary[]).forEach(d => { newDowntimeMap[d.machineId] = d; });

      setRawData(newRaw);
      setStats(statsMap);
      setAlerts(alertsList as Alert[]);
      setRulMap(newRulMap);
      setTrendsMap(newTrendsMap);
      setDowntimeMap(newDowntimeMap);
      setError(null);
      setLastSync(new Date());
    } catch (err) {
      console.error('Erro ao buscar dados:', err);
      setError('Falha na comunicação com a API. Verifique se o backend está no ar.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchAll();
    const interval = setInterval(fetchAll, 5000);
    return () => clearInterval(interval);
  }, [fetchAll]);

  const criticalCount = alerts.filter(a => a.severity === 'CRÍTICO').length;
  const alertCount    = alerts.filter(a => a.severity === 'ALERTA').length;

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-950">
        <div className="text-center">
          <div className="animate-spin rounded-full h-16 w-16 border-b-2 border-blue-400 mx-auto mb-4" />
          <p className="text-gray-300 text-lg">Conectando ao chão de fábrica...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-950 text-gray-100">
      <header className="bg-gray-900 border-b border-gray-800 px-6 py-4">
        <div className="max-w-screen-xl mx-auto flex flex-col md:flex-row md:items-center md:justify-between gap-3">
          <div>
            <h1 className="text-2xl font-bold text-white tracking-tight">
              Manutenção Preditiva — Industry 4.0
            </h1>
            <p className="text-sm text-gray-400 mt-0.5">Monitoramento Industrial em Tempo Real</p>
          </div>
          <div className="flex items-center gap-4 flex-wrap">
            {criticalCount > 0 && (
              <span className="px-3 py-1 rounded-full bg-red-600 text-white text-sm font-semibold animate-pulse">
                {criticalCount} CRÍTICO{criticalCount > 1 ? 'S' : ''}
              </span>
            )}
            {alertCount > 0 && (
              <span className="px-3 py-1 rounded-full bg-yellow-500 text-gray-900 text-sm font-semibold">
                {alertCount} ALERTA{alertCount > 1 ? 'S' : ''}
              </span>
            )}
            {criticalCount === 0 && alertCount === 0 && (
              <span className="px-3 py-1 rounded-full bg-green-700 text-white text-sm font-semibold">
                Todas as máquinas normais
              </span>
            )}
            <span className="text-xs text-gray-500">
              {lastSync?.toLocaleTimeString('pt-BR') ?? '--:--:--'}
            </span>
            <button
              onClick={fetchAll}
              className="px-3 py-1 rounded bg-blue-700 hover:bg-blue-600 text-white text-sm transition"
            >
              Atualizar
            </button>
          </div>
        </div>
      </header>

      <main className="max-w-screen-xl mx-auto px-4 py-6 space-y-6">
        <FactoryKpis stats={Object.values(stats)} downtime={Object.values(downtimeMap)} />

        {alerts.length > 0 && <AlertPanel alerts={alerts} />}

        {error && (
          <div className="bg-red-900/40 border border-red-700 rounded-lg px-4 py-3 text-red-300">
            {error}
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
          {MACHINES.map(machineId => (
            <MachineCard
              key={machineId}
              machineId={machineId}
              data={rawData[machineId] ?? []}
              stats={stats[machineId] ?? null}
              rul={rulMap[machineId] ?? null}
              trends={trendsMap[machineId] ?? null}
              downtime={downtimeMap[machineId] ?? null}
            />
          ))}
        </div>
      </main>
    </div>
  );
};

interface FactoryKpisProps {
  stats: MachineStats[];
  downtime: DowntimeSummary[];
}

const FactoryKpis: React.FC<FactoryKpisProps> = ({ stats, downtime }) => {
  const online    = stats.length;
  const avgOee    = online > 0 ? (stats.reduce((s, m) => s + m.oee, 0) / online).toFixed(1) : '—';
  const critical  = stats.filter(m => m.currentState === 'critical').length;
  const degrading = stats.filter(m => m.currentState === 'degrading').length;
  const totalCost = downtime.reduce((s, d) => s + d.estimatedCostBrl, 0);
  const avgAvail  = downtime.length > 0
    ? (downtime.reduce((s, d) => s + d.availabilityPct, 0) / downtime.length).toFixed(1)
    : '—';

  const kpis = [
    { label: 'Máquinas on-line',   value: `${online} / 6`,      color: 'text-blue-400' },
    { label: 'OEE Médio',          value: `${avgOee}%`,          color: Number(avgOee)  >= 85 ? 'text-green-400' : 'text-yellow-400' },
    { label: 'Disponibilidade 24h', value: `${avgAvail}%`,       color: Number(avgAvail) >= 95 ? 'text-green-400' : Number(avgAvail) >= 85 ? 'text-yellow-400' : 'text-red-400' },
    { label: 'Em Estado Crítico',  value: String(critical),      color: critical  > 0 ? 'text-red-400'    : 'text-green-400' },
    { label: 'Em Degradação',      value: String(degrading),     color: degrading > 0 ? 'text-yellow-400' : 'text-green-400' },
    { label: 'Custo Downtime 24h', value: totalCost > 0 ? `R$ ${totalCost.toFixed(0)}` : 'R$ 0', color: totalCost > 1000 ? 'text-red-400' : totalCost > 0 ? 'text-yellow-400' : 'text-green-400' },
  ];

  return (
    <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-3">
      {kpis.map(k => (
        <div key={k.label} className="bg-gray-900 border border-gray-800 rounded-xl px-4 py-3 text-center">
          <p className={`text-2xl font-bold ${k.color}`}>{k.value}</p>
          <p className="text-xs text-gray-500 mt-1">{k.label}</p>
        </div>
      ))}
    </div>
  );
};

export default App;
