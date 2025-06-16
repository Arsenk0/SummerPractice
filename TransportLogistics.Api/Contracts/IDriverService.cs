// TransportLogistics.Api/Contracts/IDriverService.cs
using TransportLogistics.Api.DTOs;
using TransportLogistics.Api.DTOs.QueryParams; // Додано для DriverQueryParams
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TransportLogistics.Api.Contracts
{
    public interface IDriverService
    {
        // Змінено сигнатуру: тепер приймає DriverQueryParams
        Task<List<DriverDto>> GetAllDriversAsync(DriverQueryParams queryParams);
        Task<DriverDto?> GetDriverByIdAsync(Guid id);
        Task<DriverDto> CreateDriverAsync(CreateDriverRequest request);
        Task<DriverDto?> UpdateDriverAsync(Guid id, UpdateDriverRequest request);
        Task<bool> DeleteDriverAsync(Guid id);
    }
}