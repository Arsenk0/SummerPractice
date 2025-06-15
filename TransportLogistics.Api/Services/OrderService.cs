// TransportLogistics.Api/Services/OrderService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransportLogistics.Api.Contracts;
using TransportLogistics.Api.Data.Entities;
using TransportLogistics.Api.DTOs;

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

        public async Task<List<OrderResponse>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(o => MapToOrderResponse(o)).ToList();
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
            // Перевірка на існування водія та транспортного засобу, якщо ID надано
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
                Status = (OrderStatus)request.Status, // <-- Явне приведення int до enum
                TotalWeightKg = request.TotalWeightKg,
                TotalVolumeM3 = request.TotalVolumeM3,
                Price = request.Price,
                Notes = request.Notes,
                CreationDate = DateTime.UtcNow,
                DriverId = request.DriverId,
                VehicleId = request.VehicleId,
                Cargos = request.Cargo.Select(c => new Cargo // Мапимо request.Cargo (з DTO) до order.Cargos (з сутності)
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

            // Перевірка на існування водія та транспортного засобу, якщо ID надано
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

            // Оновлюємо поля
            existingOrder.OriginAddress = request.OriginAddress;
            existingOrder.DestinationAddress = request.DestinationAddress;
            existingOrder.ScheduledPickupDate = request.ScheduledPickupDate;
            existingOrder.ActualPickupDate = request.ActualPickupDate;
            existingOrder.ScheduledDeliveryDate = request.ScheduledDeliveryDate;
            existingOrder.ActualDeliveryDate = request.ActualDeliveryDate;
            existingOrder.Status = (OrderStatus)request.Status; // <-- Явне приведення int до enum
            existingOrder.TotalWeightKg = request.TotalWeightKg;
            existingOrder.TotalVolumeM3 = request.TotalVolumeM3;
            existingOrder.Price = request.Price;
            existingOrder.Notes = request.Notes;
            existingOrder.DriverId = request.DriverId;
            existingOrder.VehicleId = request.VehicleId;

            // Оновлення Cargo - замінюємо колекцію
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

        // Перевірка на null для параметра 'order' тут не потрібна, якщо метод викликається з order!
        // Якщо ж він може бути null, то сигнатура має бути 'Order? order'
        private OrderResponse MapToOrderResponse(Order order)
        {
            // Якщо у методі GetOrderByIdAsync() та CreateOrderAsync() ви вже перевіряєте на null
            // та передаєте сюди гарантовано не-null об'єкт, то 'if (order == null)' не потрібна.
            // Приймаємо, що 'order' тут завжди не-null.
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
                Cargos = order.Cargos?.Select(c => new CargoInOrderDto // <-- Використовуємо order.Cargos (з сутності)
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