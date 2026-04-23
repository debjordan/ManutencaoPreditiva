using Microsoft.Extensions.Caching.Memory;
using IoTDataApi.Application.DTOs;
using IoTDataApi.Application.Interfaces;
using IoTDataApi.Application.Parsers;
using IoTDataApi.Domain.Interfaces;

namespace IoTDataApi.Application.Services;

public sealed class DowntimeService : IDowntimeService
{
    private static readonly string[] KnownMachines = ["M1", "M2", "M3", "M4", "M5", "M6"];

    // Estimated cost of one hour of unplanned downtime per machine type.
    // Based on production capacity and labour cost — used for ROI visualization only.
    private static readonly Dictionary<string, double> HourlyDowntimeCost = new()
    {
        ["M1"] = 500.0,
        ["M2"] = 800.0,
        ["M3"] = 300.0,
        ["M4"] = 400.0,
        ["M5"] = 600.0,
        ["M6"] = 450.0,
    };

    private const double DefaultHourlyCost       = 400.0;
    private const double ReadingIntervalSeconds   = 5.0;

    private readonly IIoTDataRepository _repository;
    private readonly IMemoryCache       _cache;
    private static readonly TimeSpan    CacheTtl = TimeSpan.FromSeconds(30);

    public DowntimeService(IIoTDataRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache      = cache;
    }

    public async Task<DowntimeSummaryDto?> GetForMachineAsync(string machineId)
    {
        string cacheKey = $"downtime:{machineId}";

        if (_cache.TryGetValue(cacheKey, out DowntimeSummaryDto? cached))
            return cached;

        var since    = DateTime.UtcNow.AddHours(-24);
        var records  = await _repository.GetByMachineIdSinceAsync(machineId, since);
        var readings = records.Select(SensorParser.Parse).OfType<SensorReadingDto>().ToList();

        if (readings.Count == 0)
            return null;

        var firstReading = readings.First();
        var result = BuildSummary(machineId, firstReading.MachineName, firstReading.Area, readings);
        _cache.Set(cacheKey, result, CacheTtl);
        return result;
    }

    public async Task<IEnumerable<DowntimeSummaryDto>> GetForAllMachinesAsync()
    {
        var tasks   = KnownMachines.Select(GetForMachineAsync);
        var results = await Task.WhenAll(tasks);
        return results.OfType<DowntimeSummaryDto>();
    }

    private static DowntimeSummaryDto BuildSummary(
        string machineId, string machineName, string area,
        List<SensorReadingDto> readings)
    {
        int totalReadings    = readings.Count;
        int criticalReadings = readings.Count(r => r.State == "critical");
        int normalReadings   = totalReadings - criticalReadings;

        double periodHours    = totalReadings  * ReadingIntervalSeconds / 3600.0;
        double downtimeMin    = criticalReadings * ReadingIntervalSeconds / 60.0;
        double uptimeMin      = normalReadings   * ReadingIntervalSeconds / 60.0;

        int failureEvents = CountDistinctFailureEvents(readings);

        double mttr = failureEvents > 0 ? Math.Round(downtimeMin  / failureEvents, 1) : 0;
        double mtbf = failureEvents > 0 ? Math.Round(uptimeMin / 60.0 / failureEvents, 2)
                                        : Math.Round(periodHours, 2);
        double availabilityPct = totalReadings > 0
            ? Math.Round(normalReadings * 100.0 / totalReadings, 1)
            : 100.0;

        HourlyDowntimeCost.TryGetValue(machineId, out double costPerHour);
        if (costPerHour == 0) costPerHour = DefaultHourlyCost;

        return new DowntimeSummaryDto
        {
            MachineId             = machineId,
            MachineName           = machineName,
            Area                  = area,
            TotalReadingsAnalyzed = totalReadings,
            PeriodHours           = Math.Round(periodHours, 2),
            DowntimeMinutes       = Math.Round(downtimeMin, 1),
            AvailabilityPct       = availabilityPct,
            FailureEvents         = failureEvents,
            MttrMinutes           = mttr,
            MtbfHours             = mtbf,
            CostPerHour           = costPerHour,
            EstimatedCostBrl      = Math.Round(downtimeMin / 60.0 * costPerHour, 2),
        };
    }

    // Counts transitions into "critical" state (newest-first ordering).
    private static int CountDistinctFailureEvents(List<SensorReadingDto> readings)
    {
        int events   = 0;
        bool inCrisis = false;

        foreach (var reading in readings)
        {
            if (reading.State == "critical" && !inCrisis)
            {
                events++;
                inCrisis = true;
            }
            else if (reading.State != "critical")
            {
                inCrisis = false;
            }
        }

        return events;
    }
}
