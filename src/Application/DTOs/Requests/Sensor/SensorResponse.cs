namespace ManutencaoPreditiva.Application.DTOs;
public record SensorResponse(string MachineId, double Vibration, double Temperature, string Timestamp);
