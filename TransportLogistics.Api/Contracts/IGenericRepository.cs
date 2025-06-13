using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TransportLogistics.Api.Contracts
{
    // T - тип сутності (наприклад, Driver, Vehicle), TId - тип ідентифікатора
    public interface IGenericRepository<T, TId> where T : class
    {
        Task<T?> GetByIdAsync(TId id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<int> SaveChangesAsync(); // Зберігає всі зміни в контексті
    }
}