// using System.Linq.Expressions;
// using ManutencaoPreditiva.Api.Domain.Entities;
// using ManutencaoPreditiva.Api.Infrastructure.Data;
// using ManutencaoPreditiva.Api.Infrastructure.Repositories.Abstractions;
// using ManutencaoPreditiva.Api.Infrastructure.Repositories.Implementations;
// using Microsoft.EntityFrameworkCore;

// namespace ManutencaoPreditiva.Api.Infrastructure.Repositories.Implementations;

// public class SensorRepository : BaseRepository<Sensor>, ISensorRepository
// {
//     private readonly AppDbContext _context;

//     public SensorRepository(AppDbContext context) : base(context)
//     {
//         _context = context;
//     }

//     public async Task<IEnumerable<Sensor>> GetSensorsAsync(int limit = 100, bool orderByDescending = true)
//     {
//         var query = _context.Set<Sensor>().AsQueryable();

//         if (orderByDescending)
//         {
//             query = query.OrderByDescending(s => s.Timestamp);
//         }
//         else
//         {
//             query = query.OrderBy(s => s.Timestamp);
//         }

//         return await query.Take(limit).ToListAsync();
//     }

//     public async Task<IEnumerable<Sensor>> GetFilteredSensorsAsync(Expression<Func<Sensor, bool>> filter)
//     {
//         return await _context.Set<Sensor>()
//             .Where(filter)
//             .OrderByDescending(s => s.Timestamp)
//             .ToListAsync();
//     }

// }
