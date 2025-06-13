using Microsoft.EntityFrameworkCore;
using TransportLogistics.Api.Data;
using TransportLogistics.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System; // Додано для TimeSpan
using TransportLogistics.Api.Contracts; // Для IJwtTokenService
using TransportLogistics.Api.Services;   // Для JwtTokenService

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
// Реєстрація нашого JWT сервісу
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// === Додаємо налаштування JWT Authentication ===
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!); // Отримання секретного ключа з appsettings.json

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // У продакшені має бути true (для HTTPS)
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true, // Валідувати емітента
        ValidIssuer = jwtSettings["Issuer"], // Емітент з appsettings.json
        ValidateAudience = true, // Валідувати аудиторію
        ValidAudience = jwtSettings["Audience"], // Аудиторія з appsettings.json
        ValidateLifetime = true, // Валідувати термін дії токену
        ClockSkew = TimeSpan.Zero // Відключити похибку годинника (токен має бути дійсним рівно до вказаного часу)
    };
});

// ===============================================

var app = builder.Build();

// Конфігуруємо HTTP-пайплайн запитів.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Ми вимкнули HTTPS для простоти, але якщо ввімкнете, то це потрібно

app.UseRouting(); // Цей рядок завжди має бути перед UseAuthentication та UseAuthorization

app.UseAuthentication(); // !!! ВАЖЛИВО: цей рядок має бути перед UseAuthorization !!!
app.UseAuthorization();  // Дозволяємо використання авторизації

app.MapControllers();

app.Run();