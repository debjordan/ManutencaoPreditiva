// using ManutencaoPreditiva.Api.Application.DTOs;
// using ManutencaoPreditiva.Api.Domain.Common;
// using ManutencaoPreditiva.Application.Services;
// using System.Globalization;

// namespace ManutencaoPreditiva.Api.Application.Services
// {
//     public class SensorService : ISensorService
//     {
//         private readonly ISensorRepository _sensorRepository;
//         private readonly ILogger<SensorService> _logger;

//         public SensorService(ISensorRepository sensorRepository, ILogger<SensorService> logger)
//         {
//             _sensorRepository = sensorRepository;
//             _logger = logger;
//         }

//         public async Task<Result<bool>> SaveSensorDataAsync(SensorData data)
//         {
//             try
//             {
//                 var sensor = new Sensor
//                 {
//                     MachineId = data.machine_id,
//                     Vibration = data.vibration,
//                     Temperature = data.temperature,
//                     Timestamp = ParseTimestampToUtc(data.timestamp)
//                 };

//                 await _sensorRepository.AddAsync(sensor);
//                 await _sensorRepository.SaveChangesAsync();

//                 _logger.LogInformation("Sensor data saved for machine {MachineId}", data.machine_id);

//                 return Result<bool>.Success(true);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error saving sensor data for machine {MachineId}", data.machine_id);
//                 return Result<bool>.Failure(
//                     $"Erro ao salvar dados do sensor: {ex.Message}",
//                     Result<bool>.ResultErrorType.ServiceUnavailable
//                 );
//             }
//         }

//         public async Task<Result<List<SensorResponse>>> GetSensorsAsync(int limit = 100)
//         {
//             try
//             {
//                 var sensors = await _sensorRepository.GetSensorsAsync(limit);

//                 if (sensors == null || !sensors.Any())
//                 {
//                     return Result<List<SensorResponse>>.Failure(
//                         "Nenhum sensor encontrado",
//                         Result<List<SensorResponse>>.ResultErrorType.NoContent
//                     );
//                 }

//                 var response = new List<SensorResponse>();
//                 foreach (var sensor in sensors)
//                 {
//                     response.Add(new SensorResponse(
//                         sensor.MachineId,
//                         sensor.Vibration,
//                         sensor.Temperature,
//                         sensor.Timestamp.ToString("o")
//                     ));
//                 }

//                 return Result<List<SensorResponse>>.Success(response);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error getting sensors with limit {Limit}", limit);
//                 return Result<List<SensorResponse>>.Failure(
//                     $"Erro ao buscar sensores: {ex.Message}",
//                     Result<List<SensorResponse>>.ResultErrorType.ServiceUnavailable
//                 );
//             }
//         }

//         public async Task<Result<List<SensorResponse>>> GetSensorsByMachineAsync(string machineId, int limit = 100)
//         {
//             try
//             {
//                 // var dados = new { Id = "", Nome = "MQ01" };
//                 if (string.IsNullOrWhiteSpace(machineId))
//                 {
//                     return Result<List<SensorResponse>>.Failure(
//                         "ID da máquina é obrigatório",
//                         Result<List<SensorResponse>>.ResultErrorType.BadRequest
//                     );
//                 }

//                 var sensors = await _sensorRepository.GetFilteredSensorsAsync(
//                     s => s.MachineId == machineId);

//                 if (sensors == null || !sensors.Any())
//                 {
//                     return Result<List<SensorResponse>>.Failure(
//                         $"Nenhum sensor encontrado para a máquina {machineId}",
//                         Result<List<SensorResponse>>.ResultErrorType.NotFound
//                     );
//                 }

//                 var response = sensors
//                     .OrderByDescending(s => s.Timestamp)
//                     .Take(limit)
//                     .Select(s => new SensorResponse(
//                         s.MachineId,
//                         s.Vibration,
//                         s.Temperature,
//                         s.Timestamp.ToString("o")))
//                     .ToList();

//                 return Result<List<SensorResponse>>.Success(response);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error getting sensors for machine {MachineId}", machineId);
//                 return Result<List<SensorResponse>>.Failure(
//                         $"Erro ao buscar sensores da máquina {machineId}: {ex.Message}",
//                         Result<List<SensorResponse>>.ResultErrorType.ServiceUnavailable
//                 );
//             }
//         }

//         private static DateTime ParseTimestampToUtc(string timestamp)
//         {
//             if (DateTime.TryParse(timestamp, null, DateTimeStyles.RoundtripKind, out var dateTime))
//             {
//                 return dateTime.Kind == DateTimeKind.Utc
//                         ? dateTime
//                         : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
//             }

//             var parsed = DateTime.Parse(timestamp);
//             return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
//         }
//     }
// }
