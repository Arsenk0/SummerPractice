using TransportLogistics.Api.Data.Entities; // Для Driver
using System;
using System.Threading.Tasks;

namespace TransportLogistics.Api.Contracts
{
    public interface IDriverRepository : IGenericRepository<Driver, Guid>
    {
        // Тут можна додати специфічні для драйвера методи, якщо вони потрібні.
        // Наприклад: Task<Driver?> GetDriverByLicenseNumberAsync(string licenseNumber);
    }
}