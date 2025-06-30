// using ManutencaoPreditiva.Api.Application.DTOs;
// using ManutencaoPreditiva.Api.Domain.Entities;
// using ManutencaoPreditiva.Api.Infrastructure.Data;
// using Microsoft.EntityFrameworkCore;
// using System.Globalization;

// namespace ManutencaoPreditiva.Api.Application.Services;

// public class ProductionService
// {
//     private readonly AppDbContext _dbContext;

//     public ProductionService(AppDbContext dbContext)
//     {
//         _dbContext = dbContext;
//     }

//     public async Task SaveProductionDataAsync(ProductionData data)
//     {
//         var production = new Production
//         {
//             MachineId = data.machine_id,
//             CycleTime = data.cycle_time,
//             ManTime = data.man_time,
//             MachineTime = data.machine_time,
//             Availability = data.availability,
//             Performance = data.performance,
//             Quality = data.quality,
//             Timestamp = ParseTimestampToUtc(data.timestamp)
//         };

//         _dbContext.Production.Add(production);
//         await _dbContext.SaveChangesAsync();
//     }

//     public async Task<object> GetMetricsAsync(int limit = 100)
//     {
//         var productionData = await _dbContext.Production
//             .OrderByDescending(p => p.Timestamp)
//             .Take(limit)
//             .ToListAsync();

//         return new
//         {
//             OEE = productionData.Any() ? productionData.Average(p => p.Availability * p.Performance * p.Quality) : 0,
//             MTT = productionData.Any() ? productionData.Average(p => p.ManTime) : 0,
//             AvgCycleTime = productionData.Any() ? productionData.Average(p => p.CycleTime) : 0,
//             AvgManTime = productionData.Any() ? productionData.Average(p => p.ManTime) : 0,
//             AvgMachineTime = productionData.Any() ? productionData.Average(p => p.MachineTime) : 0
//         };
//     }

//     private static DateTime ParseTimestampToUtc(string timestamp)
//     {
//         if (DateTime.TryParse(timestamp, null, DateTimeStyles.RoundtripKind, out var dateTime))
//         {
//             return dateTime.Kind == DateTimeKind.Utc
//                         ? dateTime
//                         : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
//         }

//         var parsed = DateTime.Parse(timestamp);
//         return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
//     }
// }
