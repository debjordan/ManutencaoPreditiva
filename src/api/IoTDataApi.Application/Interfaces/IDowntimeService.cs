using IoTDataApi.Application.DTOs;

namespace IoTDataApi.Application.Interfaces;

public interface IDowntimeService
{
    Task<DowntimeSummaryDto?> GetForMachineAsync(string machineId);
    Task<IEnumerable<DowntimeSummaryDto>> GetForAllMachinesAsync();
}
