namespace IoTDataApi.Domain.Entities;

public class MaintenanceEvent
{
    public int    Id             { get; set; }
    public string MachineId     { get; set; } = string.Empty;
    public string MachineName   { get; set; } = string.Empty;
    public string Type          { get; set; } = "corrective"; // "corrective" | "preventive"
    public string StartedAt     { get; set; } = string.Empty;
    public string? ResolvedAt   { get; set; }
    public string? Notes        { get; set; }
    public string? TechnicianName { get; set; }
}
