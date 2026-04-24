using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using IoTDataApi.Application.Algorithms;
using IoTDataApi.Application.Services;
using IoTDataApi.Domain.Entities;
using IoTDataApi.Domain.Interfaces;

namespace IoTDataApi.Tests.Services;

public class RulServiceTests
{
    private readonly Mock<IIoTDataRepository> _repoMock     = new();
    private readonly Mock<IRulStrategy>       _strategyMock = new();
    private readonly IMemoryCache             _cache        = new MemoryCache(new MemoryCacheOptions());
    private readonly RulService               _service;

    public RulServiceTests()
    {
        _service = new RulService(_repoMock.Object, _strategyMock.Object, _cache);
    }

    // ── dados insuficientes ───────────────────────────────────────────────────

    [Fact]
    public async Task GetForMachine_FewerThan10ParseableRecords_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByMachineIdAsync("M1", 120))
            .ReturnsAsync(MakeRecords("M1", 9));

        var result = await _service.GetForMachineAsync("M1");

        result.Should().BeNull();
        _strategyMock.Verify(
            s => s.Estimate(It.IsAny<double[]>(), It.IsAny<double>(), It.IsAny<double>()),
            Times.Never);
    }

    // ── sensores estáveis ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetForMachine_AllSensorsStable_ReturnsNaConfidenceAndNullHours()
    {
        _repoMock.Setup(r => r.GetByMachineIdAsync("M1", 120))
            .ReturnsAsync(MakeRecords("M1", 15));

        _strategyMock.Setup(s => s.Estimate(It.IsAny<double[]>(), It.IsAny<double>(), It.IsAny<double>()))
            .Returns((RulEstimate?)null);

        var result = await _service.GetForMachineAsync("M1");

        result.Should().NotBeNull();
        result!.EstimatedHoursToFailure.Should().BeNull();
        result.Confidence.Should().Be("n/a");
        result.LimitingSensor.Should().BeNull();
    }

    // ── sensor limitante ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetForMachine_OnlyVibrationTrending_SetsLimitingSensorToVibration()
    {
        _repoMock.Setup(r => r.GetByMachineIdAsync("M1", 120))
            .ReturnsAsync(MakeRecords("M1", 15));

        // vibration threshold = 12.0 → returns estimate; all others → null (Moq default)
        _strategyMock.Setup(s => s.Estimate(It.IsAny<double[]>(), 12.0, It.IsAny<double>()))
            .Returns(new RulEstimate(10.0, 0.1, 0.85));

        var result = await _service.GetForMachineAsync("M1");

        result!.LimitingSensor.Should().Be("vibration");
        result.EstimatedHoursToFailure.Should().BeApproximately(10.0, 0.01);
    }

    [Fact]
    public async Task GetForMachine_TwoSensorsTrending_PicksSensorWithShorterRul()
    {
        _repoMock.Setup(r => r.GetByMachineIdAsync("M1", 120))
            .ReturnsAsync(MakeRecords("M1", 15));

        // vibration  (12.0) → 5h
        _strategyMock.Setup(s => s.Estimate(It.IsAny<double[]>(), 12.0, It.IsAny<double>()))
            .Returns(new RulEstimate(5.0, 0.1, 0.85));

        // temperature (60.0) → 2h  ← shorter = limitante
        _strategyMock.Setup(s => s.Estimate(It.IsAny<double[]>(), 60.0, It.IsAny<double>()))
            .Returns(new RulEstimate(2.0, 0.5, 0.75));

        var result = await _service.GetForMachineAsync("M1");

        result!.LimitingSensor.Should().Be("temperature");
        result.EstimatedHoursToFailure.Should().BeApproximately(2.0, 0.01);
    }

    // ── classificação de confiança (R²) ───────────────────────────────────────

    [Theory]
    [InlineData(0.85, "alta")]
    [InlineData(0.71, "alta")]
    [InlineData(0.55, "média")]
    [InlineData(0.41, "média")]
    [InlineData(0.30, "baixa")]
    public async Task GetForMachine_VariousR2Values_MapsToCorrectConfidence(double r2, string expected)
    {
        _repoMock.Setup(r => r.GetByMachineIdAsync("M1", 120))
            .ReturnsAsync(MakeRecords("M1", 15));

        _strategyMock.Setup(s => s.Estimate(It.IsAny<double[]>(), It.IsAny<double>(), It.IsAny<double>()))
            .Returns(new RulEstimate(10.0, 0.1, r2));

        var result = await _service.GetForMachineAsync("M1");

        result!.Confidence.Should().Be(expected);
    }

    // ── interpretação textual ─────────────────────────────────────────────────

    [Theory]
    [InlineData(1.5,  "iminente")]
    [InlineData(5.0,  "manutenção preventiva")]
    [InlineData(24.0, "Tendência")]
    public async Task GetForMachine_VariousHours_InterpretationContainsKeyword(double hours, string keyword)
    {
        _repoMock.Setup(r => r.GetByMachineIdAsync("M1", 120))
            .ReturnsAsync(MakeRecords("M1", 15));

        _strategyMock.Setup(s => s.Estimate(It.IsAny<double[]>(), It.IsAny<double>(), It.IsAny<double>()))
            .Returns(new RulEstimate(hours, 0.1, 0.8));

        var result = await _service.GetForMachineAsync("M1");

        result!.Interpretation.Should().Contain(keyword);
    }

    // ── cache ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetForMachine_SecondCall_HitsCache()
    {
        _repoMock.Setup(r => r.GetByMachineIdAsync("M1", 120))
            .ReturnsAsync(MakeRecords("M1", 15));

        _strategyMock.Setup(s => s.Estimate(It.IsAny<double[]>(), It.IsAny<double>(), It.IsAny<double>()))
            .Returns((RulEstimate?)null);

        await _service.GetForMachineAsync("M1");
        await _service.GetForMachineAsync("M1");

        _repoMock.Verify(r => r.GetByMachineIdAsync("M1", 120), Times.Once);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static IEnumerable<IoTData> MakeRecords(string machineId, int count) =>
        Enumerable.Range(0, count).Select(_ => new IoTData
        {
            Id         = Random.Shared.Next(),
            Topic      = $"sensors/{machineId}/data",
            ReceivedAt = DateTime.UtcNow.ToString("o"),
            Message    = $$"""
                {
                  "machine_id": "{{machineId}}",
                  "machine_name": "Máquina {{machineId}}",
                  "area": "Área A",
                  "state": "normal",
                  "vibration": 5.0,
                  "temperature": 40.0,
                  "pressure": 4.5,
                  "humidity": 50.0,
                  "current": 10.0,
                  "voltage": 220.0,
                  "power": 2.5
                }
                """,
        });
}
