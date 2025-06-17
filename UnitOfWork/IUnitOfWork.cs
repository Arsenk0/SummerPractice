using System;
using System.Threading.Tasks;
using TransportLogistics.Api.Contracts; // Для IGenericRepository
using TransportLogistics.Api.Data.Entities; // Для доступу до сутностей
using Microsoft.AspNetCore.Identity; // Для UserManager/RoleManager, якщо вони будуть частиною UoW

namespace TransportLogistics.Api.UnitOfWork{
    public interface IUnitOfWork : IDisposable
    {
        // Метод для отримання універсального репозиторію
        IGenericRepository<TEntity, TId> GetRepository<TEntity, TId>()
            where TEntity : class;

        // Метод для збереження всіх змін у поточній "одиниці роботи"
        Task<int> CompleteAsync();
        int Complete(); // Синхронна версія (менш рекомендовано для веб-API, але залишаємо для повноти)
    }
}