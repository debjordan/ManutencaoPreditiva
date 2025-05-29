namespace ManutencaoPreditiva.Api.Domain.Entities;
public class Sensor
{
    public int Id { get; set; }
    public required string MachineId { get; set; }
    public double Vibration { get; set; }
    public double Temperature { get; set; }
    public DateTime Timestamp { get; set; }
}
