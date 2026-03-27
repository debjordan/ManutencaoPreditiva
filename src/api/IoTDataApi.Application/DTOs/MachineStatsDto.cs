namespace IoTDataApi.Application.DTOs;

public class MachineStatsDto
{
    public string MachineId    { get; set; } = string.Empty;
    public string MachineName  { get; set; } = string.Empty;
    public string Area         { get; set; } = string.Empty;
    public string CurrentState { get; set; } = "normal";
    public int    RecordCount  { get; set; }
    public double RiskScore    { get; set; }
    public double Oee          { get; set; }
    public string LastSeen     { get; set; } = string.Empty;

    public SensorStats Vibration   { get; set; } = new();
    public SensorStats Temperature { get; set; } = new();
    public SensorStats Pressure    { get; set; } = new();
    public SensorStats Humidity    { get; set; } = new();
    public SensorStats Voltage     { get; set; } = new();
    public SensorStats Current     { get; set; } = new();
    public SensorStats Power       { get; set; } = new();
}

public class SensorStats
{
    public double Min  { get; set; }
    public double Max  { get; set; }
    public double Avg  { get; set; }
    public double Last { get; set; }
}
