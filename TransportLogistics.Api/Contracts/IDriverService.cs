using TransportLogistics.Api.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TransportLogistics.Api.Contracts
{
    public interface IDriverService
    {
        Task<IEnumerable<DriverDto>> GetAllDriversAsync();
        Task<DriverDto?> GetDriverByIdAsync(Guid id);
        Task<DriverDto> CreateDriverAsync(CreateDriverRequest request);
        Task<DriverDto?> UpdateDriverAsync(UpdateDriverRequest request);
        Task<bool> DeleteDriverAsync(Guid id);
    }
}