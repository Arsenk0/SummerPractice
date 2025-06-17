// TransportLogistics.Api/Repositories/OrderRepository.cs
using TransportLogistics.Api.Data;
using TransportLogistics.Api.Data.Entities;
using TransportLogistics.Api.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace TransportLogistics.Api.Repositories
{
    // OrderRepository тепер просто розширює GenericRepository
    // Якщо тут були перевизначені методи, які викликали помилку, їх потрібно видалити
    // або змінити їх сигнатуру, щоб вони відповідали IGenericRepository.
    public class OrderRepository : GenericRepository<Order, Guid>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        // Тут можуть бути специфічні методи для Order, які не є частиною IGenericRepository
        // Наприклад:
        // public async Task<IEnumerable<Order>> GetRecentOrdersByDriver(Guid driverId, int count) { ... }
    }
}