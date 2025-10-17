using IoTDataApi.Domain.Entities;

namespace IoTDataApi.Domain.Interfaces;

public interface IIoTDataRepository
{
    Task<IEnumerable<IoTData>> GetAllAsync();
    Task<IEnumerable<IoTData>> GetByMachineIdAsync(string machineId);
}