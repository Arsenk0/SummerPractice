using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TransportLogistics.Api.Contracts
{
    public interface IGenericRepository<T, TId> where T : class
    {
        Task<T?> GetByIdAsync(TId id, params Expression<Func<T, object>>[] includeProperties);
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetAllAsync(params Expression<Func<T, object>>[] includeProperties);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetSingleOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);
        Task<IEnumerable<T>> GetManyAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);
        
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);

        IQueryable<T> AsQueryable();
    }
}