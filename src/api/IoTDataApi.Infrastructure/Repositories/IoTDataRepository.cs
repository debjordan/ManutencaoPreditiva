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
        return await _context.IoTData
            .OrderByDescending(d => d.ReceivedAt)
            .Take(100)
            .ToListAsync();
    }

    public async Task<IEnumerable<IoTData>> GetByMachineIdAsync(string machineId)
    {
        return await _context.IoTData
            .Where(d => d.Topic.Contains(machineId))
            .OrderByDescending(d => d.ReceivedAt)
            .Take(100)
            .ToListAsync();
    }
}