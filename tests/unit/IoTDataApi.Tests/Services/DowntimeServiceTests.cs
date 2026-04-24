using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using IoTDataApi.Application.Services;
using IoTDataApi.Domain.Entities;
using IoTDataApi.Domain.Interfaces;

namespace IoTDataApi.Tests.Services;

public class DowntimeServiceTests
{
    private readonly Mock<IIoTDataRepository> _repoMock = new();
    private readonly IMemoryCache             _cache    = new MemoryCache(new MemoryCacheOptions());
    private readonly DowntimeService          _service;

    public DowntimeServiceTests()
    {
        _service = new DowntimeService(_repoMock.Object, _cache);
    }

    // ── sem dados ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetForMachine_NoReadings_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByMachineIdSinceAsync("M1", It.IsAny<DateTime>()))
            .ReturnsAsync([]);

        var result = await _service.GetForMachineAsync("M1");
        result.Should().BeNull();
    }

    // ── disponibilidade ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetForMachine_AllNormalReadings_Returns100PercentAvailability()
    {
        _repoMock.Setup(r => r.GetByMachineIdSinceAsync("M1", It.IsAny<DateTime>()))
            .ReturnsAsync(MakeRecords("M1", 10, "normal"));

        var result = await _service.GetForMachineAsync("M1");

        result.Should().NotBeNull();
        result!.AvailabilityPct.Should().BeApproximately(100.0, 0.01);
        result.DowntimeMinutes.Should().BeApproximately(0.0, 0.01);
        result.FailureEvents.Should().Be(0);
    }

    [Fact]
    public async Task GetForMachine_AllCriticalReadings_ReturnsZeroAvailability()
    {
        _repoMock.Setup(r => r.GetByMachineIdSinceAsync("M1", It.IsAny<DateTime>()))
            .ReturnsAsync(MakeRecords("M1", 10, "critical"));

        var result = await _service.GetForMachineAsync("M1");

        result!.AvailabilityPct.Should().BeApproximately(0.0, 0.01);
    }

    // ── contagem de eventos de falha ──────────────────────────────────────────

    [Fact]
    public async Task GetForMachine_OneCriticalBlock_CountsOneFailureEvent()
    {
        // newest-first: N N C C N N  →  1 evento (uma transição para crítico)
        var records = new[]
        {
            MakeRecord("M1", "normal"),
            MakeRecord("M1", "normal"),
            MakeRecord("M1", "critical"),
            MakeRecord("M1", "critical"),
            MakeRecord("M1", "normal"),
            MakeRecord("M1", "normal"),
        };

        _repoMock.Setup(r => r.GetByMachineIdSinceAsync("M1", It.IsAny<DateTime>()))
            .ReturnsAsync(records);

        var result = await _service.GetForMachineAsync("M1");

        result!.FailureEvents.Should().Be(1);
    }

    [Fact]
    public async Task GetForMachine_TwoSeparateCriticalBlocks_CountsTwoEvents()
    {
        // newest-first: N C N C N  →  2 eventos
        var records = new[]
        {
            MakeRecord("M1", "normal"),
            MakeRecord("M1", "critical"),
            MakeRecord("M1", "normal"),
            MakeRecord("M1", "critical"),
            MakeRecord("M1", "normal"),
        };

        _repoMock.Setup(r => r.GetByMachineIdSinceAsync("M1", It.IsAny<DateTime>()))
            .ReturnsAsync(records);

        var result = await _service.GetForMachineAsync("M1");

        result!.FailureEvents.Should().Be(2);
    }

    // ── tempo de parada ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetForMachine_FourCriticalReadings_DowntimeMinutesIsCorrect()
    {
        // 4 leituras críticas × 5 s = 20 s = 0.333 min → arredondado 0.3
        var records = new[]
        {
            MakeRecord("M1", "normal"),
            MakeRecord("M1", "critical"),
            MakeRecord("M1", "critical"),
            MakeRecord("M1", "critical"),
            MakeRecord("M1", "critical"),
            MakeRecord("M1", "normal"),
        };

        _repoMock.Setup(r => r.GetByMachineIdSinceAsync("M1", It.IsAny<DateTime>()))
            .ReturnsAsync(records);

        var result = await _service.GetForMachineAsync("M1");

        result!.DowntimeMinutes.Should().BeApproximately(0.3, 0.01);
    }

    // ── custo por máquina ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetForMachine_MachineM2_UsesConfiguredHourlyCost800()
    {
        _repoMock.Setup(r => r.GetByMachineIdSinceAsync("M2", It.IsAny<DateTime>()))
            .ReturnsAsync(MakeRecords("M2", 5, "normal"));

        var result = await _service.GetForMachineAsync("M2");

        result!.CostPerHour.Should().Be(800.0);
    }

    [Fact]
    public async Task GetForMachine_MachineM1_UsesConfiguredHourlyCost500()
    {
        _repoMock.Setup(r => r.GetByMachineIdSinceAsync("M1", It.IsAny<DateTime>()))
            .ReturnsAsync(MakeRecords("M1", 5, "normal"));

        var result = await _service.GetForMachineAsync("M1");

        result!.CostPerHour.Should().Be(500.0);
    }

    // ── MTTR e MTBF ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetForMachine_OneFailureEvent_MttrIsDowntimeOverEvents()
    {
        // 2 leituras críticas × 5 s / 60 = 0.167 min → MTTR = 0.2 (rounded 1 decimal)
        var records = new[]
        {
            MakeRecord("M1", "normal"),
            MakeRecord("M1", "critical"),
            MakeRecord("M1", "critical"),
            MakeRecord("M1", "normal"),
        };

        _repoMock.Setup(r => r.GetByMachineIdSinceAsync("M1", It.IsAny<DateTime>()))
            .ReturnsAsync(records);

        var result = await _service.GetForMachineAsync("M1");

        result!.FailureEvents.Should().Be(1);
        // downtime = 2*5/60 ≈ 0.167 min; MTTR = 0.167/1 ≈ 0.2
        result.MttrMinutes.Should().BeApproximately(0.2, 0.05);
    }

    [Fact]
    public async Task GetForMachine_NoFailureEvents_MttrIsZeroAndMtbfIsPeriodLength()
    {
        _repoMock.Setup(r => r.GetByMachineIdSinceAsync("M1", It.IsAny<DateTime>()))
            .ReturnsAsync(MakeRecords("M1", 12, "normal"));

        var result = await _service.GetForMachineAsync("M1");

        result!.MttrMinutes.Should().Be(0);
        result.MtbfHours.Should().BeGreaterThan(0);
    }

    // ── cache ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetForMachine_SecondCall_HitsCache()
    {
        _repoMock.Setup(r => r.GetByMachineIdSinceAsync("M1", It.IsAny<DateTime>()))
            .ReturnsAsync(MakeRecords("M1", 5, "normal"));

        await _service.GetForMachineAsync("M1");
        await _service.GetForMachineAsync("M1");

        _repoMock.Verify(r => r.GetByMachineIdSinceAsync("M1", It.IsAny<DateTime>()), Times.Once);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static IoTData MakeRecord(string machineId, string state = "normal") => new()
    {
        Id         = Random.Shared.Next(),
        Topic      = $"sensors/{machineId}/data",
        ReceivedAt = DateTime.UtcNow.ToString("o"),
        Message    = $$"""
            {
              "machine_id": "{{machineId}}",
              "machine_name": "Máquina {{machineId}}",
              "area": "Área A",
              "state": "{{state}}",
              "vibration": 5.0,
              "temperature": 40.0,
              "pressure": 4.5,
              "humidity": 50.0,
              "current": 10.0,
              "voltage": 220.0,
              "power": 2.5
            }
            """,
    };

    private static IoTData[] MakeRecords(string machineId, int count, string state) =>
        Enumerable.Range(0, count).Select(_ => MakeRecord(machineId, state)).ToArray();
}
