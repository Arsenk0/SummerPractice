// TransportLogistics.Api/Contracts/IOrderRepository.cs
using TransportLogistics.Api.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TransportLogistics.Api.Contracts
{
    public interface IOrderRepository : IGenericRepository<Order, Guid>
    {
        // Методи GetOrdersWithDetailsAsync() та GetOrderByIdWithDetailsAsync(Guid) видалено.
        // Їх функціональність тепер реалізована через перевизначення GetAllAsync та GetByIdAsync
        // в OrderRepository.
    }
}