using Microsoft.Extensions.Caching.Memory;
using IoTDataApi.Application.DTOs;
using IoTDataApi.Application.Interfaces;
using IoTDataApi.Application.Parsers;
using IoTDataApi.Domain.Interfaces;

namespace IoTDataApi.Application.Services;

public sealed class TrendService : ITrendService
{
    private static readonly string[] KnownMachines = ["M1", "M2", "M3", "M4", "M5", "M6"];

    // A 3% delta relative to the previous window is the minimum meaningful change
    // for industrial sensors at this precision. Below this, noise dominates the signal.
    private const double TrendThresholdPct = 0.03;

    private readonly IIoTDataRepository _repository;
    private readonly IMemoryCache       _cache;
    private static readonly TimeSpan    CacheTtl = TimeSpan.FromSeconds(15);

    public TrendService(IIoTDataRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache      = cache;
    }

    public async Task<MachineTrendsDto?> GetForMachineAsync(string machineId)
    {
        string cacheKey = $"trend:{machineId}";

        if (_cache.TryGetValue(cacheKey, out MachineTrendsDto? cached))
            return cached;

        var records  = await _repository.GetByMachineIdAsync(machineId, limit: 20);
        var readings = records.Select(SensorParser.Parse).OfType<SensorReadingDto>().ToList();

        if (readings.Count < 6)
            return null;

        var result = BuildTrends(machineId, readings.First().MachineName, readings);
        _cache.Set(cacheKey, result, CacheTtl);
        return result;
    }

    public async Task<IEnumerable<MachineTrendsDto>> GetForAllMachinesAsync()
    {
        var tasks   = KnownMachines.Select(GetForMachineAsync);
        var results = await Task.WhenAll(tasks);
        return results.OfType<MachineTrendsDto>();
    }

    private MachineTrendsDto BuildTrends(string machineId, string machineName, List<SensorReadingDto> readings)
    {
        // readings are newest-first; we compare last 5 vs previous 5
        var sensorSelectors = new List<(string name, Func<SensorReadingDto, double> selector)>
        {
            ("vibration",   r => r.Vibration),
            ("temperature", r => r.Temperature),
            ("pressure",    r => r.Pressure),
            ("current",     r => r.Current),
            ("humidity",    r => r.Humidity),
        };

        var trends = sensorSelectors.Select(sensor =>
        {
            var values  = readings.Take(10).Select(sensor.selector).ToList();
            double last5 = values.Take(5).Average();
            double prev5 = values.Count >= 10 ? values.Skip(5).Take(5).Average() : values.Last();
            double delta = last5 - prev5;
            double threshold = Math.Abs(prev5) * TrendThresholdPct;

            return new SensorTrendDto
            {
                Sensor    = sensor.name,
                Direction = delta > threshold ? "subindo" : delta < -threshold ? "caindo" : "estável",
                Delta     = Math.Round(delta, 3),
                Last5Avg  = Math.Round(last5, 2),
                Prev5Avg  = Math.Round(prev5, 2),
            };
        }).ToList();

        return new MachineTrendsDto
        {
            MachineId   = machineId,
            MachineName = machineName,
            Sensors     = trends,
        };
    }
}
