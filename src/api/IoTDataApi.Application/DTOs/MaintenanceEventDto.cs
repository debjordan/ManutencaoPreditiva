namespace IoTDataApi.Application.DTOs;

public class MaintenanceEventDto
{
    public int     Id              { get; set; }
    public string  MachineId      { get; set; } = string.Empty;
    public string  MachineName    { get; set; } = string.Empty;
    public string  Type           { get; set; } = string.Empty;
    public string  StartedAt      { get; set; } = string.Empty;
    public string? ResolvedAt     { get; set; }
    public double? DurationMinutes { get; set; }
    public string? Notes          { get; set; }
    public string? TechnicianName { get; set; }
    public bool    IsOpen         { get; set; }
}

public class CreateMaintenanceEventDto
{
    public string  MachineId      { get; set; } = string.Empty;
    public string  MachineName    { get; set; } = string.Empty;
    public string  Type           { get; set; } = "corrective";
    public string? Notes          { get; set; }
    public string? TechnicianName { get; set; }
}

public class ResolveMaintenanceEventDto
{
    public string? Notes { get; set; }
}
