import React, { useState, useEffect } from 'react';
import MachineCard from './components/MachineCard';

interface SensorData {
  id: number;
  topic: string;
  message: string;
  receivedAt: string;
}

const App: React.FC = () => {
  const [data, setData] = useState<Record<string, SensorData[]>>({});
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchData = async () => {
    try {
      const machines = ['M1', 'M2', 'M3', 'M4', 'M5', 'M6', 'M7', 'M8', 'M9'];
      const newData: Record<string, SensorData[]> = {};

      for (const machine of machines) {
        try {
          const response = await fetch(`http://localhost:5002/api/iot/machine/${machine}`);
          if (response.ok) {
            const machineData = await response.json();
            newData[machine] = machineData;
          } else {
            console.warn(`Failed to fetch data for ${machine}: ${response.status}`);
            newData[machine] = [];
          }
        } catch (err) {
          console.error(`Error fetching data for ${machine}:`, err);
          newData[machine] = [];
        }
      }

      setData(newData);
      setError(null);
    } catch (err) {
      console.error('Error fetching data:', err);
      setError('Erro ao carregar dados das máquinas');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
    const interval = setInterval(fetchData, 5000);
    return () => clearInterval(interval);
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-100">
        <div className="text-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-500 mx-auto"></div>
          <p className="mt-4 text-lg text-gray-600">Carregando dados das máquinas...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-100">
        <div className="text-center">
          <div className="text-red-500 text-xl mb-4">⚠️</div>
          <p className="text-lg text-red-600">{error}</p>
          <button
            onClick={fetchData}
            className="mt-4 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
          >
            Tentar Novamente
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 p-4">
      <div className="max-w-7xl mx-auto">
        <h1 className="text-3xl font-bold text-gray-800 mb-6 text-center">
          Dashboard IoT - Manutenção Preditiva
        </h1>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {Object.entries(data).map(([machineId, machineData]) => (
            <MachineCard
              key={machineId}
              machineId={machineId}
              data={machineData}
            />
          ))}
        </div>

        {Object.keys(data).length === 0 && (
          <div className="text-center py-12">
            <p className="text-lg text-gray-600">
              Nenhum dado disponível. Verifique se o simulador e a API estão rodando.
            </p>
          </div>
        )}
      </div>
    </div>
  );
};

export default App;
