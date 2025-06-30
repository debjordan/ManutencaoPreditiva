using ManutencaoPreditiva.Application.DTOs;
using ManutencaoPreditiva.Domain.Common;

namespace ManutencaoPreditiva.Application.Services
{
    public interface ISensorService
    {
        Task<Result<bool>> SaveSensorDataAsync(SensorData data);
        Task<Result<List<SensorResponse>>> GetSensorsAsync(int limit = 100);
        Task<Result<List<SensorResponse>>> GetSensorsByMachineAsync(string machineId, int limit = 100);
    }
}
