using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransportLogistics.Api.Contracts;
using TransportLogistics.Api.Data.Entities;
using TransportLogistics.Api.DTOs; // Базовий неймспейс для DTO
using TransportLogistics.Api.DTOs.QueryParams;
using TransportLogistics.Api.Exceptions;
using TransportLogistics.Api.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace TransportLogistics.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<OrderResponse>> GetAllOrdersAsync(OrderQueryParams queryParams)
        {
            var ordersQuery = _unitOfWork.GetRepository<Order, Guid>()
                                         .AsQueryable();

            // === Фільтрування ===
            if (!string.IsNullOrWhiteSpace(queryParams.OriginAddress))
            {
                ordersQuery = ordersQuery.Where(o => o.OriginAddress.Contains(queryParams.OriginAddress));
            }
            if (!string.IsNullOrWhiteSpace(queryParams.DestinationAddress))
            {
                ordersQuery = ordersQuery.Where(o => o.DestinationAddress.Contains(queryParams.DestinationAddress));
            }
            if (queryParams.Status.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.Status == queryParams.Status.Value);
            }
            if (queryParams.CreationDateFrom.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreationDate >= queryParams.CreationDateFrom.Value);
            }
            if (queryParams.CreationDateTo.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreationDate <= queryParams.CreationDateTo.Value.AddDays(1)); // Включаємо кінець дня
            }
            if (queryParams.MinPrice.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.Price >= queryParams.MinPrice.Value);
            }
            if (queryParams.MaxPrice.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.Price <= queryParams.MaxPrice.Value);
            }

            ordersQuery = ordersQuery.Include(o => o.Driver)
                                     .Include(o => o.Vehicle)
                                     .Include(o => o.Cargos);


            // === Сортування ===
            if (!string.IsNullOrWhiteSpace(queryParams.SortBy))
            {
                switch (queryParams.SortBy.ToLowerInvariant())
                {
                    case "creationdate":
                        ordersQuery = queryParams.SortOrder.ToLowerInvariant() == "desc" ?
                            ordersQuery.OrderByDescending(o => o.CreationDate) :
                            ordersQuery.OrderBy(o => o.CreationDate);
                        break;
                    case "scheduledpickupdate":
                        ordersQuery = queryParams.SortOrder.ToLowerInvariant() == "desc" ?
                            ordersQuery.OrderByDescending(o => o.ScheduledPickupDate) :
                            ordersQuery.OrderBy(o => o.ScheduledPickupDate);
                        break;
                    case "price":
                        ordersQuery = queryParams.SortOrder.ToLowerInvariant() == "desc" ?
                            ordersQuery.OrderByDescending(o => o.Price) :
                            ordersQuery.OrderBy(o => o.Price);
                        break;
                    case "status":
                        ordersQuery = queryParams.SortOrder.ToLowerInvariant() == "desc" ?
                            ordersQuery.OrderByDescending(o => o.Status) :
                            ordersQuery.OrderBy(o => o.Status);
                        break;
                    case "originaddress":
                        ordersQuery = queryParams.SortOrder.ToLowerInvariant() == "desc" ?
                            ordersQuery.OrderByDescending(o => o.OriginAddress) :
                            ordersQuery.OrderBy(o => o.OriginAddress);
                        break;
                    case "destinationaddress":
                        ordersQuery = queryParams.SortOrder.ToLowerInvariant() == "desc" ?
                            ordersQuery.OrderByDescending(o => o.DestinationAddress) :
                            ordersQuery.OrderBy(o => o.DestinationAddress);
                        break;
                    case "driverfirstname":
                        ordersQuery = queryParams.SortOrder.ToLowerInvariant() == "desc" ?
                            ordersQuery.OrderByDescending(o => o.Driver!.FirstName) :
                            ordersQuery.OrderBy(o => o.Driver!.FirstName);
                        break;
                    case "vehiclelicenseplate":
                        ordersQuery = queryParams.SortOrder.ToLowerInvariant() == "desc" ?
                            ordersQuery.OrderByDescending(o => o.Vehicle!.LicensePlate) :
                            ordersQuery.OrderBy(o => o.Vehicle!.LicensePlate);
                        break;
                    default:
                        ordersQuery = ordersQuery.OrderBy(o => o.CreationDate);
                        break;
                }
            }
            else
            {
                ordersQuery = ordersQuery.OrderBy(o => o.Id);
            }

            // === Пагінація ===
            var pagedOrders = await ordersQuery
                .Skip(queryParams.Skip)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return _mapper.Map<List<OrderResponse>>(pagedOrders);
        }

        public async Task<OrderResponse?> GetOrderByIdAsync(Guid id)
        {
            var order = await _unitOfWork.GetRepository<Order, Guid>()
                                         .GetByIdAsync(id, o => o.Driver!, o => o.Vehicle!, o => o.Cargos);
            if (order == null)
            {
                return null;
            }
            return _mapper.Map<OrderResponse>(order);
        }

        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            // Перевіряємо існування водія
            if (request.DriverId.HasValue)
            {
                var driverExists = await _unitOfWork.GetRepository<Driver, Guid>().GetByIdAsync(request.DriverId.Value);
                if (driverExists == null)
                {
                    throw new NotFoundException($"Driver with ID {request.DriverId.Value} not found.");
                }
            }

            // Перевіряємо існування транспортного засобу та його вантажопідйомність
            Vehicle? vehicle = null;
            if (request.VehicleId.HasValue)
            {
                vehicle = await _unitOfWork.GetRepository<Vehicle, Guid>().GetByIdAsync(request.VehicleId.Value);
                if (vehicle == null)
                {
                    throw new NotFoundException($"Vehicle with ID {request.VehicleId.Value} not found.");
                }
                if (request.TotalWeightKg > vehicle.MaxWeightCapacityKg)
                {
                    throw new BadRequestException($"Vehicle (ID: {vehicle.Id}) cannot carry {request.TotalWeightKg} kg. Max weight capacity: {vehicle.MaxWeightCapacityKg} kg.");
                }
                if (request.TotalVolumeM3 > vehicle.MaxVolumeCapacityM3)
                {
                    throw new BadRequestException($"Vehicle (ID: {vehicle.Id}) cannot carry {request.TotalVolumeM3} m³. Max volume: {vehicle.MaxVolumeCapacityM3} m³.");
                }
            }

            var order = _mapper.Map<Order>(request);
            order.Id = Guid.NewGuid();
            order.CreationDate = DateTime.UtcNow;
            order.Status = OrderStatus.Pending;

            if (request.Cargo != null && request.Cargo.Any())
            {
                order.Cargos = _mapper.Map<ICollection<Cargo>>(request.Cargo);
                foreach (var cargo in order.Cargos)
                {
                    cargo.Id = Guid.NewGuid();
                    cargo.OrderId = order.Id;
                }
            }
            else
            {
                order.Cargos = new List<Cargo>();
            }

            await _unitOfWork.GetRepository<Order, Guid>().AddAsync(order);
            await _unitOfWork.CompleteAsync();

            var createdOrder = await _unitOfWork.GetRepository<Order, Guid>()
                                                 .GetByIdAsync(order.Id, o => o.Driver!, o => o.Vehicle!, o => o.Cargos);

            return _mapper.Map<OrderResponse>(createdOrder);
        }

        public async Task<bool> DeleteOrderAsync(Guid id)
        {
            var order = await _unitOfWork.GetRepository<Order, Guid>().GetByIdAsync(id);
            if (order == null)
            {
                return false;
            }

            if (order.Status == OrderStatus.InTransit || order.Status == OrderStatus.PickedUp)
            {
                throw new ConflictException($"Cannot delete order with ID {id} because it is currently in transit or picked up.");
            }

            _unitOfWork.GetRepository<Order, Guid>().Delete(order);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<OrderResponse?> UpdateOrderAsync(Guid id, UpdateOrderRequest request)
        {
            var orderToUpdate = await _unitOfWork.GetRepository<Order, Guid>()
                                         .GetByIdAsync(id, o => o.Cargos);
            if (orderToUpdate == null)
            {
                return null;
            }

            if (request.DriverId.HasValue && request.DriverId.Value != orderToUpdate.DriverId)
            {
                var driver = await _unitOfWork.GetRepository<Driver, Guid>().GetByIdAsync(request.DriverId.Value);
                if (driver == null)
                {
                    throw new NotFoundException($"Driver with ID {request.DriverId.Value} not found.");
                }
            }

            Vehicle? vehicle = null;
            if (request.VehicleId.HasValue)
            {
                vehicle = await _unitOfWork.GetRepository<Vehicle, Guid>().GetByIdAsync(request.VehicleId.Value);
                if (vehicle == null)
                {
                    throw new NotFoundException($"Vehicle with ID {request.VehicleId.Value} not found.");
                }
                if (request.TotalWeightKg > vehicle.MaxWeightCapacityKg)
                {
                    throw new BadRequestException($"Vehicle (ID: {vehicle.Id}) cannot carry {request.TotalWeightKg} kg. Max capacity: {vehicle.MaxWeightCapacityKg} kg.");
                }
                if (request.TotalVolumeM3 > vehicle.MaxVolumeCapacityM3)
                {
                    throw new BadRequestException($"Vehicle (ID: {vehicle.Id}) cannot carry {request.TotalVolumeM3} m³. Max volume: {vehicle.MaxVolumeCapacityM3} m³.");
                }
            }


            _mapper.Map(request, orderToUpdate);

            if (request.Cargo != null)
            {
                var existingCargos = orderToUpdate.Cargos.ToList();
                var requestCargoIds = request.Cargo.Where(c => c.Id != Guid.Empty).Select(c => c.Id).ToHashSet();

                foreach (var existingCargo in existingCargos)
                {
                    if (!requestCargoIds.Contains(existingCargo.Id))
                    {
                        _unitOfWork.GetRepository<Cargo, Guid>().Delete(existingCargo);
                    }
                }

                foreach (var cargoRequest in request.Cargo)
                {
                    if (cargoRequest.Id == Guid.Empty)
                    {
                        var newCargo = _mapper.Map<Cargo>(cargoRequest);
                        newCargo.Id = Guid.NewGuid();
                        newCargo.OrderId = orderToUpdate.Id;
                        await _unitOfWork.GetRepository<Cargo, Guid>().AddAsync(newCargo);
                    }
                    else
                    {
                        var cargoToUpdate = existingCargos.FirstOrDefault(c => c.Id == cargoRequest.Id);
                        if (cargoToUpdate != null)
                        {
                            _mapper.Map(cargoRequest, cargoToUpdate);
                            _unitOfWork.GetRepository<Cargo, Guid>().Update(cargoToUpdate);
                        }
                        else
                        {
                            throw new BadRequestException($"Cargo with ID {cargoRequest.Id} not found for order {orderToUpdate.Id}.");
                        }
                    }
                }
            } else {
                if (orderToUpdate.Cargos.Any())
                {
                    foreach(var existingCargo in orderToUpdate.Cargos.ToList())
                    {
                        _unitOfWork.GetRepository<Cargo, Guid>().Delete(existingCargo);
                    }
                }
            }

            _unitOfWork.GetRepository<Order, Guid>().Update(orderToUpdate);
            await _unitOfWork.CompleteAsync();

            var updatedOrder = await _unitOfWork.GetRepository<Order, Guid>()
                                                 .GetByIdAsync(orderToUpdate.Id,
                                                               o => o.Driver!,
                                                               o => o.Vehicle!,
                                                               o => o.Cargos);
            return _mapper.Map<OrderResponse>(updatedOrder);
        }
    }
}
