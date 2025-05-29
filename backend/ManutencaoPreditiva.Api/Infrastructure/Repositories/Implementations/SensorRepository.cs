using System.Linq.Expressions;
using ManutencaoPreditiva.Api.Domain.Entities;
using ManutencaoPreditiva.Api.Infrastructure.Data;
using ManutencaoPreditiva.Api.Infrastructure.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

public class SensorRepository : BaseRepository<Sensor>, ISensorRepository
{
    private readonly AppDbContext _context;
    public SensorRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task AddAsync(Sensor entity)
    {
        await _context.AddAsync(entity);
    }

    public Task<IEnumerable<Sensor>> GetFilteredSensorsAsync(Expression<Func<Sensor, bool>> filter)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Sensor>> GetSensorsAsync(int limit = 100, bool orderByDescending = true)
    {
        throw new NotImplementedException();
    }
}
