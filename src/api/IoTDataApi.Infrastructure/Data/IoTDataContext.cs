using Microsoft.EntityFrameworkCore;
using IoTDataApi.Domain.Entities;

namespace IoTDataApi.Infrastructure.Data;

public class IoTDataContext : DbContext
{
    public DbSet<IoTData> IoTData { get; set; } = null!;

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
    }
}