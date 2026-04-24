using FluentAssertions;
using Moq;
using IoTDataApi.Application.Services;
using IoTDataApi.Domain.Entities;
using IoTDataApi.Domain.Interfaces;

namespace IoTDataApi.Tests.Services;

public class IoTDataServiceTests
{
    private readonly Mock<IIoTDataRepository> _repoMock = new();
    private readonly IoTDataService           _service;

    public IoTDataServiceTests()
    {
        _service = new IoTDataService(_repoMock.Object);
    }

    // ── OEE ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMachineStats_NormalState_OeeAbove80()
    {
        _repoMock.Setup(r => r.GetByMachineIdAsync("M1"))
            .ReturnsAsync([MakeRecord("M1", state: "normal", vibration: 5.0, temperature: 40.0)]);

        var stats = await _service.GetMachineStatsAsync("M1");

        stats.Should().NotBeNull();
        stats!.Oee.Should().BeGreaterThan(80.0);
        stats.CurrentState.Should().Be("normal");
    }

    [Fact]
    public async Task GetMachineStats_CriticalState_OeeBelowNormal()
    {
        var normalRecord   = MakeRecord("M1", state: "normal",   vibration: 5.0,  temperature: 40.0);
        var criticalRecord = MakeRecord("M1", state: "critical", vibration: 13.0, temperature: 62.0);

        _repoMock.Setup(r => r.GetByMachineIdAsync("M1"))
            .ReturnsAsync([criticalRecord]);
        var criticalStats = await _service.GetMachineStatsAsync("M1");

        _repoMock.Setup(r => r.GetByMachineIdAsync("M1"))
            .ReturnsAsync([normalRecord]);
        var normalStats = await _service.GetMachineStatsAsync("M1");

        criticalStats!.Oee.Should().BeLessThan(normalStats!.Oee);
    }

    [Theory]
    [InlineData("normal",   true)]
    [InlineData("degrading", true)]
    [InlineData("critical", true)]
    public async Task GetMachineStats_AllStates_ReturnsNonNullOee(string state, bool expectResult)
    {
        _repoMock.Setup(r => r.GetByMachineIdAsync("M1"))
            .ReturnsAsync([MakeRecord("M1", state: state, vibration: 5.0, temperature: 40.0)]);

        var stats = await _service.GetMachineStatsAsync("M1");
        (stats is not null).Should().Be(expectResult);
    }

    // ── Risk Score ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMachineStats_LowSensorValues_LowRiskScore()
    {
        _repoMock.Setup(r => r.GetByMachineIdAsync("M1"))
            .ReturnsAsync([MakeRecord("M1", vibration: 2.0, temperature: 30.0, pressure: 3.0)]);

        var stats = await _service.GetMachineStatsAsync("M1");
        stats!.RiskScore.Should().BeLessThan(40.0);
    }

    [Fact]
    public async Task GetMachineStats_HighSensorValues_HighRiskScore()
    {
        _repoMock.Setup(r => r.GetByMachineIdAsync("M1"))
            .ReturnsAsync([MakeRecord("M1", vibration: 14.0, temperature: 68.0, pressure: 5.8)]);

        var stats = await _service.GetMachineStatsAsync("M1");
        stats!.RiskScore.Should().BeGreaterThan(80.0);
    }

    // ── Alertas ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetActiveAlerts_VibrationAboveCritical_ReturnsCriticoAlert()
    {
        _repoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync([MakeRecord("M1", vibration: 13.5, temperature: 40.0, pressure: 4.0)]);

        var alerts = (await _service.GetActiveAlertsAsync()).ToList();

        alerts.Should().Contain(a => a.Sensor == "vibration" && a.Severity == "CRÍTICO");
    }

    [Fact]
    public async Task GetActiveAlerts_VibrationBetweenWarnAndCritical_ReturnsAlertaAlert()
    {
        // vibração entre 10 (warn) e 12 (crit)
        _repoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync([MakeRecord("M1", vibration: 11.0, temperature: 40.0, pressure: 4.0)]);

        var alerts = (await _service.GetActiveAlertsAsync()).ToList();

        alerts.Should().Contain(a => a.Sensor == "vibration" && a.Severity == "ALERTA");
        alerts.Should().NotContain(a => a.Sensor == "vibration" && a.Severity == "CRÍTICO");
    }

    [Fact]
    public async Task GetActiveAlerts_AllSensorsNormal_ReturnsNoAlerts()
    {
        _repoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync([MakeRecord("M1", vibration: 5.0, temperature: 40.0, pressure: 4.0,
                                           humidity: 50.0, current: 10.0)]);

        var alerts = await _service.GetActiveAlertsAsync();
        alerts.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveAlerts_MultipleThresholdsBroken_ReturnsOneAlertPerSensor()
    {
        _repoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync([MakeRecord("M1", vibration: 13.0, temperature: 62.0, pressure: 5.8,
                                           humidity: 85.0, current: 22.0)]);

        var alerts = (await _service.GetActiveAlertsAsync()).ToList();

        alerts.Should().OnlyContain(a => a.Severity == "CRÍTICO");
        alerts.Select(a => a.Sensor).Should().OnlyHaveUniqueItems();
    }

    // ── Stats sem dados ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetMachineStats_NoRecords_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByMachineIdAsync("M99"))
            .ReturnsAsync([]);

        var stats = await _service.GetMachineStatsAsync("M99");
        stats.Should().BeNull();
    }

    // ── AggregateStats: min/max/avg/last ─────────────────────────────────────

    [Fact]
    public async Task GetMachineStats_MultipleReadings_AggregatesCorrectly()
    {
        // 3 registros com vibração 2, 6, 4 (mais recente primeiro)
        var records = new[]
        {
            MakeRecord("M1", vibration: 2.0, temperature: 35.0),
            MakeRecord("M1", vibration: 6.0, temperature: 35.0),
            MakeRecord("M1", vibration: 4.0, temperature: 35.0),
        };

        _repoMock.Setup(r => r.GetByMachineIdAsync("M1")).ReturnsAsync(records);

        var stats = await _service.GetMachineStatsAsync("M1");

        stats!.Vibration.Min.Should().BeApproximately(2.0, 0.01);
        stats.Vibration.Max.Should().BeApproximately(6.0, 0.01);
        stats.Vibration.Avg.Should().BeApproximately(4.0, 0.01);
        stats.Vibration.Last.Should().BeApproximately(2.0, 0.01); // primeiro = mais recente
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static IoTData MakeRecord(
        string machineId,
        string state       = "normal",
        double vibration   = 5.0,
        double temperature = 40.0,
        double pressure    = 4.5,
        double humidity    = 40.0,
        double current     = 10.0,
        double voltage     = 220.0,
        double power       = 2.5) => new()
    {
        Id         = Random.Shared.Next(),
        Topic      = $"sensors/{machineId}/data",
        ReceivedAt = DateTime.UtcNow.ToString("o"),
        Message    = $$"""
            {
              "machine_id": "{{machineId}}",
              "machine_name": "Máquina {{machineId}}",
              "area": "Área",
              "state": "{{state}}",
              "vibration": {{vibration}},
              "temperature": {{temperature}},
              "pressure": {{pressure}},
              "humidity": {{humidity}},
              "current": {{current}},
              "voltage": {{voltage}},
              "power": {{power}},
              "timestamp": "2026-04-24T10:00:00Z"
            }
            """,
    };
}
