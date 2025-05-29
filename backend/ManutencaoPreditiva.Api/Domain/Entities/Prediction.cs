namespace ManutencaoPreditiva.Api.Domain.Entities;
public class Prediction
{
    public int Id { get; set; }
    public required string MachineId { get; set; }
    public double FailureProbability { get; set; }
    public DateTime Timestamp { get; set; }
}
