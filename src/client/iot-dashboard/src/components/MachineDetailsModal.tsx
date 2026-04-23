import React, { useEffect, useState } from 'react';
import {
    CartesianGrid, Legend, Line, LineChart,
    ReferenceLine, ResponsiveContainer, Tooltip, XAxis, YAxis,
} from 'recharts';
import { DowntimeSummary, MachineTrends, MachineStats, RulData, SensorData } from '../App';
import './MachineDetailsModal.scss';

declare const process: { env: { API_BASE_URL: string } };
const API_BASE = process.env.API_BASE_URL;

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

interface SensorReading {
  machineId: string;
  state: string;
  vibration: number;
  temperature: number;
  pressure: number;
  receivedAt: string;
}

interface MaintenanceEvent {
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

interface MachineDetailsModalProps {
  isOpen: boolean;
  onClose: () => void;
  machineId: string;
  machineName: string;
  area: string;
  parsedData: ParsedSensorData | null;
  latestData: SensorData | null;
  stats: MachineStats | null;
  rul: RulData | null;
  trends: MachineTrends | null;
  downtime: DowntimeSummary | null;
}

type Tab = 'overview' | 'maintenance';

const TREND_ICON: Record<string, string> = { subindo: '↑', caindo: '↓', estável: '→' };
const TREND_COLOR: Record<string, string> = {
  subindo: 'text-red-400', caindo: 'text-green-400', estável: 'text-gray-400',
};

const MachineDetailsModal: React.FC<MachineDetailsModalProps> = ({
  isOpen, onClose, machineId, machineName, area,
  parsedData, stats, rul, trends, downtime,
}) => {
  const [tab, setTab]                       = useState<Tab>('overview');
  const [readings, setReadings]             = useState<SensorReading[]>([]);
  const [loadingChart, setLoadingChart]     = useState(false);
  const [events, setEvents]                 = useState<MaintenanceEvent[]>([]);
  const [loadingEvents, setLoadingEvents]   = useState(false);
  const [showForm, setShowForm]             = useState(false);
  const [formType, setFormType]             = useState('corrective');
  const [formNotes, setFormNotes]           = useState('');
  const [formTech, setFormTech]             = useState('');
  const [submitting, setSubmitting]         = useState(false);

  useEffect(() => {
    if (!isOpen) return;
    setLoadingChart(true);
    fetch(`${API_BASE}/api/iot/machine/${machineId}/readings?limit=60`)
      .then(r => r.json())
      .then((data: SensorReading[]) => setReadings(data.reverse()))
      .catch(() => setReadings([]))
      .finally(() => setLoadingChart(false));
  }, [isOpen, machineId]);

  const loadEvents = () => {
    setLoadingEvents(true);
    fetch(`${API_BASE}/api/maintenance/machine/${machineId}`)
      .then(r => r.json())
      .then((data: MaintenanceEvent[]) => setEvents(data))
      .catch(() => setEvents([]))
      .finally(() => setLoadingEvents(false));
  };

  useEffect(() => {
    if (isOpen && tab === 'maintenance') loadEvents();
  }, [isOpen, tab, machineId]);

  if (!isOpen) return null;

  const state     = stats?.currentState ?? parsedData?.state ?? 'normal';
  const riskScore = stats?.riskScore ?? calcRisk(parsedData);
  const oee       = stats?.oee ?? null;
  const maintenance = getMaintenanceRecommendation(state, riskScore);

  const statusBadge = state === 'critical'
    ? 'bg-red-900/60 text-red-300 border-red-700'
    : state === 'degrading'
      ? 'bg-yellow-900/60 text-yellow-300 border-yellow-700'
      : 'bg-green-900/60 text-green-300 border-green-700';
  const statusLabel = state === 'critical' ? 'CRÍTICO' : state === 'degrading' ? 'DEGRADANDO' : 'NORMAL';

  const chartData = readings.map(r => ({
    time:        new Date(r.receivedAt).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit', second: '2-digit' }),
    Vibração:    r.vibration,
    Temperatura: r.temperature,
    Pressão:     r.pressure,
  }));

  const handleRegister = async () => {
    setSubmitting(true);
    try {
      await fetch(`${API_BASE}/api/maintenance`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          machineId,
          machineName,
          type: formType,
          notes: formNotes || null,
          technicianName: formTech || null,
        }),
      });
      setShowForm(false);
      setFormNotes('');
      setFormTech('');
      loadEvents();
    } catch {
      /* keep form open */
    } finally {
      setSubmitting(false);
    }
  };

  const handleResolve = async (id: number) => {
    await fetch(`${API_BASE}/api/maintenance/${id}/resolve`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ notes: null }),
    });
    loadEvents();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center bg-black/70 overflow-y-auto py-6 px-4">
      <div className="bg-gray-900 border border-gray-700 rounded-2xl w-full max-w-4xl shadow-2xl">
        {/* header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-800">
          <div>
            <div className="flex items-center gap-3">
              <h2 className="text-xl font-bold text-white">{machineName}</h2>
              <span className={`px-2 py-0.5 rounded-full text-xs font-bold border ${statusBadge}`}>
                {statusLabel}
              </span>
            </div>
            <p className="text-sm text-gray-500">{machineId} · {area}</p>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-white text-2xl leading-none transition">×</button>
        </div>

        {/* tabs */}
        <div className="flex border-b border-gray-800">
          {(['overview', 'maintenance'] as Tab[]).map(t => (
            <button
              key={t}
              onClick={() => setTab(t)}
              className={`px-6 py-3 text-sm font-medium transition ${
                tab === t
                  ? 'text-white border-b-2 border-blue-500'
                  : 'text-gray-500 hover:text-gray-300'
              }`}
            >
              {t === 'overview' ? 'Visão Geral' : 'Manutenção'}
            </button>
          ))}
        </div>

        <div className="p-6 space-y-6">
          {tab === 'overview' && (
            <>
              {/* KPI row */}
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
                {[
                  { label: 'Score de Risco', value: `${riskScore}%`,
                    color: riskScore > 60 ? 'text-red-400' : riskScore > 40 ? 'text-yellow-400' : 'text-green-400' },
                  { label: 'OEE', value: oee !== null ? `${oee}%` : '—',
                    color: oee !== null ? (oee >= 85 ? 'text-green-400' : oee >= 65 ? 'text-yellow-400' : 'text-red-400') : 'text-gray-500' },
                  { label: 'Potência', value: parsedData?.power !== undefined ? `${parsedData.power.toFixed(1)} kW` : '—',
                    color: 'text-blue-400' },
                  { label: 'Leituras', value: stats?.recordCount !== undefined ? String(stats.recordCount) : '—',
                    color: 'text-purple-400' },
                ].map(k => (
                  <div key={k.label} className="bg-gray-800 rounded-xl py-3 text-center">
                    <p className={`text-2xl font-bold ${k.color}`}>{k.value}</p>
                    <p className="text-xs text-gray-500 mt-0.5">{k.label}</p>
                  </div>
                ))}
              </div>

              {/* RUL banner */}
              {rul && rul.estimatedHoursToFailure !== null && (
                <div className={`rounded-xl px-4 py-3 border ${
                  rul.estimatedHoursToFailure < 2
                    ? 'bg-red-900/30 border-red-700'
                    : rul.estimatedHoursToFailure < 8
                      ? 'bg-yellow-900/30 border-yellow-700'
                      : 'bg-blue-900/20 border-blue-800'
                }`}>
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-xs text-gray-400 uppercase tracking-wider mb-0.5">
                        Vida Útil Restante (RUL) — sensor: {rul.limitingSensor}
                      </p>
                      <p className={`text-lg font-bold ${
                        rul.estimatedHoursToFailure < 2 ? 'text-red-300'
                        : rul.estimatedHoursToFailure < 8 ? 'text-yellow-300'
                        : 'text-blue-300'
                      }`}>
                        ~{rul.estimatedHoursToFailure.toFixed(1)}h para falha estimada
                      </p>
                      <p className="text-xs text-gray-400 mt-0.5">{rul.interpretation}</p>
                    </div>
                    <div className="text-right text-xs text-gray-500">
                      <p>Confiança</p>
                      <p className="font-semibold text-gray-300">{rul.confidence}</p>
                      <p className="mt-1">R²</p>
                      <p className="font-mono text-gray-300">{rul.confidenceR2.toFixed(3)}</p>
                    </div>
                  </div>
                </div>
              )}

              {/* Downtime strip */}
              {downtime && downtime.totalReadingsAnalyzed > 0 && (
                <div className="bg-gray-800 rounded-xl px-4 py-3">
                  <p className="text-xs text-gray-500 uppercase tracking-wider mb-3">
                    Downtime — últimas {downtime.periodHours.toFixed(1)}h analisadas
                  </p>
                  <div className="grid grid-cols-2 sm:grid-cols-5 gap-3 text-center">
                    {[
                      { label: 'Disponibilidade', value: `${downtime.availabilityPct}%`,
                        color: downtime.availabilityPct >= 95 ? 'text-green-400' : downtime.availabilityPct >= 85 ? 'text-yellow-400' : 'text-red-400' },
                      { label: 'Downtime',  value: `${downtime.downtimeMinutes.toFixed(0)}min`, color: downtime.downtimeMinutes > 30 ? 'text-red-400' : 'text-gray-300' },
                      { label: 'Eventos',   value: String(downtime.failureEvents),  color: downtime.failureEvents > 0 ? 'text-yellow-400' : 'text-green-400' },
                      { label: 'MTTR',      value: downtime.mttrMinutes > 0 ? `${downtime.mttrMinutes}min` : '—', color: 'text-blue-400' },
                      { label: 'MTBF',      value: downtime.mtbfHours > 0 ? `${downtime.mtbfHours}h` : '—',   color: 'text-purple-400' },
                    ].map(k => (
                      <div key={k.label}>
                        <p className={`text-xl font-bold ${k.color}`}>{k.value}</p>
                        <p className="text-xs text-gray-500">{k.label}</p>
                      </div>
                    ))}
                  </div>
                  {downtime.estimatedCostBrl > 0 && (
                    <p className="text-xs text-gray-500 mt-3 text-right">
                      Custo estimado de downtime:{' '}
                      <span className="text-red-400 font-semibold">
                        R$ {downtime.estimatedCostBrl.toFixed(2)}
                      </span>
                      {' '}(R$ {downtime.costPerHour}/h)
                    </p>
                  )}
                </div>
              )}

              {/* Trend indicators */}
              {trends && trends.sensors.length > 0 && (
                <div className="bg-gray-800 rounded-xl px-4 py-3">
                  <p className="text-xs text-gray-500 uppercase tracking-wider mb-3">Tendência dos Sensores</p>
                  <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
                    {trends.sensors.map(s => (
                      <div key={s.sensor} className="flex items-center gap-2 bg-gray-700/50 rounded-lg px-3 py-2">
                        <span className={`text-lg font-bold ${TREND_COLOR[s.direction]}`}>
                          {TREND_ICON[s.direction]}
                        </span>
                        <div>
                          <p className="text-xs text-gray-400 capitalize">{s.sensor}</p>
                          <p className={`text-xs font-semibold ${TREND_COLOR[s.direction]}`}>{s.direction}</p>
                        </div>
                        <span className="ml-auto text-xs font-mono text-gray-500">
                          {s.delta > 0 ? '+' : ''}{s.delta.toFixed(2)}
                        </span>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Trend chart */}
              <div>
                <h3 className="text-sm font-semibold text-gray-300 mb-3">Histórico de Sensores (últimas leituras)</h3>
                {loadingChart ? (
                  <div className="h-48 flex items-center justify-center text-gray-600 text-sm">
                    Carregando dados históricos…
                  </div>
                ) : chartData.length > 1 ? (
                  <div className="bg-gray-800 rounded-xl p-3">
                    <ResponsiveContainer width="100%" height={220}>
                      <LineChart data={chartData} margin={{ top: 5, right: 10, left: -10, bottom: 5 }}>
                        <CartesianGrid stroke="#374151" strokeDasharray="3 3" />
                        <XAxis dataKey="time" tick={{ fill: '#6b7280', fontSize: 10 }} interval="preserveStartEnd" />
                        <YAxis tick={{ fill: '#6b7280', fontSize: 10 }} />
                        <Tooltip
                          contentStyle={{ backgroundColor: '#1f2937', border: '1px solid #374151', borderRadius: '8px' }}
                          labelStyle={{ color: '#d1d5db' }}
                        />
                        <Legend wrapperStyle={{ fontSize: '12px', color: '#9ca3af' }} />
                        <ReferenceLine y={12} stroke="#ef4444" strokeDasharray="4 4" label={{ value: 'Vib Crit', fill: '#ef4444', fontSize: 10 }} />
                        <ReferenceLine y={60} stroke="#f97316" strokeDasharray="4 4" label={{ value: 'Temp Crit', fill: '#f97316', fontSize: 10 }} />
                        <Line type="monotone" dataKey="Vibração"    stroke="#60a5fa" strokeWidth={2} dot={false} />
                        <Line type="monotone" dataKey="Temperatura" stroke="#f59e0b" strokeWidth={2} dot={false} />
                        <Line type="monotone" dataKey="Pressão"     stroke="#34d399" strokeWidth={2} dot={false} />
                      </LineChart>
                    </ResponsiveContainer>
                  </div>
                ) : (
                  <div className="h-24 flex items-center justify-center text-gray-600 text-sm bg-gray-800 rounded-xl">
                    Poucas leituras para gerar gráfico.
                  </div>
                )}
              </div>

              {/* Stats table + recommendation */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {stats && (
                  <div>
                    <h3 className="text-sm font-semibold text-gray-300 mb-3">Estatísticas (últimas 100 leituras)</h3>
                    <div className="bg-gray-800 rounded-xl overflow-hidden">
                      <table className="w-full text-xs">
                        <thead>
                          <tr className="border-b border-gray-700">
                            <th className="text-left px-3 py-2 text-gray-500">Sensor</th>
                            <th className="px-2 py-2 text-gray-500">Mín</th>
                            <th className="px-2 py-2 text-gray-500">Méd</th>
                            <th className="px-2 py-2 text-gray-500">Máx</th>
                            <th className="px-2 py-2 text-gray-500">Atual</th>
                          </tr>
                        </thead>
                        <tbody>
                          {[
                            { label: 'Vibração',    s: stats.vibration,   unit: 'mm/s', warn: 10, crit: 12 },
                            { label: 'Temperatura', s: stats.temperature, unit: '°C',   warn: 55, crit: 60 },
                            { label: 'Pressão',     s: stats.pressure,    unit: 'bar',  warn: 5.0, crit: 5.5 },
                            { label: 'Umidade',     s: stats.humidity,    unit: '%',    warn: 70, crit: 80 },
                            { label: 'Corrente',    s: stats.current,     unit: 'A',    warn: 18, crit: 21 },
                            { label: 'Potência',    s: stats.power,       unit: 'kW',   warn: 99, crit: 999 },
                          ].map(row => {
                            const valColor = row.s.last > row.crit ? 'text-red-400' : row.s.last > row.warn ? 'text-yellow-400' : 'text-gray-300';
                            return (
                              <tr key={row.label} className="border-b border-gray-700/50 hover:bg-gray-700/30">
                                <td className="px-3 py-1.5 text-gray-400">{row.label}</td>
                                <td className="px-2 py-1.5 text-center text-gray-500">{row.s.min.toFixed(1)}</td>
                                <td className="px-2 py-1.5 text-center text-gray-400">{row.s.avg.toFixed(1)}</td>
                                <td className="px-2 py-1.5 text-center text-gray-400">{row.s.max.toFixed(1)}</td>
                                <td className={`px-2 py-1.5 text-center font-semibold ${valColor}`}>
                                  {row.s.last.toFixed(1)} <span className="text-gray-600">{row.unit}</span>
                                </td>
                              </tr>
                            );
                          })}
                        </tbody>
                      </table>
                    </div>
                  </div>
                )}

                <div>
                  <h3 className="text-sm font-semibold text-gray-300 mb-3">Recomendação de Manutenção</h3>
                  <div className="bg-gray-800 rounded-xl p-4 space-y-3">
                    <div className={`text-sm font-bold ${maintenance.color}`}>{maintenance.urgency}</div>
                    <p className="text-gray-300 text-sm">{maintenance.action}</p>
                    <div className="grid grid-cols-2 gap-2 text-xs">
                      <div className="bg-gray-700 rounded-lg p-2">
                        <p className="text-gray-500">Prazo</p>
                        <p className="text-white font-medium">{maintenance.timeframe}</p>
                      </div>
                      <div className="bg-gray-700 rounded-lg p-2">
                        <p className="text-gray-500">Custo Estimado</p>
                        <p className="text-white font-medium">{maintenance.cost}</p>
                      </div>
                    </div>
                    {oee !== null && (
                      <div className="pt-2 border-t border-gray-700">
                        <p className="text-xs text-gray-500 mb-2">OEE Breakdown</p>
                        <OeeBar
                          availability={state === 'critical' ? 60 : state === 'degrading' ? 82 : 97}
                          performance={Math.max(0, Math.min(100, (1 - (parsedData?.vibration ?? 0) / 20) * 100))}
                          quality={Math.max(0, Math.min(100, (1 - (parsedData?.temperature ?? 0) / 80) * 100))}
                        />
                      </div>
                    )}
                  </div>
                </div>
              </div>
            </>
          )}

          {tab === 'maintenance' && (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="text-sm font-semibold text-gray-300">Histórico de Intervenções</h3>
                <button
                  onClick={() => setShowForm(f => !f)}
                  className="text-xs px-3 py-1.5 rounded-lg bg-blue-700 hover:bg-blue-600 text-white transition"
                >
                  {showForm ? 'Cancelar' : '+ Registrar Manutenção'}
                </button>
              </div>

              {showForm && (
                <div className="bg-gray-800 rounded-xl p-4 space-y-3">
                  <p className="text-sm font-semibold text-gray-300">Nova Intervenção — {machineName}</p>
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="text-xs text-gray-500 block mb-1">Tipo</label>
                      <select
                        value={formType}
                        onChange={e => setFormType(e.target.value)}
                        className="w-full bg-gray-700 text-gray-200 text-sm rounded-lg px-3 py-2 border border-gray-600 focus:outline-none"
                      >
                        <option value="corrective">Corretiva</option>
                        <option value="preventive">Preventiva</option>
                      </select>
                    </div>
                    <div>
                      <label className="text-xs text-gray-500 block mb-1">Técnico</label>
                      <input
                        value={formTech}
                        onChange={e => setFormTech(e.target.value)}
                        placeholder="Nome do técnico"
                        className="w-full bg-gray-700 text-gray-200 text-sm rounded-lg px-3 py-2 border border-gray-600 focus:outline-none"
                      />
                    </div>
                  </div>
                  <div>
                    <label className="text-xs text-gray-500 block mb-1">Observações</label>
                    <textarea
                      value={formNotes}
                      onChange={e => setFormNotes(e.target.value)}
                      rows={2}
                      placeholder="Descreva o que foi feito ou o problema encontrado..."
                      className="w-full bg-gray-700 text-gray-200 text-sm rounded-lg px-3 py-2 border border-gray-600 focus:outline-none resize-none"
                    />
                  </div>
                  <button
                    onClick={handleRegister}
                    disabled={submitting}
                    className="w-full py-2 rounded-lg bg-blue-700 hover:bg-blue-600 text-white text-sm font-medium transition disabled:opacity-50"
                  >
                    {submitting ? 'Registrando...' : 'Confirmar Registro'}
                  </button>
                </div>
              )}

              {loadingEvents ? (
                <div className="text-center text-gray-600 py-8 text-sm">Carregando histórico...</div>
              ) : events.length === 0 ? (
                <div className="text-center text-gray-600 py-8 text-sm">
                  Nenhuma intervenção registrada para esta máquina.
                </div>
              ) : (
                <div className="space-y-2">
                  {events.map(ev => (
                    <div
                      key={ev.id}
                      className={`bg-gray-800 rounded-xl px-4 py-3 border-l-4 ${
                        ev.isOpen ? 'border-yellow-500' : 'border-green-600'
                      }`}
                    >
                      <div className="flex items-start justify-between gap-2">
                        <div className="space-y-0.5">
                          <div className="flex items-center gap-2">
                            <span className={`text-xs font-bold px-2 py-0.5 rounded-full ${
                              ev.type === 'corrective'
                                ? 'bg-red-900/50 text-red-300'
                                : 'bg-blue-900/50 text-blue-300'
                            }`}>
                              {ev.type === 'corrective' ? 'Corretiva' : 'Preventiva'}
                            </span>
                            <span className={`text-xs font-semibold ${ev.isOpen ? 'text-yellow-400' : 'text-green-400'}`}>
                              {ev.isOpen ? 'Em aberto' : 'Concluída'}
                            </span>
                            {ev.durationMinutes !== null && (
                              <span className="text-xs text-gray-500">{ev.durationMinutes}min</span>
                            )}
                          </div>
                          <p className="text-xs text-gray-400">
                            Início: {new Date(ev.startedAt).toLocaleString('pt-BR')}
                            {ev.resolvedAt && ` · Fim: ${new Date(ev.resolvedAt).toLocaleString('pt-BR')}`}
                          </p>
                          {ev.technicianName && (
                            <p className="text-xs text-gray-500">Técnico: {ev.technicianName}</p>
                          )}
                          {ev.notes && <p className="text-xs text-gray-400 mt-1">{ev.notes}</p>}
                        </div>
                        {ev.isOpen && (
                          <button
                            onClick={() => handleResolve(ev.id)}
                            className="text-xs px-2 py-1 rounded bg-green-800 hover:bg-green-700 text-green-200 transition flex-shrink-0"
                          >
                            Concluir
                          </button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

// ── helpers ──────────────────────────────────────────────────────────────────

function calcRisk(d: ParsedSensorData | null): number {
  if (!d) return 0;
  return Math.round(
    (Math.min(d.vibration / 15 * 100, 100) +
     Math.min(d.temperature / 70 * 100, 100) +
     Math.min(d.pressure / 6 * 100, 100)) / 3
  );
}

function getMaintenanceRecommendation(state: string, risk: number) {
  if (state === 'critical' || risk > 70)
    return { urgency: 'IMEDIATA', action: 'Parar máquina e inspecionar componentes críticos.', timeframe: 'Próximas 2 horas', cost: 'R$ 2.500–R$ 5.000', color: 'text-red-400' };
  if (state === 'degrading' || risk > 45)
    return { urgency: 'PRIORITÁRIA', action: 'Agendar manutenção preventiva com equipe técnica.', timeframe: 'Próximos 7 dias', cost: 'R$ 800–R$ 1.500', color: 'text-yellow-400' };
  return { urgency: 'PROGRAMADA', action: 'Manutenção de rotina conforme plano preventivo.', timeframe: 'Próximos 30 dias', cost: 'R$ 200–R$ 500', color: 'text-green-400' };
}

interface OeeBarProps { availability: number; performance: number; quality: number; }

const OeeBar: React.FC<OeeBarProps> = ({ availability, performance, quality }) => {
  const oee = ((availability * performance * quality) / 10000).toFixed(1);
  const items = [
    { label: 'Disponibilidade', value: availability, color: 'bg-blue-500' },
    { label: 'Performance',     value: performance,  color: 'bg-green-500' },
    { label: 'Qualidade',       value: quality,      color: 'bg-purple-500' },
  ];
  return (
    <div className="space-y-1.5">
      {items.map(i => (
        <div key={i.label} className="flex items-center gap-2">
          <span className="text-gray-500 text-xs w-28">{i.label}</span>
          <div className="flex-1 bg-gray-700 rounded-full h-2">
            <div className={`${i.color} h-2 rounded-full`} style={{ width: `${Math.min(i.value, 100)}%` }} />
          </div>
          <span className="text-xs text-gray-400 w-10 text-right">{i.value.toFixed(0)}%</span>
        </div>
      ))}
      <p className="text-xs text-gray-400 pt-1">OEE calculado: <span className="text-white font-bold">{oee}%</span></p>
    </div>
  );
};

export default MachineDetailsModal;
