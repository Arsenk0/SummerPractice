// TransportLogistics.Api/Contracts/IDriverService.cs
using TransportLogistics.Api.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TransportLogistics.Api.Contracts
{
    public interface IDriverService
    {
        Task<List<DriverDto>> GetAllDriversAsync(); // Змінено на List<DriverDto>
        Task<DriverDto?> GetDriverByIdAsync(Guid id);
        Task<DriverDto> CreateDriverAsync(CreateDriverRequest request);
        Task<DriverDto?> UpdateDriverAsync(Guid id, UpdateDriverRequest request);
        Task<bool> DeleteDriverAsync(Guid id);
    }
}