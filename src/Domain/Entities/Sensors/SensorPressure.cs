using System;

namespace ManutencaoPreditiva.Domain.Entities;

public class SensorPressure
{
    public Guid Id { get; set; }
    public Guid MachineId { get; set; }
    public decimal Pressure { get; set; }
    public DateTime Date { get; set; }
}
