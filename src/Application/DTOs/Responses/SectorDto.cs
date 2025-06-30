using ManutencaoPreditiva.Domain.Enums;

namespace ManutencaoPreditiva.Application.DTOs.Response;

public class SectorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CostCenter { get; set; }
}
