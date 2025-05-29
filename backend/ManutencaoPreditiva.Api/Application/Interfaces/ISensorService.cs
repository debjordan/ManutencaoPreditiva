using ManutencaoPreditiva.Api.Application.DTOs;
using ManutencaoPreditiva.Api.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManutencaoPreditiva.Api.Application.Services
{
    public interface ISensorService
    {
        Task<Result<bool>> SaveSensorDataAsync(SensorData data);
        Task<Result<List<SensorResponse>>> GetSensorsAsync(int limit = 100);
        Task<Result<List<SensorResponse>>> GetSensorsByMachineAsync(string machineId, int limit = 100);
    }
}
