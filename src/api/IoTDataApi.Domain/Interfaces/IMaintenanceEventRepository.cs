using IoTDataApi.Domain.Entities;

namespace IoTDataApi.Domain.Interfaces;

public interface IMaintenanceEventRepository
{
    Task<IEnumerable<MaintenanceEvent>> GetAllAsync();
    Task<IEnumerable<MaintenanceEvent>> GetByMachineIdAsync(string machineId);
    Task<MaintenanceEvent?> GetByIdAsync(int id);
    Task<MaintenanceEvent> AddAsync(MaintenanceEvent ev);
    Task<MaintenanceEvent?> UpdateAsync(MaintenanceEvent ev);
}
