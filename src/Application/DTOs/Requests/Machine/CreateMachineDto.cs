using System;
using ManutencaoPreditiva.Domain.ValueObjects;

namespace ManutencaoPreditiva.Application.DTOs.Request
{
    public class CreateMachineDto
    {
        public string Name { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime InstallationDate { get; set; }
        public string? Description { get; set; }
    }
}
