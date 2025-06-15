// TransportLogistics.Api/Services/DriverService.cs
using TransportLogistics.Api.Contracts;
using TransportLogistics.Api.Data.Entities;
using TransportLogistics.Api.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // Для UserManager

namespace TransportLogistics.Api.Services
{
    public class DriverService : IDriverService
    {
        private readonly IDriverRepository _driverRepository;
        private readonly UserManager<User> _userManager; // Для доступу до User

        public DriverService(IDriverRepository driverRepository, UserManager<User> userManager)
        {
            _driverRepository = driverRepository;
            _userManager = userManager;
        }

        public async Task<List<DriverDto>> GetAllDriversAsync()
        {
            var drivers = await _driverRepository.GetAllAsync();
            var driverDtos = new List<DriverDto>();

            foreach (var driver in drivers)
            {
                var user = await _userManager.FindByIdAsync(driver.UserId.ToString());
                driverDtos.Add(new DriverDto
                {
                    Id = driver.Id,
                    FirstName = driver.FirstName,
                    LastName = driver.LastName,
                    LicenseNumber = driver.LicenseNumber,
                    DateOfBirth = driver.DateOfBirth,
                    IsAvailable = driver.IsAvailable,
                    UserId = driver.UserId,
                    UserName = user?.UserName, // ?. для безпечного доступу
                    Email = user?.Email        // ?. для безпечного доступу
                });
            }
            return driverDtos;
        }

        public async Task<DriverDto?> GetDriverByIdAsync(Guid id)
        {
            var driver = await _driverRepository.GetByIdAsync(id);
            if (driver == null)
            {
                return null;
            }

            var user = await _userManager.FindByIdAsync(driver.UserId.ToString());
            return new DriverDto
            {
                Id = driver.Id,
                FirstName = driver.FirstName,
                LastName = driver.LastName,
                LicenseNumber = driver.LicenseNumber,
                DateOfBirth = driver.DateOfBirth,
                IsAvailable = driver.IsAvailable,
                UserId = driver.UserId,
                UserName = user?.UserName,
                Email = user?.Email
            };
        }

        public async Task<DriverDto> CreateDriverAsync(CreateDriverRequest request)
        {
            // Перевіряємо, чи існує користувач з таким UserId
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                throw new ArgumentException($"User with ID {request.UserId} not found.");
            }

            // Перевіряємо, чи вже існує водій для цього користувача
            var existingDriver = (await _driverRepository.FindAsync(d => d.UserId == request.UserId)).FirstOrDefault();
            if (existingDriver != null)
            {
                throw new ArgumentException($"Driver already exists for User ID {request.UserId}.");
            }

            var driver = new Driver
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                LicenseNumber = request.LicenseNumber,
                DateOfBirth = request.DateOfBirth,
                IsAvailable = true, // За замовчуванням доступний
                UserId = request.UserId // Без ??, оскільки UserId вже Guid
            };

            await _driverRepository.AddAsync(driver);

            return new DriverDto
            {
                Id = driver.Id,
                FirstName = driver.FirstName,
                LastName = driver.LastName,
                LicenseNumber = driver.LicenseNumber,
                DateOfBirth = driver.DateOfBirth,
                IsAvailable = driver.IsAvailable,
                UserId = driver.UserId,
                UserName = user.UserName,
                Email = user.Email
            };
        }

        public async Task<DriverDto?> UpdateDriverAsync(Guid id, UpdateDriverRequest request)
        {
            if (id != request.Id)
            {
                throw new ArgumentException("ID in URL does not match ID in request body.");
            }

            var driverToUpdate = await _driverRepository.GetByIdAsync(id);
            if (driverToUpdate == null)
            {
                return null; // Водія не знайдено
            }

            // Перевіряємо, чи існує користувач з таким UserId, якщо він змінюється
            if (request.UserId != driverToUpdate.UserId)
            {
                var newUser = await _userManager.FindByIdAsync(request.UserId.ToString());
                if (newUser == null)
                {
                    throw new ArgumentException($"New User with ID {request.UserId} not found.");
                }
                // Перевіряємо, чи вже є інший водій, призначений на нового користувача
                var existingDriverForNewUser = (await _driverRepository.FindAsync(d => d.UserId == request.UserId && d.Id != id)).FirstOrDefault();
                if (existingDriverForNewUser != null)
                {
                    throw new ArgumentException($"Another driver already exists for New User ID {request.UserId}.");
                }
            }
            
            driverToUpdate.FirstName = request.FirstName;
            driverToUpdate.LastName = request.LastName;
            driverToUpdate.LicenseNumber = request.LicenseNumber;
            driverToUpdate.DateOfBirth = request.DateOfBirth;
            driverToUpdate.IsAvailable = request.IsAvailable;
            driverToUpdate.UserId = request.UserId; // Без ??, оскільки UserId вже Guid

            await _driverRepository.UpdateAsync(driverToUpdate); // Використовуємо UpdateAsync

            var user = await _userManager.FindByIdAsync(driverToUpdate.UserId.ToString());
            return new DriverDto
            {
                Id = driverToUpdate.Id,
                FirstName = driverToUpdate.FirstName,
                LastName = driverToUpdate.LastName,
                LicenseNumber = driverToUpdate.LicenseNumber,
                DateOfBirth = driverToUpdate.DateOfBirth,
                IsAvailable = driverToUpdate.IsAvailable,
                UserId = driverToUpdate.UserId,
                UserName = user?.UserName,
                Email = user?.Email
            };
        }

        public async Task<bool> DeleteDriverAsync(Guid id)
        {
            var driverToDelete = await _driverRepository.GetByIdAsync(id);
            if (driverToDelete == null)
            {
                return false; // Водія не знайдено
            }
            await _driverRepository.DeleteAsync(driverToDelete); // Використовуємо DeleteAsync
            return true;
        }
    }
}