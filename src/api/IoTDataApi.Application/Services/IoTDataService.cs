using IoTDataApi.Application.DTOs;
using IoTDataApi.Application.Interfaces;
using IoTDataApi.Application.Parsers;
using IoTDataApi.Domain.Entities;
using IoTDataApi.Domain.Interfaces;

namespace IoTDataApi.Application.Services;

/// <summary>
/// Responsabilidade: consultas de dados brutos, estatísticas agregadas e alertas ativos.
/// RUL, tendências e downtime vivem em serviços dedicados (SRP).
/// </summary>
public sealed class IoTDataService : IIoTDataService
{
    private static readonly Dictionary<string, (double Warning, double Critical)> Thresholds = new()
    {
        ["vibration"]   = (10.0, 12.0),
        ["temperature"] = (55.0, 60.0),
        ["pressure"]    = (5.0,  5.5),
        ["humidity"]    = (70.0, 80.0),
        ["current"]     = (18.0, 21.0),
    };

    private readonly IIoTDataRepository _repository;

    public IoTDataService(IIoTDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<IoTData>> GetAllDataAsync()
        => await _repository.GetAllAsync();

    public async Task<IEnumerable<IoTData>> GetMachineDataAsync(string machineId)
        => await _repository.GetByMachineIdAsync(machineId);

    public async Task<IEnumerable<SensorReadingDto>> GetMachineReadingsAsync(string machineId, int limit = 100)
    {
        var records = await _repository.GetByMachineIdAsync(machineId, limit);
        return records.Select(SensorParser.Parse).OfType<SensorReadingDto>();
    }

    public async Task<IEnumerable<MachineStatsDto>> GetAllMachineStatsAsync()
    {
        var records = await _repository.GetAllAsync();
        return records
            .Select(SensorParser.Parse)
            .OfType<SensorReadingDto>()
            .GroupBy(reading => reading.MachineId)
            .Select(group => BuildStats(group.Key, group.ToList()));
    }

    public async Task<MachineStatsDto?> GetMachineStatsAsync(string machineId)
    {
        var records  = await _repository.GetByMachineIdAsync(machineId);
        var readings = records.Select(SensorParser.Parse).OfType<SensorReadingDto>().ToList();
        return readings.Count == 0 ? null : BuildStats(machineId, readings);
    }

    public async Task<IEnumerable<AlertDto>> GetActiveAlertsAsync()
    {
        var records  = await _repository.GetAllAsync();
        var alerts   = new List<AlertDto>();

        var latestPerMachine = records
            .Select(SensorParser.Parse)
            .OfType<SensorReadingDto>()
            .GroupBy(reading => reading.MachineId)
            .Select(group => group.MaxBy(reading => reading.Timestamp))
            .OfType<SensorReadingDto>();

        foreach (var latest in latestPerMachine)
        {
            CheckThreshold(alerts, latest, "vibration",   latest.Vibration);
            CheckThreshold(alerts, latest, "temperature", latest.Temperature);
            CheckThreshold(alerts, latest, "pressure",    latest.Pressure);
            CheckThreshold(alerts, latest, "humidity",    latest.Humidity);
            CheckThreshold(alerts, latest, "current",     latest.Current);
        }

        return alerts.OrderByDescending(alert => alert.Severity);
    }

    // ── private builders ──────────────────────────────────────────────────────

    private static MachineStatsDto BuildStats(string machineId, List<SensorReadingDto> readings)
    {
        var latest = readings.MaxBy(reading => reading.Timestamp)!;

        double riskScore = CalculateRiskScore(latest);
        double oee       = CalculateOee(latest);

        return new MachineStatsDto
        {
            MachineId    = machineId,
            MachineName  = latest.MachineName,
            Area         = latest.Area,
            CurrentState = latest.State,
            RecordCount  = readings.Count,
            RiskScore    = riskScore,
            Oee          = oee,
            LastSeen     = latest.ReceivedAt,
            Vibration    = AggregateStats(readings.Select(r => r.Vibration)),
            Temperature  = AggregateStats(readings.Select(r => r.Temperature)),
            Pressure     = AggregateStats(readings.Select(r => r.Pressure)),
            Humidity     = AggregateStats(readings.Select(r => r.Humidity)),
            Voltage      = AggregateStats(readings.Select(r => r.Voltage)),
            Current      = AggregateStats(readings.Select(r => r.Current)),
            Power        = AggregateStats(readings.Select(r => r.Power)),
        };
    }

    private static double CalculateRiskScore(SensorReadingDto latest)
    {
        double vibRisk  = Math.Min(latest.Vibration  / 15.0 * 100, 100);
        double tempRisk = Math.Min(latest.Temperature / 70.0 * 100, 100);
        double presRisk = Math.Min(latest.Pressure   / 6.0  * 100, 100);
        return Math.Round((vibRisk + tempRisk + presRisk) / 3.0, 1);
    }

    private static double CalculateOee(SensorReadingDto latest)
    {
        double availability = latest.State switch
        {
            "critical"  => 60.0,
            "degrading" => 82.0,
            _           => 97.0,
        };
        double performance = Math.Max(0, Math.Min(100, (1.0 - latest.Vibration  / 20.0) * 100));
        double quality     = Math.Max(0, Math.Min(100, (1.0 - latest.Temperature / 80.0) * 100));
        return Math.Round(availability * performance * quality / 10_000.0, 1);
    }

    private static SensorStats AggregateStats(IEnumerable<double> values)
    {
        var list = values.ToList();
        if (list.Count == 0) return new SensorStats();
        return new SensorStats
        {
            Min  = Math.Round(list.Min(), 2),
            Max  = Math.Round(list.Max(), 2),
            Avg  = Math.Round(list.Average(), 2),
            Last = Math.Round(list.First(), 2),
        };
    }

    private static void CheckThreshold(List<AlertDto> alerts, SensorReadingDto reading,
                                       string sensor, double value)
    {
        if (!Thresholds.TryGetValue(sensor, out var threshold)) return;

        if (value >= threshold.Critical)
        {
            alerts.Add(new AlertDto
            {
                MachineId   = reading.MachineId,
                MachineName = reading.MachineName,
                Area        = reading.Area,
                Severity    = "CRÍTICO",
                Sensor      = sensor,
                Value       = value,
                Threshold   = threshold.Critical,
                Message     = $"{sensor} em {value:F2} — acima do limite crítico ({threshold.Critical})",
                DetectedAt  = reading.ReceivedAt,
            });
        }
        else if (value >= threshold.Warning)
        {
            alerts.Add(new AlertDto
            {
                MachineId   = reading.MachineId,
                MachineName = reading.MachineName,
                Area        = reading.Area,
                Severity    = "ALERTA",
                Sensor      = sensor,
                Value       = value,
                Threshold   = threshold.Warning,
                Message     = $"{sensor} em {value:F2} — atenção, acima de {threshold.Warning}",
                DetectedAt  = reading.ReceivedAt,
            });
        }
    }
}
