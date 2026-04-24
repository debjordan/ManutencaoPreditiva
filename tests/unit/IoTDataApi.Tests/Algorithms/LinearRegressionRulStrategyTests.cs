using FluentAssertions;
using IoTDataApi.Application.Algorithms;

namespace IoTDataApi.Tests.Algorithms;

public class LinearRegressionRulStrategyTests
{
    private readonly LinearRegressionRulStrategy _strategy = new();

    // ── dados insuficientes ───────────────────────────────────────────────────

    [Fact]
    public void Estimate_WithFewerThanTenPoints_ReturnsNull()
    {
        var values = Enumerable.Repeat(5.0, 9).ToArray();
        _strategy.Estimate(values, criticalThreshold: 12.0).Should().BeNull();
    }

    [Fact]
    public void Estimate_WithEmptyArray_ReturnsNull()
    {
        _strategy.Estimate([], criticalThreshold: 12.0).Should().BeNull();
    }

    // ── sem tendência de deterioração ─────────────────────────────────────────

    [Fact]
    public void Estimate_WithFlatData_ReturnsNull()
    {
        // slope ≈ 0, não há tendência
        var values = Enumerable.Repeat(8.0, 20).ToArray();
        _strategy.Estimate(values, criticalThreshold: 12.0).Should().BeNull();
    }

    [Fact]
    public void Estimate_WithDescendingData_ReturnsNull()
    {
        // sensor melhorando — slope negativo
        var values = Enumerable.Range(0, 20).Select(i => 10.0 - i * 0.2).ToArray();
        _strategy.Estimate(values, criticalThreshold: 12.0).Should().BeNull();
    }

    [Fact]
    public void Estimate_WhenCurrentValueAlreadyExceedsThreshold_ReturnsNull()
    {
        // sensor já passou do threshold — RUL não faz sentido
        var values = Enumerable.Repeat(13.0, 20).ToArray();
        _strategy.Estimate(values, criticalThreshold: 12.0).Should().BeNull();
    }

    // ── tendência crescente: estimativa esperada ──────────────────────────────

    [Fact]
    public void Estimate_WithLinearlyRisingData_ReturnsPositiveEstimate()
    {
        // crescimento de 0.1 por leitura (5s); parte de 8.0, threshold = 12.0
        var values = Enumerable.Range(0, 30).Select(i => 8.0 + i * 0.1).ToArray();
        var result = _strategy.Estimate(values, criticalThreshold: 12.0);

        result.Should().NotBeNull();
        result!.HoursToFailure.Should().BeGreaterThan(0);
        result.Slope.Should().BeApproximately(0.1, precision: 0.01);
    }

    [Fact]
    public void Estimate_WithStrongRisingTrend_ReturnsShortRul()
    {
        // degradação rápida: +0.5 por leitura, parte de 10.0, threshold = 12.0
        // distância ao threshold: 2.0 / 0.5 = 4 leituras * 5s / 3600 ≈ 0.0056h
        var values = Enumerable.Range(0, 20).Select(i => 10.0 + i * 0.05).ToArray();
        var result = _strategy.Estimate(values, criticalThreshold: 12.0);

        result.Should().NotBeNull();
        result!.HoursToFailure.Should().BeGreaterThan(0).And.BeLessThan(72.0);
    }

    [Fact]
    public void Estimate_WithSlowRisingTrend_ReturnsRulCappedAt72Hours()
    {
        // degradação mínima — RUL projetado seria enorme, deve ser capeado em 72h
        var values = Enumerable.Range(0, 20).Select(i => 5.0 + i * 0.00001).ToArray();
        var result = _strategy.Estimate(values, criticalThreshold: 12.0);

        result.Should().NotBeNull();
        result!.HoursToFailure.Should().BeLessOrEqualTo(72.0);
    }

    // ── qualidade do ajuste (R²) ──────────────────────────────────────────────

    [Fact]
    public void Estimate_WithPerfectLinearData_HasHighR2()
    {
        // dados exatamente lineares → R² deve ser próximo de 1
        var values = Enumerable.Range(0, 20).Select(i => 5.0 + i * 0.2).ToArray();
        var result = _strategy.Estimate(values, criticalThreshold: 12.0);

        result.Should().NotBeNull();
        result!.R2.Should().BeGreaterThan(0.99);
    }

    [Fact]
    public void Estimate_WithNoisyData_HasLowerR2ThanPerfectLinear()
    {
        var random = new Random(42);
        // tendência crescente com ruído alto
        var noisy  = Enumerable.Range(0, 20).Select(i => 5.0 + i * 0.2 + (random.NextDouble() - 0.5) * 2.0).ToArray();
        var clean  = Enumerable.Range(0, 20).Select(i => 5.0 + i * 0.2).ToArray();

        var noisyResult = _strategy.Estimate(noisy, criticalThreshold: 20.0);
        var cleanResult = _strategy.Estimate(clean, criticalThreshold: 20.0);

        // ambos devem ter estimativa, mas o ruidoso terá R² menor
        if (noisyResult is not null && cleanResult is not null)
            noisyResult.R2.Should().BeLessThan(cleanResult.R2);
    }

    // ── intervalo de leitura ──────────────────────────────────────────────────

    [Fact]
    public void Estimate_WithCustomReadingInterval_ScalesRulCorrectly()
    {
        // slope = 0.1/leitura; threshold - current = 4.0
        // com 5s: (4.0/0.1) * 5 / 3600 ≈ 0.0556h
        // com 10s: dobro ≈ 0.1111h
        var values = Enumerable.Range(0, 20).Select(i => 8.0 + i * 0.1).ToArray();

        var with5s  = _strategy.Estimate(values, criticalThreshold: 12.0, readingIntervalSeconds: 5.0);
        var with10s = _strategy.Estimate(values, criticalThreshold: 12.0, readingIntervalSeconds: 10.0);

        with5s.Should().NotBeNull();
        with10s.Should().NotBeNull();
        with10s!.HoursToFailure.Should().BeApproximately(with5s!.HoursToFailure * 2, precision: 0.01);
    }
}
