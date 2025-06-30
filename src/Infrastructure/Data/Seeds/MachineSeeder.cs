using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ManutencaoPreditiva.Domain.Entities;
using ManutencaoPreditiva.Domain.ValueObjects;
using ManutencaoPreditiva.Infrastructure.Data.Context;

namespace ManutencaoPreditiva.Infrastructure.Data.Seeds
{
    public static class MachineSeeder
    {
        public static async Task SeedAsync(RegistrysContext context)
        {
            if (await context.Machines.AnyAsync())
                return;

            var machines = new List<Machine>
            {
                new Machine(
                    "Máquina de Solda Industrial",
                    new SerialNumber("SW-001-2024"),
                    "WeldMaster 3000",
                    "TechWeld Corp",
                    "Setor A - Linha de Produção 1",
                    DateTime.UtcNow.AddMonths(-6),
                    "Máquina de solda automatizada para componentes metálicos"
                ),
                new Machine(
                    "Torno CNC Horizontal",
                    new SerialNumber("CNC-H-002-2024"),
                    "PrecisionTurn Pro",
                    "MachineWorks Ltd",
                    "Setor B - Usinagem",
                    DateTime.UtcNow.AddMonths(-12),
                    "Torno CNC de alta precisão para peças complexas"
                ),
                new Machine(
                    "Fresadora Vertical",
                    new SerialNumber("FV-003-2024"),
                    "MillMaster V5",
                    "Industrial Solutions",
                    "Setor B - Usinagem",
                    DateTime.UtcNow.AddMonths(-8),
                    "Fresadora vertical para acabamento de precisão"
                ),
                new Machine(
                    "Compressor de Ar",
                    new SerialNumber("CA-004-2024"),
                    "AirForce 500",
                    "CompressTech",
                    "Área de Utilidades",
                    DateTime.UtcNow.AddMonths(-18),
                    "Compressor de ar industrial para sistema pneumático geral"
                ),
                new Machine(
                    "Prensa Hidráulica",
                    new SerialNumber("PH-005-2024"),
                    "HydroPress 2000",
                    "HeavyDuty Machines",
                    "Setor C - Conformação",
                    DateTime.UtcNow.AddMonths(-3),
                    "Prensa hidráulica para conformação de chapas metálicas"
                )
            };

            machines[1].ScheduleNextMaintenance(DateTime.UtcNow.AddDays(15));
            machines[3].ScheduleNextMaintenance(DateTime.UtcNow.AddDays(-5));

            await context.Machines.AddRangeAsync(machines);
            await context.SaveChangesAsync();
        }
    }
}
