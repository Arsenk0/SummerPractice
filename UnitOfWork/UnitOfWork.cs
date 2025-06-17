using Microsoft.EntityFrameworkCore;
using TransportLogistics.Api.Contracts;
using TransportLogistics.Api.Data;
using TransportLogistics.Api.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Identity; // Додано, якщо ви плануєте керувати Identity через UoW, але поки не використовуємо.
using TransportLogistics.Api.Data.Entities; // Додано для Identity

namespace TransportLogistics.Api.UnitOfWork{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        // Зберігає екземпляри репозиторіїв, щоб переконатися, що повертається один і той же об'єкт
        private readonly Dictionary<Type, object> _repositories;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
        }

        public IGenericRepository<TEntity, TId> GetRepository<TEntity, TId>()
            where TEntity : class
        {
            // Якщо репозиторій для цього типу вже був створений, повертаємо його
            if (_repositories.ContainsKey(typeof(TEntity)))
            {
                return (IGenericRepository<TEntity, TId>)_repositories[typeof(TEntity)];
            }

            // Інакше створюємо новий GenericRepository і зберігаємо його
            var repository = new GenericRepository<TEntity, TId>(_context);
            _repositories.Add(typeof(TEntity), repository);
            return repository;
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
            // GC.SuppressFinalize(this); // Зазвичай не потрібно, якщо Dispose викликається явно
        }
    }
}