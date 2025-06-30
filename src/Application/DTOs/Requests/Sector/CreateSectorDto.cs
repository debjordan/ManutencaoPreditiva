namespace ManutencaoPreditiva.Application.DTOs.Request;

public class CreateSectorDto
{
    public string Name { get; set; } = string.Empty;
    public int CostCenter { get; set; }
}
