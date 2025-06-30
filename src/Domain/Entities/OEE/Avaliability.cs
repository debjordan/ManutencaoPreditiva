namespace ManutencaoPreditiva.Domain.Entities;
public class AvailabilityOEE
{
    public Guid Id { get; set; }
    public Guid MachineId { get; set; }
    public DateTime InitialHour { get; set; }
    public DateTime FinalHour { get; set; }
    public decimal ResultHour { get; set; }
    public decimal Availability { get; set; }
}
