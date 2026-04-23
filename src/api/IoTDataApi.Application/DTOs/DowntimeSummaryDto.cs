namespace IoTDataApi.Application.DTOs;

public class DowntimeSummaryDto
{
    public string MachineId              { get; set; } = string.Empty;
    public string MachineName            { get; set; } = string.Empty;
    public string Area                   { get; set; } = string.Empty;
    public int    TotalReadingsAnalyzed  { get; set; }
    public double PeriodHours            { get; set; }
    public double DowntimeMinutes        { get; set; }
    public double AvailabilityPct        { get; set; }
    public int    FailureEvents          { get; set; }
    public double MttrMinutes            { get; set; } // Mean Time To Repair
    public double MtbfHours             { get; set; } // Mean Time Between Failures
    public double CostPerHour            { get; set; }
    public double EstimatedCostBrl       { get; set; }
}
