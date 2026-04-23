namespace IoTDataApi.Application.Algorithms;

/// <summary>
/// Estima RUL via regressão linear simples (OLS) sobre a série temporal.
/// Complexidade: O(n) — adequado para janelas curtas de sensor (≤ 120 leituras).
///
/// Tradeoff: regressão linear assume degradação monotônica e constante.
/// Subestima RUL em degradações aceleradas (falha chega antes do previsto)
/// e superestima em degradações que desaceleram. Para produção com histórico
/// suficiente, substituir por modelo ARIMA ou LSTM via IRulStrategy.
/// </summary>
public sealed class LinearRegressionRulStrategy : IRulStrategy
{
    private const double MaxProjectionHours = 72.0;

    public RulEstimate? Estimate(double[] values, double criticalThreshold, double readingIntervalSeconds = 5.0)
    {
        if (values.Length < 10)
            return null;

        var (slope, r2) = OrdinaryLeastSquares(values);

        if (slope <= 0)
            return null; // tendência estável ou melhorando

        double currentValue = values[^1];
        if (currentValue >= criticalThreshold)
            return null; // já passou do threshold

        double stepsToFailure = (criticalThreshold - currentValue) / slope;
        double hoursToFailure = Math.Min(stepsToFailure * readingIntervalSeconds / 3600.0, MaxProjectionHours);

        if (hoursToFailure <= 0)
            return null;

        return new RulEstimate(
            HoursToFailure: Math.Round(hoursToFailure, 1),
            Slope:          Math.Round(slope, 5),
            R2:             Math.Round(r2, 3)
        );
    }

    /// <summary>
    /// Regressão linear (OLS) sem alocar vetor de índices — O(n), O(1) de memória.
    /// Usa as fórmulas fechadas com somatórios: ∑x = n(n-1)/2, ∑x² = n(n-1)(2n-1)/6.
    /// </summary>
    private static (double slope, double r2) OrdinaryLeastSquares(double[] y)
    {
        int n = y.Length;

        double sumX  = n * (n - 1.0) / 2.0;
        double sumX2 = n * (n - 1.0) * (2.0 * n - 1.0) / 6.0;
        double sumY  = 0.0;
        double sumXY = 0.0;

        for (int i = 0; i < n; i++)
        {
            sumY  += y[i];
            sumXY += i * y[i];
        }

        double denominator = n * sumX2 - sumX * sumX;
        if (Math.Abs(denominator) < 1e-10)
            return (0.0, 0.0);

        double slope     = (n * sumXY - sumX * sumY) / denominator;
        double intercept = (sumY - slope * sumX) / n;

        double yMean = sumY / n;
        double ssTot = 0.0;
        double ssRes = 0.0;

        for (int i = 0; i < n; i++)
        {
            double residual = y[i] - (intercept + slope * i);
            ssTot += (y[i] - yMean) * (y[i] - yMean);
            ssRes += residual * residual;
        }

        double r2 = ssTot < 1e-10 ? 0.0 : 1.0 - ssRes / ssTot;
        return (slope, Math.Max(0.0, r2));
    }
}
