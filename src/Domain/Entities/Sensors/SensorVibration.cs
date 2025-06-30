using System;

namespace ManutencaoPreditiva.Domain.Entities;

public class SensorVibration
{
    public Guid Id { get; set; }
    public Guid MachineId { get; set; }
    public decimal Vibration { get; set; }
    public DateTime Date { get; set; }
}
