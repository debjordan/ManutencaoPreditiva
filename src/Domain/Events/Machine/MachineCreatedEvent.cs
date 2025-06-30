using ManutencaoPreditiva.Domain.Common;

namespace ManutencaoPreditiva.Domain.Events
{
    public class MachineCreatedEvent : DomainEvent
    {
        public Guid MachineId { get; }
        public string Name { get; }

        public MachineCreatedEvent(Guid machineId, string name)
        {
            MachineId = machineId;
            Name = name;
        }
    }
}
