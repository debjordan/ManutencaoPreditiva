using ManutencaoPreditiva.Domain.Common;
using System;

namespace ManutencaoPreditiva.Domain.Events
{
    public class MaintenanceScheduledEvent : DomainEvent
    {
        public Guid MachineId { get; }
        public DateTime MaintenanceDate { get; }

        public MaintenanceScheduledEvent(Guid machineId, DateTime maintenanceDate)
        {
            MachineId = machineId;
            MaintenanceDate = maintenanceDate;
        }
    }
}
