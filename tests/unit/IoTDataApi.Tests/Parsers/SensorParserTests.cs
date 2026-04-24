using FluentAssertions;
using IoTDataApi.Application.Parsers;
using IoTDataApi.Domain.Entities;

namespace IoTDataApi.Tests.Parsers;

public class SensorParserTests
{
    // ── JSON válido e completo ────────────────────────────────────────────────

    [Fact]
    public void Parse_WithValidJson_MapsAllFields()
    {
        var record = MakeRecord("""
            {
              "machine_id": "M1",
              "machine_name": "Router CNC",
              "machine_type": "cnc_router",
              "area": "Usinagem",
              "state": "normal",
              "vibration": 6.42,
              "temperature": 48.1,
              "pressure": 4.7,
              "humidity": 38.5,
              "voltage": 221.3,
              "current": 11.2,
              "power": 3.2,
              "timestamp": "2026-04-24T10:00:00Z"
            }
            """);

        var result = SensorParser.Parse(record);

        result.Should().NotBeNull();
        result!.MachineId.Should().Be("M1");
        result.MachineName.Should().Be("Router CNC");
        result.MachineType.Should().Be("cnc_router");
        result.Area.Should().Be("Usinagem");
        result.State.Should().Be("normal");
        result.Vibration.Should().BeApproximately(6.42, 0.001);
        result.Temperature.Should().BeApproximately(48.1, 0.001);
        result.Pressure.Should().BeApproximately(4.7, 0.001);
        result.Humidity.Should().BeApproximately(38.5, 0.001);
        result.Voltage.Should().BeApproximately(221.3, 0.001);
        result.Current.Should().BeApproximately(11.2, 0.001);
        result.Power.Should().BeApproximately(3.2, 0.001);
        result.Timestamp.Should().Be("2026-04-24T10:00:00Z");
        result.ReceivedAt.Should().Be(record.ReceivedAt);
    }

    // ── campos ausentes → defaults ────────────────────────────────────────────

    [Fact]
    public void Parse_WhenMachineNameMissing_FallsBackToMachineId()
    {
        var record = MakeRecord("""{"machine_id":"M2","vibration":5.0,"temperature":40.0,"pressure":4.5}""");
        var result = SensorParser.Parse(record);

        result.Should().NotBeNull();
        result!.MachineName.Should().Be("M2");
    }

    [Fact]
    public void Parse_WhenStateFieldMissing_DefaultsToNormal()
    {
        var record = MakeRecord("""{"machine_id":"M3","vibration":5.0,"temperature":40.0,"pressure":4.0}""");
        var result = SensorParser.Parse(record);

        result!.State.Should().Be("normal");
    }

    [Fact]
    public void Parse_WhenNumericFieldsMissing_DefaultsToZero()
    {
        var record = MakeRecord("""{"machine_id":"M4"}""");
        var result = SensorParser.Parse(record);

        result.Should().NotBeNull();
        result!.Vibration.Should().Be(0.0);
        result.Temperature.Should().Be(0.0);
        result.Pressure.Should().Be(0.0);
    }

    // ── estados da máquina ────────────────────────────────────────────────────

    [Theory]
    [InlineData("normal")]
    [InlineData("degrading")]
    [InlineData("critical")]
    public void Parse_WithKnownStates_PreservesState(string state)
    {
        var record = MakeRecord($$$"""{"machine_id":"M1","state":"{{{state}}}"}""");
        SensorParser.Parse(record)!.State.Should().Be(state);
    }

    // ── JSON inválido ─────────────────────────────────────────────────────────

    [Fact]
    public void Parse_WithMalformedJson_ReturnsNull()
    {
        var record = MakeRecord("não é JSON { quebrado");
        SensorParser.Parse(record).Should().BeNull();
    }

    [Fact]
    public void Parse_WithEmptyMessage_ReturnsNull()
    {
        var record = MakeRecord("");
        SensorParser.Parse(record).Should().BeNull();
    }

    [Fact]
    public void Parse_WithEmptyJsonObject_ReturnsRecordWithDefaults()
    {
        var record = MakeRecord("{}");
        var result = SensorParser.Parse(record);

        result.Should().NotBeNull();
        result!.MachineId.Should().BeEmpty();
        result.State.Should().Be("normal");
        result.Vibration.Should().Be(0.0);
    }

    // ── ReceivedAt preservado ─────────────────────────────────────────────────

    [Fact]
    public void Parse_AlwaysPreservesRecordReceivedAt()
    {
        const string receivedAt = "2026-04-24T15:30:00Z";
        var record = MakeRecord("""{"machine_id":"M1"}""", receivedAt);
        SensorParser.Parse(record)!.ReceivedAt.Should().Be(receivedAt);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static IoTData MakeRecord(string message, string? receivedAt = null) => new()
    {
        Id         = 1,
        Topic      = "sensors/M1/data",
        Message    = message,
        ReceivedAt = receivedAt ?? "2026-04-24T10:00:00Z",
    };
}
