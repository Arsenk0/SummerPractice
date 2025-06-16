// TransportLogistics.Api/Contracts/IOrderService.cs
using TransportLogistics.Api.DTOs;
using TransportLogistics.Api.DTOs.QueryParams; // Додано для OrderQueryParams
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TransportLogistics.Api.Contracts
{
    public interface IOrderService
    {
        // Змінено сигнатуру: тепер приймає OrderQueryParams
        Task<List<OrderResponse>> GetAllOrdersAsync(OrderQueryParams queryParams);
        Task<OrderResponse?> GetOrderByIdAsync(Guid id);
        Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
        Task<OrderResponse?> UpdateOrderAsync(Guid id, UpdateOrderRequest request);
        Task<bool> DeleteOrderAsync(Guid id);
    }
}