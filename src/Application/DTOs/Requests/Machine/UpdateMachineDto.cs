namespace ManutencaoPreditiva.Application.DTOs.Request;

public class UpdateMachineDto
{
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
}
