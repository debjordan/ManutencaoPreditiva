// Infrastructure/Data/Repositories/MachineRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ManutencaoPreditiva.Domain.Entities;
using ManutencaoPreditiva.Domain.Interfaces.Repositories;
using ManutencaoPreditiva.Infrastructure.Data.Context;

namespace ManutencaoPreditiva.Infrastructure.Data.Repositories
{
    public class MachineRepository : BaseRepository<Machine>, IMachineRepository
    {
        public MachineRepository(RegistrysContext context) : base(context) { }

        public async Task<Machine?> GetBySerialNumberAsync(string serialNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(m => m.SerialNumber.Value == serialNumber);
        }

        public async Task<IEnumerable<Machine>> GetActiveAsync()
        {
            return await _dbSet.Where(m => m.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Machine>> GetByLocationAsync(string location)
        {
            return await _dbSet.Where(m => m.Location == location).ToListAsync();
        }

        public async Task<IEnumerable<Machine>> GetRequiringMaintenanceAsync()
        {
            return await _dbSet.Where(m => m.NextMaintenanceDate.HasValue && m.NextMaintenanceDate <= DateTime.UtcNow).ToListAsync();
        }

        public async Task<bool> SerialNumberExistsAsync(string serialNumber)
        {
            return await _dbSet.AnyAsync(m => m.SerialNumber.Value == serialNumber);
        }

        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }
    }
}
