using TransportLogistics.Api.Contracts;
using TransportLogistics.Api.Data.Entities;
using TransportLogistics.Api.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // Для UserManager та User
using Microsoft.EntityFrameworkCore; // Для Include

namespace TransportLogistics.Api.Services
{
    public class DriverService : IDriverService
    {
        private readonly IDriverRepository _driverRepository;
        private readonly UserManager<User> _userManager;

        public DriverService(IDriverRepository driverRepository, UserManager<User> userManager)
        {
            _driverRepository = driverRepository;
            _userManager = userManager;
        }

        public async Task<IEnumerable<DriverDto>> GetAllDriversAsync()
        {
            // Включаємо User, щоб отримати FirstName та LastName
            var drivers = await _driverRepository.GetAllAsync();
            var driverDtos = new List<DriverDto>();

            foreach (var driver in drivers)
            {
                var user = await _userManager.FindByIdAsync(driver.UserId.ToString()!);
                driverDtos.Add(new DriverDto
                {
                    Id = driver.Id,
                    LicenseNumber = driver.LicenseNumber,
                    DateOfBirth = driver.DateOfBirth,
                    UserId = driver.UserId ?? Guid.Empty, // Забезпечуємо ненульове значення
                    UserName = user?.UserName ?? "N/A",
                    FirstName = user?.FirstName ?? "N/A",
                    LastName = user?.LastName ?? "N/A"
                });
            }

            return driverDtos;
        }

        public async Task<DriverDto?> GetDriverByIdAsync(Guid id)
        {
            var driver = await _driverRepository.GetByIdAsync(id);
            if (driver == null) return null;

            var user = await _userManager.FindByIdAsync(driver.UserId.ToString()!);

            return new DriverDto
            {
                Id = driver.Id,
                LicenseNumber = driver.LicenseNumber,
                DateOfBirth = driver.DateOfBirth,
                UserId = driver.UserId ?? Guid.Empty,
                UserName = user?.UserName ?? "N/A",
                FirstName = user?.FirstName ?? "N/A",
                LastName = user?.LastName ?? "N/A"
            };
        }

        public async Task<DriverDto> CreateDriverAsync(CreateDriverRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                throw new ArgumentException("User with provided ID does not exist.");
            }

            var newDriver = new Driver
            {
                LicenseNumber = request.LicenseNumber,
                DateOfBirth = request.DateOfBirth,
                UserId = request.UserId,
                FirstName = user.FirstName ?? string.Empty, // Або user.FirstName!
                LastName = user.LastName ?? string.Empty    // Або user.LastName!
            };

            await _driverRepository.AddAsync(newDriver);
            await _driverRepository.SaveChangesAsync();

            return new DriverDto
            {
                Id = newDriver.Id,
                LicenseNumber = newDriver.LicenseNumber,
                DateOfBirth = newDriver.DateOfBirth,
                UserId = newDriver.UserId ?? Guid.Empty,
                UserName = user.UserName ?? "N/A",
                FirstName = user.FirstName ?? "N/A",
                LastName = user.LastName ?? "N/A"
            };
        }

        public async Task<DriverDto?> UpdateDriverAsync(UpdateDriverRequest request)
        {
            var existingDriver = await _driverRepository.GetByIdAsync(request.Id);
            if (existingDriver == null) return null;

            existingDriver.LicenseNumber = request.LicenseNumber;
            existingDriver.DateOfBirth = request.DateOfBirth;
            // existingDriver.IsAvailable = request.IsAvailable; // Якщо ви хочете дозволити оновлювати IsAvailable

            _driverRepository.Update(existingDriver);
            await _driverRepository.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(existingDriver.UserId.ToString()!);

            return new DriverDto
            {
                Id = existingDriver.Id,
                LicenseNumber = existingDriver.LicenseNumber,
                DateOfBirth = existingDriver.DateOfBirth,
                UserId = existingDriver.UserId ?? Guid.Empty,
                UserName = user?.UserName ?? "N/A",
                FirstName = user?.FirstName ?? "N/A",
                LastName = user?.LastName ?? "N/A"
            };
        }

        public async Task<bool> DeleteDriverAsync(Guid id)
        {
            var driver = await _driverRepository.GetByIdAsync(id);
            if (driver == null) return false;

            _driverRepository.Delete(driver);
            await _driverRepository.SaveChangesAsync();
            return true;
        }
    }
}