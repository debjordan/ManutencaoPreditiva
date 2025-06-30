namespace ManutencaoPreditiva.Domain.Entities;
public class PerformanceOEE
{
    public Guid Id { get; set; }
    public Guid MachineId { get; set; }
    public DateTime Date { get; set; }
    public decimal ResultHour { get; set; }
    public decimal Performance { get; set; }
}
