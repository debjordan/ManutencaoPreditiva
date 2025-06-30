namespace ManutencaoPreditiva.Domain.Entities;
public class Production
{
    public Guid Id { get; set; }
    public Guid MachineId { get; set; }
    public DateTime Date { get; set; }
    public decimal Availability { get; set; }
    public decimal Performance { get; set; }
    public decimal Quality { get; set; }
}
