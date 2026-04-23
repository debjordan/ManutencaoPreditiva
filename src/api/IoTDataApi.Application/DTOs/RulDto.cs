namespace IoTDataApi.Application.DTOs;

public class RulDto
{
    public string MachineId               { get; set; } = string.Empty;
    public string MachineName             { get; set; } = string.Empty;
    public string? LimitingSensor         { get; set; }
    public double? EstimatedHoursToFailure { get; set; }
    public double  TrendSlope             { get; set; }
    public string  Confidence             { get; set; } = "n/a"; // "alta" | "média" | "baixa" | "n/a"
    public double  ConfidenceR2           { get; set; }
    public string  Interpretation         { get; set; } = string.Empty;
}
