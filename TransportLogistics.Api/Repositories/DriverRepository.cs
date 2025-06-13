using TransportLogistics.Api.Contracts;
using TransportLogistics.Api.Data;
using TransportLogistics.Api.Data.Entities;
using System;

namespace TransportLogistics.Api.Repositories
{
    public class DriverRepository : GenericRepository<Driver, Guid>, IDriverRepository
    {
        public DriverRepository(ApplicationDbContext context) : base(context)
        {
        }

        // Реалізація специфічних методів Driver, якщо вони були додані в інтерфейс.
        // Наприклад:
        // public async Task<Driver?> GetDriverByLicenseNumberAsync(string licenseNumber)
        // {
        //     return await _dbSet.FirstOrDefaultAsync(d => d.LicenseNumber == licenseNumber);
        // }
    }
}