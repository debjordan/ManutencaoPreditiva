using Microsoft.Extensions.Caching.Memory;
using IoTDataApi.Application.Algorithms;
using IoTDataApi.Application.DTOs;
using IoTDataApi.Application.Interfaces;
using IoTDataApi.Application.Parsers;
using IoTDataApi.Domain.Interfaces;

namespace IoTDataApi.Application.Services;

public sealed class RulService : IRulService
{
    private static readonly string[] KnownMachines = ["M1", "M2", "M3", "M4", "M5", "M6"];

    // (warning, critical) thresholds per sensor
    private static readonly Dictionary<string, double> CriticalThresholds = new()
    {
        ["vibration"]   = 12.0,
        ["temperature"] = 60.0,
        ["pressure"]    = 5.5,
        ["current"]     = 21.0,
    };

    private readonly IIoTDataRepository _repository;
    private readonly IRulStrategy       _strategy;
    private readonly IMemoryCache       _cache;

    // Cache slightly longer than the simulator interval (5s) so consecutive
    // requests within the same polling cycle hit the cache instead of running
    // O(n) regression on every HTTP request.
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(15);

    public RulService(IIoTDataRepository repository, IRulStrategy strategy, IMemoryCache cache)
    {
        _repository = repository;
        _strategy   = strategy;
        _cache      = cache;
    }

    public async Task<RulDto?> GetForMachineAsync(string machineId)
    {
        string cacheKey = $"rul:{machineId}";

        if (_cache.TryGetValue(cacheKey, out RulDto? cached))
            return cached;

        var records  = await _repository.GetByMachineIdAsync(machineId, limit: 120);
        var readings = records.Select(SensorParser.Parse).OfType<SensorReadingDto>().ToList();

        if (readings.Count < 10)
            return null;

        // oldest-first for regression
        readings.Reverse();

        var result = BuildRul(machineId, readings.Last().MachineName, readings);
        _cache.Set(cacheKey, result, CacheTtl);
        return result;
    }

    public async Task<IEnumerable<RulDto>> GetForAllMachinesAsync()
    {
        var tasks   = KnownMachines.Select(GetForMachineAsync);
        var results = await Task.WhenAll(tasks);
        return results.OfType<RulDto>();
    }

    private RulDto BuildRul(string machineId, string machineName, List<SensorReadingDto> readings)
    {
        string? limitingSensor = null;
        RulEstimate? best      = null;

        var sensorSelectors = new Dictionary<string, Func<SensorReadingDto, double>>
        {
            ["vibration"]   = r => r.Vibration,
            ["temperature"] = r => r.Temperature,
            ["pressure"]    = r => r.Pressure,
            ["current"]     = r => r.Current,
        };

        foreach (var (sensor, selector) in sensorSelectors)
        {
            if (!CriticalThresholds.TryGetValue(sensor, out double threshold))
                continue;

            double[] values  = readings.Select(selector).ToArray();
            var estimate     = _strategy.Estimate(values, threshold);

            if (estimate is null) continue;

            if (best is null || estimate.HoursToFailure < best.HoursToFailure)
            {
                best            = estimate;
                limitingSensor  = sensor;
            }
        }

        if (best is null)
            return new RulDto
            {
                MachineId      = machineId,
                MachineName    = machineName,
                Confidence     = "n/a",
                Interpretation = "Sensores estáveis ou melhorando — sem tendência de falha identificada.",
            };

        string confidence = best.R2 > 0.7 ? "alta" : best.R2 > 0.4 ? "média" : "baixa";
        string interpretation = best.HoursToFailure < 2
            ? $"Falha iminente em {best.HoursToFailure:F1}h — intervenção imediata necessária."
            : best.HoursToFailure < 8
                ? $"Agendar manutenção preventiva nas próximas {best.HoursToFailure:F1}h."
                : $"Tendência de degradação detectada; estimar inspeção em {best.HoursToFailure:F1}h.";

        return new RulDto
        {
            MachineId               = machineId,
            MachineName             = machineName,
            LimitingSensor          = limitingSensor,
            EstimatedHoursToFailure = best.HoursToFailure,
            TrendSlope              = best.Slope,
            Confidence              = confidence,
            ConfidenceR2            = best.R2,
            Interpretation          = interpretation,
        };
    }
}
