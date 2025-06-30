using ManutencaoPreditiva.Application.DTOs.Request;
using ManutencaoPreditiva.Application.DTOs.Response;
using ManutencaoPreditiva.Domain.Common;

namespace ManutencaoPreditiva.Application.Interfaces.Services;

public interface IMachineApplicationService
{
    Task<Result<MachineDto>> GetMachineByIdAsync(Guid id);
    Task<Result<IEnumerable<MachineDto>>> GetAllMachinesAsync();
    Task<Result<IEnumerable<MachineDto>>> GetActiveMachinesAsync();
    Task<Result<IEnumerable<MachineDto>>> GetMachinesByLocationAsync(string location);
    Task<Result<IEnumerable<MachineDto>>> GetMachinesRequiringMaintenanceAsync();
    Task<Result<MachineDto>> CreateMachineAsync(CreateMachineDto dto);
    Task<Result<MachineDto>> UpdateMachineByIdAsync(Guid id, UpdateMachineDto dto);
    Task<Result<bool>> DeleteMachineByIdAsync(Guid id);
    Task<Result<bool>> ScheduleMaintenanceAsync(Guid machineId, DateTime maintenanceDate);
    Task<Result<bool>> UpdateMaintenanceAsync(Guid machineId, DateTime maintenanceDate);
}
