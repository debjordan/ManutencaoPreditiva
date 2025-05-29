namespace ManutencaoPreditiva.Api.Domain.Entities;
public class Production
{
    public int Id { get; set; }
    public required string MachineId { get; set; }
    public double CycleTime { get; set; }
    public double ManTime { get; set; }
    public double MachineTime { get; set; }
    public double Availability { get; set; }
    public double Performance { get; set; }
    public double Quality { get; set; }
    public DateTime Timestamp { get; set; }
}
