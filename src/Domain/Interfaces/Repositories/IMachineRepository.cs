using ManutencaoPreditiva.Domain.Entities;

namespace ManutencaoPreditiva.Domain.Interfaces.Repositories;

public interface IMachineRepository
{
    Task<Machine?> GetByIdAsync(Guid id);
    Task<Machine?> GetBySerialNumberAsync(string serialNumber);
    Task<IEnumerable<Machine>> GetAllAsync();
    Task<IEnumerable<Machine>> GetActiveAsync();
    Task<IEnumerable<Machine>> GetByLocationAsync(string location);
    Task<IEnumerable<Machine>> GetRequiringMaintenanceAsync();
    Task<bool> ExistsAsync(Guid id);
    Task<bool> SerialNumberExistsAsync(string serialNumber);
    Task<Machine> AddAsync(Machine machine);
    Task<Machine> UpdateAsync(Machine machine);
    Task DeleteAsync(Guid id);
    Task<int> CountAsync();
}
