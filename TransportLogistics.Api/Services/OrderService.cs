// TransportLogistics.Api/Services/OrderService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransportLogistics.Api.Contracts;
using TransportLogistics.Api.Data.Entities;
using TransportLogistics.Api.DTOs;
using TransportLogistics.Api.DTOs.QueryParams; // Додано для OrderQueryParams
using Microsoft.EntityFrameworkCore; // Додано для IQueryable розширень

namespace TransportLogistics.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IGenericRepository<Vehicle, Guid> _vehicleRepository;

        public OrderService(IOrderRepository orderRepository, IDriverRepository driverRepository, IGenericRepository<Vehicle, Guid> vehicleRepository)
        {
            _orderRepository = orderRepository;
            _driverRepository = driverRepository;
            _vehicleRepository = vehicleRepository;
        }

        // Оновлено метод для прийому OrderQueryParams
        public async Task<List<OrderResponse>> GetAllOrdersAsync(OrderQueryParams queryParams)
        {
            // Отримуємо всі замовлення з включеними пов'язаними сутностями
            var ordersQuery = (await _orderRepository.GetAllAsync()).AsQueryable();

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
                ordersQuery = ordersQuery.Where(o => o.CreationDate <= queryParams.CreationDateTo.Value);
            }
            if (queryParams.MinPrice.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.Price >= queryParams.MinPrice.Value);
            }
            if (queryParams.MaxPrice.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.Price <= queryParams.MaxPrice.Value);
            }

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
                    default:
                        // За замовчуванням сортуємо за CreationDate, якщо поле невідоме
                        ordersQuery = ordersQuery.OrderBy(o => o.CreationDate);
                        break;
                }
            }
            else
            {
                // За замовчуванням сортуємо за Id, якщо SortBy не вказано
                ordersQuery = ordersQuery.OrderBy(o => o.Id);
            }

            // === Пагінація ===
            var pagedOrders = ordersQuery
                .Skip(queryParams.Skip)
                .Take(queryParams.PageSize)
                .ToList(); // Виконуємо запит до БД

            return pagedOrders.Select(o => MapToOrderResponse(o)).ToList();
        }

        public async Task<OrderResponse?> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return null;
            }
            return MapToOrderResponse(order);
        }

        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            if (request.DriverId.HasValue)
            {
                var driverExists = await _driverRepository.GetByIdAsync(request.DriverId.Value);
                if (driverExists == null)
                {
                    throw new ArgumentException($"Driver with ID {request.DriverId.Value} not found.");
                }
            }

            if (request.VehicleId.HasValue)
            {
                var vehicleExists = await _vehicleRepository.GetByIdAsync(request.VehicleId.Value);
                if (vehicleExists == null)
                {
                    throw new ArgumentException($"Vehicle with ID {request.VehicleId.Value} not found.");
                }
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OriginAddress = request.OriginAddress,
                DestinationAddress = request.DestinationAddress,
                ScheduledPickupDate = request.ScheduledPickupDate,
                ScheduledDeliveryDate = request.ScheduledDeliveryDate,
                Status = (OrderStatus)request.Status,
                TotalWeightKg = request.TotalWeightKg,
                TotalVolumeM3 = request.TotalVolumeM3,
                Price = request.Price,
                Notes = request.Notes,
                CreationDate = DateTime.UtcNow,
                DriverId = request.DriverId,
                VehicleId = request.VehicleId,
                Cargos = request.Cargo.Select(c => new Cargo
                {
                    Id = Guid.NewGuid(),
                    Name = c.Name,
                    WeightKg = c.WeightKg,
                    VolumeM3 = c.VolumeM3,
                    Quantity = c.Quantity
                }).ToList()
            };

            await _orderRepository.AddAsync(order);

            var createdOrderWithDetails = await _orderRepository.GetByIdAsync(order.Id);
            return MapToOrderResponse(createdOrderWithDetails!);
        }

        public async Task<OrderResponse?> UpdateOrderAsync(Guid id, UpdateOrderRequest request)
        {
            var existingOrder = await _orderRepository.GetByIdAsync(id);
            if (existingOrder == null)
            {
                return null;
            }

            if (request.DriverId.HasValue)
            {
                var driverExists = await _driverRepository.GetByIdAsync(request.DriverId.Value);
                if (driverExists == null)
                {
                    throw new ArgumentException($"Driver with ID {request.DriverId.Value} not found.");
                }
            }

            if (request.VehicleId.HasValue)
            {
                var vehicleExists = await _vehicleRepository.GetByIdAsync(request.VehicleId.Value);
                if (vehicleExists == null)
                {
                    throw new ArgumentException($"Vehicle with ID {request.VehicleId.Value} not found.");
                }
            }

            existingOrder.OriginAddress = request.OriginAddress;
            existingOrder.DestinationAddress = request.DestinationAddress;
            existingOrder.ScheduledPickupDate = request.ScheduledPickupDate;
            existingOrder.ActualPickupDate = request.ActualPickupDate;
            existingOrder.ScheduledDeliveryDate = request.ScheduledDeliveryDate;
            existingOrder.ActualDeliveryDate = request.ActualDeliveryDate;
            existingOrder.Status = (OrderStatus)request.Status;
            existingOrder.TotalWeightKg = request.TotalWeightKg;
            existingOrder.TotalVolumeM3 = request.TotalVolumeM3;
            existingOrder.Price = request.Price;
            existingOrder.Notes = request.Notes;
            existingOrder.DriverId = request.DriverId;
            existingOrder.VehicleId = request.VehicleId;

            existingOrder.Cargos.Clear();
            foreach (var cargoDto in request.Cargo)
            {
                existingOrder.Cargos.Add(new Cargo
                {
                    Id = Guid.NewGuid(),
                    Name = cargoDto.Name,
                    WeightKg = cargoDto.WeightKg,
                    VolumeM3 = cargoDto.VolumeM3,
                    Quantity = cargoDto.Quantity
                });
            }

            await _orderRepository.UpdateAsync(existingOrder);

            var updatedOrderWithDetails = await _orderRepository.GetByIdAsync(existingOrder.Id);
            return MapToOrderResponse(updatedOrderWithDetails!);
        }

        public async Task<bool> DeleteOrderAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return false;
            }
            await _orderRepository.DeleteAsync(order);
            return true;
        }

        private OrderResponse MapToOrderResponse(Order order)
        {
            if (order == null) return null!; // Додано null! для відповідності сигнатурі

            return new OrderResponse
            {
                Id = order.Id,
                OriginAddress = order.OriginAddress,
                DestinationAddress = order.DestinationAddress,
                ScheduledPickupDate = order.ScheduledPickupDate,
                ScheduledDeliveryDate = order.ScheduledDeliveryDate,
                ActualPickupDate = order.ActualPickupDate,
                ActualDeliveryDate = order.ActualDeliveryDate,
                Status = (int)order.Status,
                TotalWeightKg = order.TotalWeightKg,
                TotalVolumeM3 = order.TotalVolumeM3,
                Price = order.Price,
                Notes = order.Notes,
                CreationDate = order.CreationDate,
                Driver = order.Driver != null ? new DriverInOrderDto
                {
                    Id = order.Driver.Id,
                    FirstName = order.Driver.FirstName,
                    LastName = order.Driver.LastName,
                    LicenseNumber = order.Driver.LicenseNumber
                } : null,
                Vehicle = order.Vehicle != null ? new VehicleInOrderDto
                {
                    Id = order.Vehicle.Id,
                    Make = order.Vehicle.Make,
                    Model = order.Vehicle.Model,
                    LicensePlate = order.Vehicle.LicensePlate
                } : null,
                Cargos = order.Cargos?.Select(c => new CargoInOrderDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    WeightKg = c.WeightKg,
                    VolumeM3 = c.VolumeM3,
                    Quantity = c.Quantity
                }).ToList() ?? new List<CargoInOrderDto>()
            };
        }
    }
}
