using IoTDataApi.Domain.Entities;
using IoTDataApi.Domain.Interfaces;
using IoTDataApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IoTDataApi.Infrastructure.Repositories;

public class IoTDataRepository(IoTDataContext context) : IIoTDataRepository
{
    public async Task<IEnumerable<IoTData>> GetAllAsync()
    {
        try
        {
            return await context.IoTData
                .OrderByDescending(d => d.ReceivedAt)
                .Take(100)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"IoTDataRepository.GetAllAsync error: {ex.Message}");
            return [];
        }
    }

    public async Task<IEnumerable<IoTData>> GetByMachineIdAsync(string machineId)
        => await GetByMachineIdAsync(machineId, 100);

    public async Task<IEnumerable<IoTData>> GetByMachineIdAsync(string machineId, int limit)
    {
        try
        {
            var topic = $"sensors/{machineId}/data";
            return await context.IoTData
                .Where(d => d.Topic == topic)
                .OrderByDescending(d => d.ReceivedAt)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"IoTDataRepository.GetByMachineIdAsync error: {ex.Message}");
            return [];
        }
    }

    public async Task<IEnumerable<IoTData>> GetByMachineIdSinceAsync(string machineId, DateTime since)
    {
        try
        {
            var topic    = $"sensors/{machineId}/data";
            var sinceIso = since.ToString("o");
            return await context.IoTData
                .Where(d => d.Topic == topic && string.Compare(d.ReceivedAt, sinceIso) >= 0)
                .OrderByDescending(d => d.ReceivedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"IoTDataRepository.GetByMachineIdSinceAsync error: {ex.Message}");
            return [];
        }
    }
}
