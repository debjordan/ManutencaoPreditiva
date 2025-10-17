using IoTDataApi.Domain.Entities;

namespace IoTDataApi.Application.Interfaces;

public interface IIoTDataService
{
    Task<IEnumerable<IoTData>> GetAllDataAsync();
    Task<IEnumerable<IoTData>> GetMachineDataAsync(string machineId);
}