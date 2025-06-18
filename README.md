# 🚚 TransportLogistics.Api

## 📌 Опис

`TransportLogistics.Api` — це RESTful веб API для управління логістичними процесами: водіями, замовленнями та користувачами. Реалізований на базі .NET 9, з використанням PostgreSQL, JWT аутентифікації та архітектурного підходу Clean Architecture (розділення на DAL, BLL, API).

## 🛠 Стек технологій

- **.NET 9 Web API**
- **Entity Framework Core (PostgreSQL)**
- **ASP.NET Identity**
- **JWT Authentication**
- **AutoMapper**
- **FluentValidation**
- **Swagger / OpenAPI**
- **Clean Architecture**

## 📁 Структура проєкту

├── Contracts/ # Інтерфейси
├── Controllers/ # REST-контролери
├── DTOs/ # DTO моделі
├── Data/ # EF Core DbContext + Сутності
├── Middleware/ # Обробка помилок
├── Repositories/ # Реалізація репозиторіїв
├── Services/ # Бізнес логіка
├── Validators/ # FluentValidation валідація
├── UnitOfWork/ # UoW + репозиторії
├── Profiles/ # AutoMapper профілі
├── Migrations/ # EF Core міграції
├── Program.cs # Конфігурація застосунку
├── appsettings.json # Налаштування підключення, JWT


## 🚀 Запуск проєкту

1. **Налаштувати БД (PostgreSQL)** — в `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=logistics;Username=postgres;Password=yourpassword"
}

    Запустити міграції:

dotnet ef database update

    Запустити додаток:

dotnet run

    Відкрити Swagger UI:

https://localhost:5001/swagger

🔐 Аутентифікація

JWT токен видається через:

POST /api/auth/login

Приклад:

{
  "email": "admin@example.com",
  "password": "password123"
}

Відповідь:

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600
}
