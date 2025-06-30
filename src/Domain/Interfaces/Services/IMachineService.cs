using ManutencaoPreditiva.Domain.Common;
using ManutencaoPreditiva.Domain.Entities;

namespace ManutencaoPreditiva.Domain.Interfaces.Services;

public interface IMachineService
{
    Task<Result<Machine>> GetMachineByIdAsync(Guid id);
    Task<Result<IEnumerable<Machine>>> GetAllMachinesAsync();
    Task<Result<IEnumerable<Machine>>> GetActiveMachinesAsync();
    Task<Result<IEnumerable<Machine>>> GetMachinesByLocationAsync(string location);
    Task<Result<IEnumerable<Machine>>> GetMachinesRequiringMaintenanceAsync();
    Task<Result<Machine>> CreateMachineAsync(Machine machine);
    Task<Result<Machine>> UpdateMachineAsync(Machine machine);
    Task<Result<bool>> DeleteMachineAsync(Guid id);
    Task<Result<bool>> ScheduleMaintenanceAsync(Guid machineId, DateTime maintenanceDate);
    Task<Result<bool>> UpdateMaintenanceAsync(Guid machineId, DateTime maintenanceDate);
}
