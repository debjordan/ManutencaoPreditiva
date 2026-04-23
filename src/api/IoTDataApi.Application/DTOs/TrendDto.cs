namespace IoTDataApi.Application.DTOs;

public class MachineTrendsDto
{
    public string MachineId   { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public List<SensorTrendDto> Sensors { get; set; } = new();
}

public class SensorTrendDto
{
    public string Sensor    { get; set; } = string.Empty;
    public string Direction { get; set; } = "estável"; // "subindo" | "caindo" | "estável"
    public double Delta     { get; set; }
    public double Last5Avg  { get; set; }
    public double Prev5Avg  { get; set; }
}
