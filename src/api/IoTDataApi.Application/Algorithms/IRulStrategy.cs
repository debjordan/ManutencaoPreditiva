namespace IoTDataApi.Application.Algorithms;

/// <summary>
/// Contrato para estratégias de estimativa de Vida Útil Restante (RUL).
/// Permite trocar o algoritmo (regressão linear, suavização exponencial, ML)
/// sem alterar os serviços que consomem o resultado.
/// </summary>
public interface IRulStrategy
{
    /// <param name="values">Série temporal do sensor, ordenada do mais antigo para o mais recente.</param>
    /// <param name="criticalThreshold">Valor acima do qual a máquina entra em estado crítico.</param>
    /// <param name="readingIntervalSeconds">Intervalo entre leituras em segundos (default: 5s).</param>
    /// <returns>Estimativa de horas até atingir o threshold, ou null se a tendência for estável/descendente.</returns>
    RulEstimate? Estimate(double[] values, double criticalThreshold, double readingIntervalSeconds = 5.0);
}

public record RulEstimate(
    double HoursToFailure,
    double Slope,
    double R2
);
