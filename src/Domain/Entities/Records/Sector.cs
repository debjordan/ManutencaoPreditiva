// using ManutencaoPreditiva.Domain.Common;
// using ManutencaoPreditiva.Domain.Enums;

// namespace ManutencaoPreditiva.Domain.Entities;

// public class Sector : BaseEntity
// {
//     public string Name { get; private set; }
//     public int CostCenter { get; private set; }

//     // Construtor privado para EF
//     private Sector() { }

//     public Sector(
//         string name,
//         string costCenter)
//     {
//         Id = Guid.NewGuid();
//         Name = name;
//         SerialNumber = serialNumber;
//         Model = model;
//         Manufacturer = manufacturer;
//         Location = location;
//         InstallationDate = installationDate;
//         Description = description;
//         Status = MachineStatus.Active;
//         IsActive = true;
//         OperatingHours = 0;
//         CreatedAt = DateTime.UtcNow;
//         UpdatedAt = DateTime.UtcNow;
//     }

//     public void UpdateInfo(string name, string model, string manufacturer, string location, string? description)
//     {
//         Name = name;
//         Model = model;
//         Manufacturer = manufacturer;
//         Location = location;
//         Description = description;
//         UpdatedAt = DateTime.UtcNow;
//     }

//     public void ScheduleNextMaintenance(DateTime nextMaintenanceDate)
//     {
//         if (nextMaintenanceDate <= DateTime.UtcNow)
//             throw new ArgumentException("Data de manutenção deve ser futura");

//         NextMaintenanceDate = nextMaintenanceDate;
//         UpdatedAt = DateTime.UtcNow;
//     }

//     public void UpdateMaintenanceDate(DateTime maintenanceDate)
//     {
//         LastMaintenanceDate = maintenanceDate;
//         NextMaintenanceDate = null;
//         Status = MachineStatus.Active;
//         UpdatedAt = DateTime.UtcNow;
//     }

//     public void UpdateOperatingHours(decimal hours)
//     {
//         if (hours < 0)
//             throw new ArgumentException("Horas de operação não podem ser negativas");

//         OperatingHours = hours;
//         UpdatedAt = DateTime.UtcNow;
//     }

//     public void SetStatus(MachineStatus status)
//     {
//         Status = status;
//         UpdatedAt = DateTime.UtcNow;
//     }

//     public void Activate()
//     {
//         IsActive = true;
//         Status = MachineStatus.Active;
//         UpdatedAt = DateTime.UtcNow;
//     }

//     public void Deactivate()
//     {
//         IsActive = false;
//         Status = MachineStatus.Inactive;
//         UpdatedAt = DateTime.UtcNow;
//     }

//     public bool RequiresMaintenance()
//     {
//         return NextMaintenanceDate.HasValue && NextMaintenanceDate.Value <= DateTime.UtcNow;
//     }

//     public int DaysSinceLastMaintenance()
//     {
//         if (!LastMaintenanceDate.HasValue)
//             return (DateTime.UtcNow - InstallationDate).Days;

//         return (DateTime.UtcNow - LastMaintenanceDate.Value).Days;
//     }
// }
