using Microsoft.EntityFrameworkCore;
using IoTDataApi.Domain.Entities;
using IoTDataApi.Domain.Interfaces;
using IoTDataApi.Infrastructure.Data;

namespace IoTDataApi.Infrastructure.Repositories;

public class IoTDataRepository : IIoTDataRepository
{
    private readonly IoTDataContext _context;

    public IoTDataRepository(IoTDataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<IoTData>> GetAllAsync()
    {
        try
        {
            return await _context.IoTData
                .OrderByDescending(d => d.ReceivedAt)
                .Take(100)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Log and return empty list to avoid 500 when DB is temporarily unavailable
            Console.Error.WriteLine($"IoTDataRepository.GetAllAsync error: {ex.Message}");
            return Enumerable.Empty<IoTData>();
        }
    }

    public async Task<IEnumerable<IoTData>> GetByMachineIdAsync(string machineId)
    {
        try
        {
            // Exact topic match prevents M1 from matching M10, M11, etc.
            var topic = $"sensors/{machineId}/data";
            return await _context.IoTData
                .Where(d => d.Topic == topic)
                .OrderByDescending(d => d.ReceivedAt)
                .Take(100)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"IoTDataRepository.GetByMachineIdAsync error: {ex.Message}");
            return Enumerable.Empty<IoTData>();
        }
    }
}
