using IoTDataApi.Application.DTOs;

namespace IoTDataApi.Application.Interfaces;

public interface ITrendService
{
    Task<MachineTrendsDto?> GetForMachineAsync(string machineId);
    Task<IEnumerable<MachineTrendsDto>> GetForAllMachinesAsync();
}
