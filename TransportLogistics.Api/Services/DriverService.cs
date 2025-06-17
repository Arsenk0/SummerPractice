using TransportLogistics.Api.Contracts;
using TransportLogistics.Api.Data.Entities;
using TransportLogistics.Api.DTOs; // Базовий неймспейс для DTO
using TransportLogistics.Api.DTOs.QueryParams;
using TransportLogistics.Api.Exceptions;
using TransportLogistics.Api.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace TransportLogistics.Api.Services
{
    public class DriverService : IDriverService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IMapper _mapper;

        public DriverService(IUnitOfWork unitOfWork, UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<List<DriverDto>> GetAllDriversAsync(DriverQueryParams queryParams)
        {
            var driversQuery = _unitOfWork.GetRepository<Driver, Guid>()
                                          .AsQueryable();

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

            driversQuery = driversQuery.Include(d => d.User);

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
                    case "username":
                        driversQuery = queryParams.SortOrder.ToLowerInvariant() == "desc" ?
                            driversQuery.OrderByDescending(d => d.User!.UserName) :
                            driversQuery.OrderBy(d => d.User!.UserName);
                        break;
                    case "email":
                        driversQuery = queryParams.SortOrder.ToLowerInvariant() == "desc" ?
                            driversQuery.OrderByDescending(d => d.User!.Email) :
                            driversQuery.OrderBy(d => d.User!.Email);
                        break;
                    default:
                        driversQuery = driversQuery.OrderBy(d => d.FirstName);
                        break;
                }
            }
            else
            {
                driversQuery = driversQuery.OrderBy(d => d.Id);
            }

            var pagedDrivers = await driversQuery
                .Skip(queryParams.Skip)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return _mapper.Map<List<DriverDto>>(pagedDrivers);
        }

        public async Task<DriverDto?> GetDriverByIdAsync(Guid id)
        {
            var driver = await _unitOfWork.GetRepository<Driver, Guid>()
                                          .GetByIdAsync(id, d => d.User);
            if (driver == null)
            {
                throw new NotFoundException($"Driver with ID {id} not found.");
            }
            return _mapper.Map<DriverDto>(driver);
        }

        public async Task<DriverDto> CreateDriverAsync(CreateDriverRequest request)
        {
            // Перевірка унікальності ліцензійного номера
            var existingDriverWithLicense = await _unitOfWork.GetRepository<Driver, Guid>()
                                                  .GetSingleOrDefaultAsync(d => d.LicenseNumber == request.LicenseNumber);
            if (existingDriverWithLicense != null)
            {
                throw new ConflictException($"Driver with license number '{request.LicenseNumber}' already exists.");
            }

            // Перевірка, чи користувач з UserId існує
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                throw new NotFoundException($"User with ID {request.UserId} not found. A driver must be linked to an existing user.");
            }

            // Перевірка, чи користувач вже не прив'язаний до іншого водія
            var existingDriverForUser = await _unitOfWork.GetRepository<Driver, Guid>()
                                                         .GetSingleOrDefaultAsync(d => d.UserId == request.UserId);
            if (existingDriverForUser != null)
            {
                throw new ConflictException($"User with ID {request.UserId} is already associated with another driver.");
            }

            // Створення сутності водія та прив'язка до існуючого користувача
            var driver = _mapper.Map<Driver>(request);
            driver.Id = Guid.NewGuid(); // Генеруємо ID для нового водія
            driver.UserId = request.UserId; // Використовуємо UserId з запиту
            driver.IsAvailable = true; // Встановлюємо початкове значення

            await _unitOfWork.GetRepository<Driver, Guid>().AddAsync(driver);
            await _unitOfWork.CompleteAsync(); // Зберігаємо водія

            // Повертаємо мапований DTO після збереження
            var createdDriverWithUser = await _unitOfWork.GetRepository<Driver, Guid>().GetByIdAsync(driver.Id, d => d.User);
            return _mapper.Map<DriverDto>(createdDriverWithUser);
        }

        public async Task<DriverDto?> UpdateDriverAsync(Guid id, UpdateDriverRequest request)
        {
            if (id != request.Id)
            {
                throw new BadRequestException("ID in URL does not match ID in request body.");
            }

            var driverToUpdate = await _unitOfWork.GetRepository<Driver, Guid>().GetByIdAsync(id);
            if (driverToUpdate == null)
            {
                throw new NotFoundException($"Driver with ID {id} not found.");
            }

            // Перевірка унікальності ліцензійного номера, якщо він змінився
            if (request.LicenseNumber != driverToUpdate.LicenseNumber)
            {
                var existingDriverWithLicense = await _unitOfWork.GetRepository<Driver, Guid>()
                                                                .GetSingleOrDefaultAsync(d => d.LicenseNumber == request.LicenseNumber && d.Id != id);
                if (existingDriverWithLicense != null)
                {
                    throw new ConflictException($"Driver with license number '{request.LicenseNumber}' already exists.");
                }
            }

            // Перевірка, чи новий UserId існує і не прив'язаний до іншого водія
            if (request.UserId != driverToUpdate.UserId)
            {
                var newUser = await _userManager.FindByIdAsync(request.UserId.ToString());
                if (newUser == null)
                {
                    throw new NotFoundException($"New User with ID {request.UserId} not found.");
                }
                var existingDriverForNewUser = await _unitOfWork.GetRepository<Driver, Guid>()
                                                                 .GetSingleOrDefaultAsync(d => d.UserId == request.UserId && d.Id != id);
                if (existingDriverForNewUser != null)
                {
                    throw new ConflictException($"Another driver already exists for New User ID {request.UserId}.");
                }
            }

            // Оновлення полів водія
            _mapper.Map(request, driverToUpdate);
            // Важливо: Email, Password, FirstName, LastName користувача НЕ оновлюються через Driver DTO
            // Якщо ці поля потрібно оновлювати, це має бути окремий API для User.

            _unitOfWork.GetRepository<Driver, Guid>().Update(driverToUpdate);
            await _unitOfWork.CompleteAsync();

            var updatedDriverWithUser = await _unitOfWork.GetRepository<Driver, Guid>().GetByIdAsync(driverToUpdate.Id, d => d.User);
            return _mapper.Map<DriverDto>(updatedDriverWithUser);
        }

        public async Task<bool> DeleteDriverAsync(Guid id)
        {
            var driverToDelete = await _unitOfWork.GetRepository<Driver, Guid>().GetByIdAsync(id);
            if (driverToDelete == null)
            {
                return false;
            }

            var activeOrders = await _unitOfWork.GetRepository<Order, Guid>()
                                                 .GetManyAsync(o => o.DriverId == id &&
                                                                   (o.Status == OrderStatus.Pending ||
                                                                    o.Status == OrderStatus.Assigned ||
                                                                    o.Status == OrderStatus.PickedUp ||
                                                                    o.Status == OrderStatus.InTransit));
            if (activeOrders.Any())
            {
                throw new ConflictException($"Cannot delete driver with ID {id} because there are active orders associated with them.");
            }

            if (driverToDelete.UserId != Guid.Empty)
            {
                var user = await _userManager.FindByIdAsync(driverToDelete.UserId.ToString());
                if (user != null)
                {
                    var deleteUserResult = await _userManager.DeleteAsync(user);
                    if (!deleteUserResult.Succeeded)
                    {
                        throw new BadRequestException($"Failed to delete user associated with driver: {string.Join(", ", deleteUserResult.Errors.Select(e => e.Description))}");
                    }
                }
            }

            _unitOfWork.GetRepository<Driver, Guid>().Delete(driverToDelete);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
