// TransportLogistics.Api/Contracts/IGenericRepository.cs
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TransportLogistics.Api.Contracts
{
    public interface IGenericRepository<T, TId> where T : class
    {
        Task<T?> GetByIdAsync(TId id);
        Task<List<T>> GetAllAsync(); // Змінено на List<T>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task SaveChangesAsync();
    }
}