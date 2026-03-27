import React, { useState } from 'react';
import { Alert } from '../App';

interface AlertPanelProps {
  alerts: Alert[];
}

const SENSOR_LABELS: Record<string, string> = {
  vibration:   'Vibração',
  temperature: 'Temperatura',
  pressure:    'Pressão',
  humidity:    'Umidade',
  current:     'Corrente',
};

const AlertPanel: React.FC<AlertPanelProps> = ({ alerts }) => {
  const [collapsed, setCollapsed] = useState(false);

  const critical = alerts.filter(a => a.severity === 'CRÍTICO');
  const warning  = alerts.filter(a => a.severity === 'ALERTA');

  return (
    <div className="bg-gray-900 border border-gray-700 rounded-2xl overflow-hidden">
      <button
        onClick={() => setCollapsed(c => !c)}
        className="w-full flex items-center justify-between px-5 py-3 hover:bg-gray-800 transition"
      >
        <div className="flex items-center gap-3">
          <span className="text-sm font-semibold text-gray-200">🔔 Painel de Alertas</span>
          {critical.length > 0 && (
            <span className="px-2 py-0.5 rounded-full bg-red-700 text-white text-xs font-bold">
              {critical.length} crítico{critical.length > 1 ? 's' : ''}
            </span>
          )}
          {warning.length > 0 && (
            <span className="px-2 py-0.5 rounded-full bg-yellow-600 text-white text-xs font-bold">
              {warning.length} alerta{warning.length > 1 ? 's' : ''}
            </span>
          )}
        </div>
        <span className="text-gray-500 text-sm">{collapsed ? '▼' : '▲'}</span>
      </button>

      {!collapsed && (
        <div className="divide-y divide-gray-800 max-h-64 overflow-y-auto">
          {alerts.length === 0 ? (
            <p className="px-5 py-3 text-sm text-gray-500">Nenhum alerta ativo.</p>
          ) : (
            alerts.map((alert, idx) => {
              const isCrit = alert.severity === 'CRÍTICO';
              return (
                <div
                  key={idx}
                  className={`flex items-start gap-3 px-5 py-3 ${isCrit ? 'bg-red-950/30' : 'bg-yellow-950/20'}`}
                >
                  <span className={`mt-0.5 text-xs font-bold shrink-0 px-2 py-0.5 rounded-full border ${isCrit ? 'bg-red-900/60 text-red-300 border-red-700' : 'bg-yellow-900/60 text-yellow-300 border-yellow-700'}`}>
                    {alert.severity}
                  </span>
                  <div className="min-w-0">
                    <p className="text-sm text-gray-200 font-medium">
                      {alert.machineName} ({alert.machineId})
                      <span className="text-gray-500 font-normal"> — {alert.area}</span>
                    </p>
                    <p className="text-xs text-gray-400 mt-0.5">
                      {SENSOR_LABELS[alert.sensor] ?? alert.sensor}: <span className="text-white font-mono">{alert.value.toFixed(2)}</span>
                      {' '}<span className="text-gray-600">(limite: {alert.threshold})</span>
                    </p>
                  </div>
                </div>
              );
            })
          )}
        </div>
      )}
    </div>
  );
};

export default AlertPanel;
