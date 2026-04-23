using IoTDataApi.Application.DTOs;

namespace IoTDataApi.Application.Interfaces;

public interface IMaintenanceEventService
{
    Task<IEnumerable<MaintenanceEventDto>> GetAllEventsAsync();
    Task<IEnumerable<MaintenanceEventDto>> GetMachineEventsAsync(string machineId);
    Task<MaintenanceEventDto?> GetEventByIdAsync(int id);
    Task<MaintenanceEventDto> CreateEventAsync(CreateMaintenanceEventDto dto);
    Task<MaintenanceEventDto?> ResolveEventAsync(int id, ResolveMaintenanceEventDto dto);
}
