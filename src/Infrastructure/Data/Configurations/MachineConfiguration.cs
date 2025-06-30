// Infrastructure/Data/Configurations/MachineConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManutencaoPreditiva.Domain.Entities;
using ManutencaoPreditiva.Domain.ValueObjects;

namespace ManutencaoPreditiva.Infrastructure.Data.Configurations
{
    public class MachineConfiguration : IEntityTypeConfiguration<Machine>
    {
        public void Configure(EntityTypeBuilder<Machine> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.OwnsOne(m => m.SerialNumber, sn =>
            {
                sn.Property(s => s.Value)
                    .HasColumnName("SerialNumber")
                    .IsRequired()
                    .HasMaxLength(50);
            });

            builder.Property(m => m.Model)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.Manufacturer)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.Location)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(m => m.InstallationDate)
                .IsRequired();

            builder.Property(m => m.Description)
                .HasMaxLength(500);

            builder.Property(m => m.OperatingHours)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(m => m.Status)
                .IsRequired();

            builder.Property(m => m.IsActive)
                .IsRequired();

            builder.Property(m => m.CreatedAt)
                .IsRequired();

            builder.Property(m => m.UpdatedAt)
                .IsRequired();
        }
    }
}
