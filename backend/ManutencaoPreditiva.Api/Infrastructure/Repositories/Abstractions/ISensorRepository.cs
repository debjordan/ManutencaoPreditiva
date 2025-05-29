using System.Linq.Expressions;
using ManutencaoPreditiva.Api.Domain.Entities;
using ManutencaoPreditiva.Api.Infrastructure.Repositories.Abstractions;

public interface ISensorRepository : IBaseRepository<Sensor>
{
    Task<IEnumerable<Sensor>> GetSensorsAsync(int limit = 100, bool orderByDescending = true);
    Task<IEnumerable<Sensor>> GetFilteredSensorsAsync(Expression<Func<Sensor, bool>> filter);
}
