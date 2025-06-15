// TransportLogistics.Api/Repositories/OrderRepository.cs
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransportLogistics.Api.Contracts;
using TransportLogistics.Api.Data;
using TransportLogistics.Api.Data.Entities;

namespace TransportLogistics.Api.Repositories
{
    public class OrderRepository : GenericRepository<Order, Guid>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<Order?> GetByIdAsync(Guid id) // Додано '?'
        {
            return await _context.Orders
                .Include(o => o.Driver)
                .Include(o => o.Vehicle)
                .Include(o => o.Cargos) // <-- Використовуємо Cargos (множина) з сутності Order
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public override async Task<List<Order>> GetAllAsync() // Повертає List<Order>
        {
            return await _context.Orders
                .Include(o => o.Driver)
                .Include(o => o.Vehicle)
                .Include(o => o.Cargos) // <-- Використовуємо Cargos (множина) з сутності Order
                .ToListAsync();
        }
    }
}