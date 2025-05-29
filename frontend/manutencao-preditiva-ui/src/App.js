import React, { useEffect, useState } from "react";
import { Bar, Line, Doughnut } from "react-chartjs-2";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  LineElement,
  PointElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
} from "chart.js";

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  LineElement,
  PointElement,
  Title,
  Tooltip,
  Legend,
  ArcElement
);

function App() {
  const [sensorData, setSensorData] = useState([]);
  const [metrics, setMetrics] = useState({
    oee: 0,
    mtt: 0,
    avgCycleTime: 0,
    avgManTime: 0,
    avgMachineTime: 0,
  });
  const [predictions, setPredictions] = useState([]);
  const [loading, setLoading] = useState(true);

  // Simulação de dados quando a API não estiver disponível
  const generateMockData = () => {
    const now = new Date();
    const mockSensorData = Array.from({ length: 20 }, (_, i) => ({
      machineId: `M${(i % 5) + 1}`,
      vibration: Math.random() * 10 + 5,
      temperature: Math.random() * 30 + 40,
      pressure: Math.random() * 5 + 2,
      timestamp: new Date(now.getTime() - (19 - i) * 300000).toISOString(),
    }));

    const mockMetrics = {
      oee: 0.75 + Math.random() * 0.2,
      mtt: Math.random() * 120 + 60,
      avgCycleTime: Math.random() * 20 + 30,
      avgManTime: Math.random() * 10 + 15,
      avgMachineTime: Math.random() * 15 + 20,
    };

    const mockPredictions = Array.from({ length: 10 }, (_, i) => ({
      failureProbability: Math.random() * 0.3,
      timestamp: new Date(now.getTime() - (9 - i) * 600000).toISOString(),
    }));

    setSensorData(mockSensorData);
    setMetrics(mockMetrics);
    setPredictions(mockPredictions);
    setLoading(false);
  };

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch("/api/sensors");
        if (!response.ok) throw new Error("API not available");

        const sensorResponse = await fetch("/api/sensors").then((r) =>
          r.json()
        );
        const metricsResponse = await fetch("/api/metrics").then((r) =>
          r.json()
        );
        const predictResponse = await fetch("/api/predict").then((r) =>
          r.json()
        );

        setSensorData(sensorResponse);
        setMetrics(metricsResponse);
        setPredictions((prev) =>
          [
            ...prev,
            {
              failureProbability: predictResponse.failureProbability,
              timestamp: new Date().toISOString(),
            },
          ].slice(-10)
        );
        setLoading(false);
      } catch (error) {
        console.log("Using mock data for demonstration");
        generateMockData();
      }
    };

    fetchData();
    const interval = setInterval(fetchData, 5000);
    return () => clearInterval(interval);
  }, []);

  // Análises estratégicas
  const getOperationalStatus = () => {
    const avgFailureProb =
      predictions.length > 0
        ? predictions.reduce((sum, p) => sum + p.failureProbability, 0) /
          predictions.length
        : 0;

    if (avgFailureProb > 0.7)
      return { status: "CRÍTICO", color: "#ff3b30", bg: "#ffebea" };
    if (avgFailureProb > 0.4)
      return { status: "ATENÇÃO", color: "#ff9500", bg: "#fff4e6" };
    return { status: "NORMAL", color: "#34c759", bg: "#eafaf1" };
  };

  const getMachineAnalysis = () => {
    const machineGroups = sensorData.reduce((acc, data) => {
      if (!acc[data.machineId]) {
        acc[data.machineId] = { vibration: [], temperature: [], pressure: [] };
      }
      acc[data.machineId].vibration.push(data.vibration);
      acc[data.machineId].temperature.push(data.temperature);
      acc[data.machineId].pressure.push(data.pressure || 0);
      return acc;
    }, {});

    return Object.entries(machineGroups).map(([id, data]) => {
      const avgVib =
        data.vibration.reduce((sum, v) => sum + v, 0) / data.vibration.length;
      const avgTemp =
        data.temperature.reduce((sum, t) => sum + t, 0) /
        data.temperature.length;
      const avgPressure =
        data.pressure.reduce((sum, p) => sum + p, 0) / data.pressure.length;

      let risk = "BAIXO";
      let riskColor = "#34c759";

      if (avgVib > 12 || avgTemp > 65 || avgPressure > 6) {
        risk = "ALTO";
        riskColor = "#ff3b30";
      } else if (avgVib > 8 || avgTemp > 55 || avgPressure > 4) {
        risk = "MÉDIO";
        riskColor = "#ff9500";
      }

      return { id, avgVib, avgTemp, avgPressure, risk, riskColor };
    });
  };

  const getRecommendations = () => {
    const operationalStatus = getOperationalStatus();
    const recommendations = [];

    if (operationalStatus.status === "CRÍTICO") {
      recommendations.push({
        priority: "ALTA",
        action: "Intervenção Imediata",
        description: "Pare a operação e execute manutenção preventiva",
        icon: "🚨",
      });
    }

    if (metrics.oee < 0.7) {
      recommendations.push({
        priority: "MÉDIA",
        action: "Otimizar OEE",
        description: "Analisar causas de baixa eficiência operacional",
        icon: "📊",
      });
    }

    if (metrics.mtt > 120) {
      recommendations.push({
        priority: "MÉDIA",
        action: "Reduzir Tempo de Reparo",
        description: "Revisar processos de manutenção",
        icon: "⏱️",
      });
    }

    return recommendations;
  };

  if (loading) {
    return (
      <div
        style={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          height: "100vh",
          fontSize: "18px",
        }}
      >
        Carregando dados...
      </div>
    );
  }

  const operationalStatus = getOperationalStatus();
  const machineAnalysis = getMachineAnalysis();
  const recommendations = getRecommendations();

  // Dados dos gráficos
  const oeeStatusData = {
    labels: ["Disponibilidade", "Performance", "Qualidade"],
    datasets: [
      {
        data: [metrics.oee * 100, 85, 92],
        backgroundColor: ["#34c759", "#007aff", "#ff9500"],
        borderWidth: 0,
      },
    ],
  };

  const riskDistributionData = {
    labels: ["Baixo Risco", "Médio Risco", "Alto Risco"],
    datasets: [
      {
        data: [
          machineAnalysis.filter((m) => m.risk === "BAIXO").length,
          machineAnalysis.filter((m) => m.risk === "MÉDIO").length,
          machineAnalysis.filter((m) => m.risk === "ALTO").length,
        ],
        backgroundColor: ["#34c759", "#ff9500", "#ff3b30"],
        borderWidth: 0,
      },
    ],
  };

  const trendData = {
    labels: predictions
      .slice(-8)
      .map((p) =>
        new Date(p.timestamp).toLocaleTimeString("pt-BR", {
          hour: "2-digit",
          minute: "2-digit",
        })
      ),
    datasets: [
      {
        label: "Risco de Falha (%)",
        data: predictions.slice(-8).map((p) => p.failureProbability * 100),
        borderColor: "#ff9500",
        backgroundColor: "rgba(255, 149, 0, 0.1)",
        tension: 0.4,
        fill: true,
      },
    ],
  };

  const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: "bottom",
        labels: { font: { size: 12 } },
      },
    },
  };

  return (
    <div
      style={{
        padding: "24px",
        backgroundColor: "#f8f9fa",
        minHeight: "100vh",
        fontFamily: "-apple-system, BlinkMacSystemFont, sans-serif",
      }}
    >
      {/* Header */}
      <div style={{ textAlign: "center", marginBottom: "32px" }}>
        <h1
          style={{
            fontSize: "2.5rem",
            fontWeight: "bold",
            color: "#1a1a1a",
            margin: "0 0 8px 0",
          }}
        >
          Centro de Controle Operacional
        </h1>
        <p style={{ fontSize: "1.1rem", color: "#666", margin: 0 }}>
          Manutenção Preditiva • Indústria 4.0
        </p>
      </div>

      {/* Status Geral */}
      <div
        style={{
          backgroundColor: operationalStatus.bg,
          border: `2px solid ${operationalStatus.color}`,
          borderRadius: "12px",
          padding: "20px",
          marginBottom: "32px",
          textAlign: "center",
        }}
      >
        <h2
          style={{
            color: operationalStatus.color,
            fontSize: "1.5rem",
            margin: "0 0 8px 0",
            fontWeight: "bold",
          }}
        >
          STATUS OPERACIONAL: {operationalStatus.status}
        </h2>
        <p style={{ fontSize: "1rem", color: "#333", margin: 0 }}>
          {operationalStatus.status === "NORMAL" &&
            "Todas as operações funcionando dentro dos parâmetros normais"}
          {operationalStatus.status === "ATENÇÃO" &&
            "Monitoramento intensivo necessário"}
          {operationalStatus.status === "CRÍTICO" &&
            "Intervenção imediata necessária"}
        </p>
      </div>

      {/* KPIs Principais */}
      <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(auto-fit, minmax(250px, 1fr))",
          gap: "20px",
          marginBottom: "32px",
        }}
      >
        <div
          style={{
            backgroundColor: "white",
            borderRadius: "12px",
            padding: "24px",
            boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
          }}
        >
          <h3
            style={{
              color: "#666",
              fontSize: "0.9rem",
              margin: "0 0 8px 0",
              textTransform: "uppercase",
              letterSpacing: "0.5px",
            }}
          >
            OEE
          </h3>
          <div
            style={{
              fontSize: "2.5rem",
              fontWeight: "bold",
              color:
                metrics.oee > 0.8
                  ? "#34c759"
                  : metrics.oee > 0.6
                  ? "#ff9500"
                  : "#ff3b30",
            }}
          >
            {(metrics.oee * 100).toFixed(1)}%
          </div>
          <p style={{ color: "#999", fontSize: "0.9rem", margin: "4px 0 0 0" }}>
            Eficiência Global
          </p>
        </div>

        <div
          style={{
            backgroundColor: "white",
            borderRadius: "12px",
            padding: "24px",
            boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
          }}
        >
          <h3
            style={{
              color: "#666",
              fontSize: "0.9rem",
              margin: "0 0 8px 0",
              textTransform: "uppercase",
              letterSpacing: "0.5px",
            }}
          >
            MTT
          </h3>
          <div
            style={{
              fontSize: "2.5rem",
              fontWeight: "bold",
              color:
                metrics.mtt < 90
                  ? "#34c759"
                  : metrics.mtt < 120
                  ? "#ff9500"
                  : "#ff3b30",
            }}
          >
            {metrics.mtt.toFixed(0)}s
          </div>
          <p style={{ color: "#999", fontSize: "0.9rem", margin: "4px 0 0 0" }}>
            Tempo Médio de Reparo
          </p>
        </div>

        <div
          style={{
            backgroundColor: "white",
            borderRadius: "12px",
            padding: "24px",
            boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
          }}
        >
          <h3
            style={{
              color: "#666",
              fontSize: "0.9rem",
              margin: "0 0 8px 0",
              textTransform: "uppercase",
              letterSpacing: "0.5px",
            }}
          >
            MÁQUINAS ATIVAS
          </h3>
          <div
            style={{ fontSize: "2.5rem", fontWeight: "bold", color: "#007aff" }}
          >
            {machineAnalysis.length}
          </div>
          <p style={{ color: "#999", fontSize: "0.9rem", margin: "4px 0 0 0" }}>
            Em Operação
          </p>
        </div>

        <div
          style={{
            backgroundColor: "white",
            borderRadius: "12px",
            padding: "24px",
            boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
          }}
        >
          <h3
            style={{
              color: "#666",
              fontSize: "0.9rem",
              margin: "0 0 8px 0",
              textTransform: "uppercase",
              letterSpacing: "0.5px",
            }}
          >
            RISCO ATUAL
          </h3>
          <div
            style={{
              fontSize: "2.5rem",
              fontWeight: "bold",
              color: operationalStatus.color,
            }}
          >
            {(predictions.length > 0
              ? predictions[predictions.length - 1].failureProbability * 100
              : 0
            ).toFixed(0)}
            %
          </div>
          <p style={{ color: "#999", fontSize: "0.9rem", margin: "4px 0 0 0" }}>
            Probabilidade de Falha
          </p>
        </div>
      </div>

      {/* Gráficos Analíticos */}
      <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(auto-fit, minmax(400px, 1fr))",
          gap: "24px",
          marginBottom: "32px",
        }}
      >
        <div
          style={{
            backgroundColor: "white",
            borderRadius: "12px",
            padding: "24px",
            boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
          }}
        >
          <h3 style={{ marginBottom: "20px", color: "#333" }}>
            Componentes do OEE
          </h3>
          <div style={{ height: "250px" }}>
            <Doughnut data={oeeStatusData} options={chartOptions} />
          </div>
        </div>

        <div
          style={{
            backgroundColor: "white",
            borderRadius: "12px",
            padding: "24px",
            boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
          }}
        >
          <h3 style={{ marginBottom: "20px", color: "#333" }}>
            Distribuição de Risco
          </h3>
          <div style={{ height: "250px" }}>
            <Doughnut data={riskDistributionData} options={chartOptions} />
          </div>
        </div>

        <div
          style={{
            backgroundColor: "white",
            borderRadius: "12px",
            padding: "24px",
            boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
            gridColumn: "span 2",
          }}
        >
          <h3 style={{ marginBottom: "20px", color: "#333" }}>
            Tendência de Risco
          </h3>
          <div style={{ height: "250px" }}>
            <Line
              data={trendData}
              options={{
                ...chartOptions,
                plugins: {
                  ...chartOptions.plugins,
                  legend: { display: false },
                },
              }}
            />
          </div>
        </div>
      </div>

      {/* Análise por Máquina */}
      <div
        style={{
          backgroundColor: "white",
          borderRadius: "12px",
          padding: "24px",
          boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
          marginBottom: "32px",
        }}
      >
        <h3 style={{ marginBottom: "20px", color: "#333" }}>
          Status das Máquinas
        </h3>
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fit, minmax(300px, 1fr))",
            gap: "16px",
          }}
        >
          {machineAnalysis.map((machine) => (
            <div
              key={machine.id}
              style={{
                border: "1px solid #e0e0e0",
                borderRadius: "8px",
                padding: "16px",
                backgroundColor:
                  machine.risk === "ALTO"
                    ? "#fff5f5"
                    : machine.risk === "MÉDIO"
                    ? "#fffbf0"
                    : "#f0fff4",
              }}
            >
              <div
                style={{
                  display: "flex",
                  justifyContent: "space-between",
                  alignItems: "center",
                  marginBottom: "12px",
                }}
              >
                <h4 style={{ margin: 0, color: "#333" }}>
                  Máquina {machine.id}
                </h4>
                <span
                  style={{
                    backgroundColor: machine.riskColor,
                    color: "white",
                    padding: "4px 8px",
                    borderRadius: "4px",
                    fontSize: "0.8rem",
                    fontWeight: "bold",
                  }}
                >
                  {machine.risk}
                </span>
              </div>
              <div
                style={{
                  display: "grid",
                  gridTemplateColumns: "repeat(3, 1fr)",
                  gap: "8px",
                  fontSize: "0.9rem",
                }}
              >
                <div>
                  <div style={{ color: "#666" }}>Vibração</div>
                  <div style={{ fontWeight: "bold" }}>
                    {machine.avgVib.toFixed(1)}
                  </div>
                </div>
                <div>
                  <div style={{ color: "#666" }}>Temp.</div>
                  <div style={{ fontWeight: "bold" }}>
                    {machine.avgTemp.toFixed(1)}°C
                  </div>
                </div>
                <div>
                  <div style={{ color: "#666" }}>Pressão</div>
                  <div style={{ fontWeight: "bold" }}>
                    {machine.avgPressure.toFixed(1)} bar
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Recomendações */}
      {recommendations.length > 0 && (
        <div
          style={{
            backgroundColor: "white",
            borderRadius: "12px",
            padding: "24px",
            boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
          }}
        >
          <h3 style={{ marginBottom: "20px", color: "#333" }}>
            Recomendações Estratégicas
          </h3>
          <div style={{ display: "grid", gap: "12px" }}>
            {recommendations.map((rec, index) => (
              <div
                key={index}
                style={{
                  display: "flex",
                  alignItems: "center",
                  padding: "16px",
                  backgroundColor:
                    rec.priority === "ALTA" ? "#fff5f5" : "#f8f9ff",
                  borderRadius: "8px",
                  border: `1px solid ${
                    rec.priority === "ALTA" ? "#ffcccb" : "#e0e7ff"
                  }`,
                }}
              >
                <div style={{ fontSize: "1.5rem", marginRight: "16px" }}>
                  {rec.icon}
                </div>
                <div style={{ flex: 1 }}>
                  <div
                    style={{
                      fontWeight: "bold",
                      color: "#333",
                      marginBottom: "4px",
                    }}
                  >
                    {rec.action}
                  </div>
                  <div style={{ color: "#666", fontSize: "0.9rem" }}>
                    {rec.description}
                  </div>
                </div>
                <div
                  style={{
                    backgroundColor:
                      rec.priority === "ALTA" ? "#ff3b30" : "#ff9500",
                    color: "white",
                    padding: "4px 8px",
                    borderRadius: "4px",
                    fontSize: "0.8rem",
                    fontWeight: "bold",
                  }}
                >
                  {rec.priority}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

export default App;
