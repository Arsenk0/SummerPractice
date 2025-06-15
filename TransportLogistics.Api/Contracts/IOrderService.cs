// TransportLogistics.Api/Contracts/IOrderService.cs
using TransportLogistics.Api.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TransportLogistics.Api.Contracts
{
    public interface IOrderService
    {
        Task<List<OrderResponse>> GetAllOrdersAsync();
        Task<OrderResponse?> GetOrderByIdAsync(Guid id); // Made nullable
        Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
        Task<OrderResponse?> UpdateOrderAsync(Guid id, UpdateOrderRequest request); // Made nullable
        Task<bool> DeleteOrderAsync(Guid id);
    }
}