using System.Text.Json;
using IoTDataApi.Application.DTOs;
using IoTDataApi.Domain.Entities;
using IoTDataApi.Domain.Interfaces;
using IoTDataApi.Application.Interfaces;

namespace IoTDataApi.Application.Services;

public class IoTDataService : IIoTDataService
{
    private readonly IIoTDataRepository _repository;

    // (warning, critical) thresholds per sensor
    private static readonly Dictionary<string, (double Warning, double Critical)> Thresholds = new()
    {
        ["vibration"]   = (10.0, 12.0),
        ["temperature"] = (55.0, 60.0),
        ["pressure"]    = (5.0,  5.5),
        ["humidity"]    = (70.0, 80.0),
        ["current"]     = (18.0, 21.0),
    };

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
        var records = await _repository.GetByMachineIdAsync(machineId);
        return records.Take(limit).Select(ParseReading).OfType<SensorReadingDto>();
    }

    public async Task<IEnumerable<MachineStatsDto>> GetAllMachineStatsAsync()
    {
        var all = await _repository.GetAllAsync();
        return all
            .Select(ParseReading)
            .OfType<SensorReadingDto>()
            .GroupBy(r => r.MachineId)
            .Select(g => BuildStats(g.Key, g.ToList()));
    }

    public async Task<MachineStatsDto?> GetMachineStatsAsync(string machineId)
    {
        var records = await _repository.GetByMachineIdAsync(machineId);
        var readings = records.Select(ParseReading).OfType<SensorReadingDto>().ToList();
        return readings.Count == 0 ? null : BuildStats(machineId, readings);
    }

    public async Task<IEnumerable<AlertDto>> GetActiveAlertsAsync()
    {
        var all = await _repository.GetAllAsync();
        var alerts = new List<AlertDto>();

        var groups = all
            .Select(ParseReading)
            .OfType<SensorReadingDto>()
            .GroupBy(r => r.MachineId);

        foreach (var group in groups)
        {
            var latest = group.MaxBy(r => r.Timestamp);
            if (latest is null) continue;
            CheckThreshold(alerts, latest, "vibration",   latest.Vibration);
            CheckThreshold(alerts, latest, "temperature", latest.Temperature);
            CheckThreshold(alerts, latest, "pressure",    latest.Pressure);
            CheckThreshold(alerts, latest, "humidity",    latest.Humidity);
            CheckThreshold(alerts, latest, "current",     latest.Current);
        }

        return alerts.OrderByDescending(a => a.Severity);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static void CheckThreshold(List<AlertDto> alerts, SensorReadingDto r,
                                       string sensor, double value)
    {
        if (!Thresholds.TryGetValue(sensor, out var t)) return;

        if (value >= t.Critical)
            alerts.Add(new AlertDto
            {
                MachineId   = r.MachineId,
                MachineName = r.MachineName,
                Area        = r.Area,
                Severity    = "CRÍTICO",
                Sensor      = sensor,
                Value       = value,
                Threshold   = t.Critical,
                Message     = $"{sensor} em {value:F2} — acima do limite crítico ({t.Critical})",
                DetectedAt  = r.ReceivedAt,
            });
        else if (value >= t.Warning)
            alerts.Add(new AlertDto
            {
                MachineId   = r.MachineId,
                MachineName = r.MachineName,
                Area        = r.Area,
                Severity    = "ALERTA",
                Sensor      = sensor,
                Value       = value,
                Threshold   = t.Warning,
                Message     = $"{sensor} em {value:F2} — atenção, acima de {t.Warning}",
                DetectedAt  = r.ReceivedAt,
            });
    }

    private static MachineStatsDto BuildStats(string machineId, List<SensorReadingDto> readings)
    {
        var latest = readings.MaxBy(r => r.Timestamp)!;

        double CalcRisk()
        {
            var vibRisk  = Math.Min(latest.Vibration  / 15.0 * 100, 100);
            var tempRisk = Math.Min(latest.Temperature / 70.0 * 100, 100);
            var presRisk = Math.Min(latest.Pressure   / 6.0  * 100, 100);
            return Math.Round((vibRisk + tempRisk + presRisk) / 3.0, 1);
        }

        double CalcOee()
        {
            double availability = latest.State switch
            {
                "critical"  => 60.0,
                "degrading" => 82.0,
                _           => 97.0,
            };
            double performance = Math.Max(0, Math.Min(100, (1.0 - latest.Vibration  / 20.0) * 100));
            double quality     = Math.Max(0, Math.Min(100, (1.0 - latest.Temperature / 80.0) * 100));
            return Math.Round(availability * performance * quality / 10000.0, 1);
        }

        return new MachineStatsDto
        {
            MachineId    = machineId,
            MachineName  = latest.MachineName,
            Area         = latest.Area,
            CurrentState = latest.State,
            RecordCount  = readings.Count,
            RiskScore    = CalcRisk(),
            Oee          = CalcOee(),
            LastSeen     = latest.ReceivedAt,
            Vibration    = Stat(readings.Select(r => r.Vibration)),
            Temperature  = Stat(readings.Select(r => r.Temperature)),
            Pressure     = Stat(readings.Select(r => r.Pressure)),
            Humidity     = Stat(readings.Select(r => r.Humidity)),
            Voltage      = Stat(readings.Select(r => r.Voltage)),
            Current      = Stat(readings.Select(r => r.Current)),
            Power        = Stat(readings.Select(r => r.Power)),
        };
    }

    private static SensorStats Stat(IEnumerable<double> values)
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

    private static SensorReadingDto? ParseReading(IoTData record)
    {
        try
        {
            using var doc = JsonDocument.Parse(record.Message);
            var root = doc.RootElement;

            string G(string key, string fallback = "") =>
                root.TryGetProperty(key, out var p) ? p.GetString() ?? fallback : fallback;
            double D(string key, double fallback = 0) =>
                root.TryGetProperty(key, out var p) && p.TryGetDouble(out var v) ? v : fallback;

            return new SensorReadingDto
            {
                MachineId   = G("machine_id"),
                MachineName = G("machine_name", G("machine_id")),
                MachineType = G("machine_type"),
                Area        = G("area"),
                State       = G("state", "normal"),
                Vibration   = D("vibration"),
                Temperature = D("temperature"),
                Pressure    = D("pressure"),
                Humidity    = D("humidity"),
                Voltage     = D("voltage"),
                Current     = D("current"),
                Power       = D("power"),
                Timestamp   = G("timestamp"),
                ReceivedAt  = record.ReceivedAt,
            };
        }
        catch
        {
            return null;
        }
    }
}
