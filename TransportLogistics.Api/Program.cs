using Microsoft.EntityFrameworkCore;
using TransportLogistics.Api.Data;
using TransportLogistics.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Додаємо послуги до контейнера.

builder.Services.AddControllers();
// Додаємо Swagger/OpenAPI для документування API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Налаштування DbContext для PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Налаштування ASP.NET Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        // Налаштування вимог до пароля (можна зробити простішими для тестування)
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.User.RequireUniqueEmail = true; // Вимагаємо унікальну пошту
    })
    .AddEntityFrameworkStores<ApplicationDbContext>() // Вказуємо, що Identity буде використовувати EF Core
    .AddDefaultTokenProviders(); // Додаємо провайдерів токенів для скидання пароля тощо


var app = builder.Build();

// Конфігуруємо HTTP-пайплайн запитів.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Ми вимкнули HTTPS для простоти, але якщо ввімкнете, то це потрібно

app.UseAuthorization(); // Дозволяємо використання авторизації

app.MapControllers();

app.Run();