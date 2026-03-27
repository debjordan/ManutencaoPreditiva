namespace IoTDataApi.Application.DTOs;

public class SensorReadingDto
{
    public string MachineId   { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string MachineType { get; set; } = string.Empty;
    public string Area        { get; set; } = string.Empty;
    public string State       { get; set; } = "normal";
    public double Vibration   { get; set; }
    public double Temperature { get; set; }
    public double Pressure    { get; set; }
    public double Humidity    { get; set; }
    public double Voltage     { get; set; }
    public double Current     { get; set; }
    public double Power       { get; set; }
    public string Timestamp   { get; set; } = string.Empty;
    public string ReceivedAt  { get; set; } = string.Empty;
}
