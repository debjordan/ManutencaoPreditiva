using ManutencaoPreditiva.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ManutencaoPreditiva.Api.Infrastructure.Data;
public class AppDbContext : DbContext
{
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<Production> Production { get; set; }
    public DbSet<Prediction> Predictions { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
