namespace IoTDataApi.Application.DTOs;

public class AlertDto
{
    public string MachineId   { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string Area        { get; set; } = string.Empty;
    public string Severity    { get; set; } = string.Empty; // "CRÍTICO" | "ALERTA"
    public string Sensor      { get; set; } = string.Empty;
    public double Value       { get; set; }
    public double Threshold   { get; set; }
    public string Message     { get; set; } = string.Empty;
    public string DetectedAt  { get; set; } = string.Empty;
}
