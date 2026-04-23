using IoTDataApi.Application.DTOs;

namespace IoTDataApi.Application.Interfaces;

public interface IRulService
{
    Task<RulDto?> GetForMachineAsync(string machineId);
    Task<IEnumerable<RulDto>> GetForAllMachinesAsync();
}
