namespace ManutencaoPreditiva.Domain.Entities;
public class QualityOEE
{
    public Guid Id { get; set; }
    public Guid MachineId { get; set; }
    public DateTime Date { get; set; }
    public decimal ResultHour { get; set; }
    public decimal Quality { get; set; }
}
