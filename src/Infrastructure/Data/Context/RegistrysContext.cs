// Infrastructure/Data/Context/RegistrysContext.cs
using ManutencaoPreditiva.Domain.Common;
using ManutencaoPreditiva.Domain.Entities;
using ManutencaoPreditiva.Domain.Events;
using ManutencaoPreditiva.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ManutencaoPreditiva.Infrastructure.Data.Context
{
    public class RegistrysContext : DbContext
    {
        public RegistrysContext(DbContextOptions<RegistrysContext> options) : base(options)
        {
        }

        public DbSet<Machine> Machines { get; set; }
        // public DbSet<Sector> Sectors { get; set; }
        // public DbSet<Turn> Turn { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar configurações específicas
            modelBuilder.ApplyConfiguration(new MachineConfiguration());

            // Ignorar DomainEvent e suas classes derivadas
            modelBuilder.Ignore<DomainEvent>();
            modelBuilder.Ignore<MachineCreatedEvent>();
            modelBuilder.Ignore<MaintenanceScheduledEvent>();

            // Aplicar configurações de todas as entidades do assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RegistrysContext).Assembly);
        }
    }
}
