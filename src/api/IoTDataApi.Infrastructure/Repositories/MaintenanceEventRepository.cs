using IoTDataApi.Domain.Entities;
using IoTDataApi.Domain.Interfaces;
using IoTDataApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IoTDataApi.Infrastructure.Repositories;

public class MaintenanceEventRepository(IoTDataContext context) : IMaintenanceEventRepository
{
    public async Task<IEnumerable<MaintenanceEvent>> GetAllAsync()
        => await context.MaintenanceEvents
            .OrderByDescending(e => e.StartedAt)
            .ToListAsync();

    public async Task<IEnumerable<MaintenanceEvent>> GetByMachineIdAsync(string machineId)
        => await context.MaintenanceEvents
            .Where(e => e.MachineId == machineId)
            .OrderByDescending(e => e.StartedAt)
            .ToListAsync();

    public async Task<MaintenanceEvent?> GetByIdAsync(int id)
        => await context.MaintenanceEvents.FindAsync(id);

    public async Task<MaintenanceEvent> AddAsync(MaintenanceEvent ev)
    {
        context.MaintenanceEvents.Add(ev);
        await context.SaveChangesAsync();
        return ev;
    }

    public async Task<MaintenanceEvent?> UpdateAsync(MaintenanceEvent ev)
    {
        context.MaintenanceEvents.Update(ev);
        await context.SaveChangesAsync();
        return ev;
    }
}
