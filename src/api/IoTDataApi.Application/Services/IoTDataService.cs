using IoTDataApi.Domain.Entities;
using IoTDataApi.Domain.Interfaces;
using IoTDataApi.Application.Interfaces;

namespace IoTDataApi.Application.Services;

public class IoTDataService : IIoTDataService
{
    private readonly IIoTDataRepository _repository;

    public IoTDataService(IIoTDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<IoTData>> GetAllDataAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<IEnumerable<IoTData>> GetMachineDataAsync(string machineId)
    {
        return await _repository.GetByMachineIdAsync(machineId);
    }
}