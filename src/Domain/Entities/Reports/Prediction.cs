namespace ManutencaoPreditiva.Domain.Entities;
public class Prediction
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public Guid MachineId { get; set; }
    public double FailureProbability { get; set; }
}
