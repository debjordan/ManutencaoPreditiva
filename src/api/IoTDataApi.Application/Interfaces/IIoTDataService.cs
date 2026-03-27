using IoTDataApi.Application.DTOs;
using IoTDataApi.Domain.Entities;

namespace IoTDataApi.Application.Interfaces;

public interface IIoTDataService
{
    Task<IEnumerable<IoTData>> GetAllDataAsync();
    Task<IEnumerable<IoTData>> GetMachineDataAsync(string machineId);
    Task<IEnumerable<SensorReadingDto>> GetMachineReadingsAsync(string machineId, int limit = 100);
    Task<IEnumerable<MachineStatsDto>> GetAllMachineStatsAsync();
    Task<MachineStatsDto?> GetMachineStatsAsync(string machineId);
    Task<IEnumerable<AlertDto>> GetActiveAlertsAsync();
}
