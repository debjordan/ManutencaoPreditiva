// Domain/Entities/Machine.cs
using ManutencaoPreditiva.Domain.Common;
using ManutencaoPreditiva.Domain.Enums;
using ManutencaoPreditiva.Domain.ValueObjects;
using ManutencaoPreditiva.Domain.Events;

namespace ManutencaoPreditiva.Domain.Entities
{
    public class Machine : BaseEntity
    {
        public string Name { get; private set; }
        public SerialNumber SerialNumber { get; private set; }
        public string Model { get; private set; }
        public string Manufacturer { get; private set; }
        public string Location { get; private set; }
        public DateTime InstallationDate { get; private set; }
        public DateTime? LastMaintenanceDate { get; private set; }
        public DateTime? NextMaintenanceDate { get; private set; }
        public MachineStatus Status { get; private set; }
        public decimal OperatingHours { get; private set; }
        public string? Description { get; private set; }
        public bool IsActive { get; private set; }

        private Machine() { } // Para EF Core

        public Machine(
            string name,
            SerialNumber serialNumber,
            string model,
            string manufacturer,
            string location,
            DateTime installationDate,
            string? description = null)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            SerialNumber = serialNumber ?? throw new ArgumentNullException(nameof(serialNumber));
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Manufacturer = manufacturer ?? throw new ArgumentNullException(nameof(manufacturer));
            Location = location ?? throw new ArgumentNullException(nameof(location));
            InstallationDate = installationDate;
            Description = description;
            Status = MachineStatus.Active;
            IsActive = true;
            OperatingHours = 0;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;

            Validate();
            AddDomainEvent(new MachineCreatedEvent(Id, Name));
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("Name cannot be empty.");
            if (string.IsNullOrWhiteSpace(Model))
                throw new ArgumentException("Model cannot be empty.");
            if (string.IsNullOrWhiteSpace(Manufacturer))
                throw new ArgumentException("Manufacturer cannot be empty.");
            if (string.IsNullOrWhiteSpace(Location))
                throw new ArgumentException("Location cannot be empty.");
            if (InstallationDate > DateTime.UtcNow)
                throw new ArgumentException("Installation date cannot be in the future.");
        }

        public void UpdateInfo(string name, string model, string manufacturer, string location, string? description)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Manufacturer = manufacturer ?? throw new ArgumentNullException(nameof(manufacturer));
            Location = location ?? throw new ArgumentNullException(nameof(location));
            Description = description;
            UpdatedAt = DateTime.UtcNow;
            Validate();
        }

        public void ScheduleNextMaintenance(DateTime nextMaintenanceDate)
        {
            if (nextMaintenanceDate <= DateTime.UtcNow)
                throw new ArgumentException("Data de manutenção deve ser futura");

            NextMaintenanceDate = nextMaintenanceDate;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new MaintenanceScheduledEvent(Id, nextMaintenanceDate));
        }

        public void UpdateMaintenanceDate(DateTime maintenanceDate)
        {
            LastMaintenanceDate = maintenanceDate;
            NextMaintenanceDate = null;
            Status = MachineStatus.Active;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateOperatingHours(decimal hours)
        {
            // if (hours < 0)
            //     throw new ArgumentException("Horas de operação não podem ser negativas");

            OperatingHours = hours;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetStatus(MachineStatus status)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            Status = MachineStatus.Active;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            Status = MachineStatus.Inactive;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool RequiresMaintenance()
        {
            return NextMaintenanceDate.HasValue && NextMaintenanceDate.Value <= DateTime.UtcNow;
        }

        public int DaysSinceLastMaintenance()
        {
            if (!LastMaintenanceDate.HasValue)
                return (DateTime.UtcNow - InstallationDate).Days;

            return (DateTime.UtcNow - LastMaintenanceDate.Value).Days;
        }
    }
}
