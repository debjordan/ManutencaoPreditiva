using System;

namespace ManutencaoPreditiva.Domain.Entities;

public class SensorTemperature
{
    public Guid Id { get; set; }
    public Guid MachineId { get; set; }
    public decimal Temperature { get; set; }
    public DateTime Date { get; set; }
}
