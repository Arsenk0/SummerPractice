// TransportLogistics.Api/Services/DriverService.cs
using TransportLogistics.Api.Contracts;
using TransportLogistics.Api.Data.Entities;
using TransportLogistics.Api.DTOs;
using TransportLogistics.Api.DTOs.QueryParams; // Додано для DriverQueryParams
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // Додано для IQueryable розширень

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

        // Оновлено метод для прийому DriverQueryParams
        public async Task<List<DriverDto>> GetAllDriversAsync(DriverQueryParams queryParams)
        {
            // Отримуємо всі драйвери
            var driversQuery = (await _driverRepository.GetAllAsync()).AsQueryable();

            // === Фільтрування ===
            if (!string.IsNullOrWhiteSpace(queryParams.FirstName))
            {
                driversQuery = driversQuery.Where(d => d.FirstName.Contains(queryParams.FirstName));
            }
            if (!string.IsNullOrWhiteSpace(queryParams.LastName))
            {
                driversQuery = driversQuery.Where(d => d.LastName.Contains(queryParams.LastName));
            }
            if (!string.IsNullOrWhiteSpace(queryParams.LicenseNumber))
            {
                driversQuery = driversQuery.Where(d => d.LicenseNumber.Contains(queryParams.LicenseNumber));
            }
            if (queryParams.IsAvailable.HasValue)
            {
                driversQuery = driversQuery.Where(d => d.IsAvailable == queryParams.IsAvailable.Value);
            }

            // === Сортування ===
            if (!string.IsNullOrWhiteSpace(queryParams.SortBy))
            {
                switch (queryParams.SortBy.ToLowerInvariant())
                {
                    case "firstname":
                        driversQuery = queryParams.SortOrder.ToLowerInvariant() == "desc" ?
                            driversQuery.OrderByDescending(d => d.FirstName) :
                            driversQuery.OrderBy(d => d.FirstName);
                        break;
                    case "lastname":
                        driversQuery = queryParams.SortOrder.ToLowerInvariant() == "desc" ?
                            driversQuery.OrderByDescending(d => d.LastName) :
                            driversQuery.OrderBy(d => d.LastName);
                        break;
                    case "licensenumber":
                        driversQuery = queryParams.SortOrder.ToLowerInvariant() == "desc" ?
                            driversQuery.OrderByDescending(d => d.LicenseNumber) :
                            driversQuery.OrderBy(d => d.LicenseNumber);
                        break;
                    case "dateofbirth":
                        driversQuery = queryParams.SortOrder.ToLowerInvariant() == "desc" ?
                            driversQuery.OrderByDescending(d => d.DateOfBirth) :
                            driversQuery.OrderBy(d => d.DateOfBirth);
                        break;
                    case "isavailable":
                        driversQuery = queryParams.SortOrder.ToLowerInvariant() == "desc" ?
                            driversQuery.OrderByDescending(d => d.IsAvailable) :
                            driversQuery.OrderBy(d => d.IsAvailable);
                        break;
                    default:
                        // За замовчуванням сортуємо за FirstName, якщо поле невідоме
                        driversQuery = driversQuery.OrderBy(d => d.FirstName);
                        break;
                }
            }
            else
            {
                // За замовчуванням сортуємо за Id, якщо SortBy не вказано
                driversQuery = driversQuery.OrderBy(d => d.Id);
            }

            // === Пагінація ===
            var pagedDrivers = driversQuery
                .Skip(queryParams.Skip)
                .Take(queryParams.PageSize)
                .ToList(); // Виконуємо запит до БД

            var driverDtos = new List<DriverDto>();
            foreach (var driver in pagedDrivers)
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
                    UserName = user?.UserName,
                    Email = user?.Email
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
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                throw new ArgumentException($"User with ID {request.UserId} not found.");
            }

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
                IsAvailable = true,
                UserId = request.UserId
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
                return null;
            }

            if (request.UserId != driverToUpdate.UserId)
            {
                var newUser = await _userManager.FindByIdAsync(request.UserId.ToString());
                if (newUser == null)
                {
                    throw new ArgumentException($"New User with ID {request.UserId} not found.");
                }
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
            driverToUpdate.UserId = request.UserId;

            await _driverRepository.UpdateAsync(driverToUpdate);

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
                return false;
            }
            await _driverRepository.DeleteAsync(driverToDelete);
            return true;
        }
    }
}
