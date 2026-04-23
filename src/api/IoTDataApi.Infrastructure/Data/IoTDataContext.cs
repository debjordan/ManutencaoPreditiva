using Microsoft.EntityFrameworkCore;
using IoTDataApi.Domain.Entities;

namespace IoTDataApi.Infrastructure.Data;

public class IoTDataContext : DbContext
{
    public DbSet<IoTData>          IoTData           { get; set; } = null!;
    public DbSet<MaintenanceEvent> MaintenanceEvents { get; set; } = null!;

    public IoTDataContext(DbContextOptions<IoTDataContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IoTData>(entity =>
        {
            entity.ToTable("iot_data");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Topic).HasColumnName("topic").IsRequired();
            entity.Property(e => e.Message).HasColumnName("message").IsRequired();
            entity.Property(e => e.ReceivedAt).HasColumnName("received_at").IsRequired();
        });

        modelBuilder.Entity<MaintenanceEvent>(entity =>
        {
            entity.ToTable("maintenance_events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MachineId).HasColumnName("machine_id").IsRequired();
            entity.Property(e => e.MachineName).HasColumnName("machine_name").IsRequired();
            entity.Property(e => e.Type).HasColumnName("type").IsRequired();
            entity.Property(e => e.StartedAt).HasColumnName("started_at").IsRequired();
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.TechnicianName).HasColumnName("technician_name");
        });
    }
}
