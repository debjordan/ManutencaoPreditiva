using ManutencaoPreditiva.Domain.Enums;

namespace ManutencaoPreditiva.Application.DTOs.Response;

public class MachineDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime InstallationDate { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public MachineStatus Status { get; set; }
    public decimal OperatingHours { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool RequiresMaintenance { get; set; }
    public int DaysSinceLastMaintenance { get; set; }
}
