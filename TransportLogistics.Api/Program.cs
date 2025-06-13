using Microsoft.EntityFrameworkCore;
using TransportLogistics.Api.Data;
using TransportLogistics.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System; // Додано для TimeSpan
using System.Collections.Generic;
using TransportLogistics.Api.Contracts; // Для IJwtTokenService
using TransportLogistics.Api.Services;   // Для JwtTokenService
using TransportLogistics.Api.Repositories; // Для DriverRepository

var builder = WebApplication.CreateBuilder(args);

// Додаємо послуги до контейнера.

builder.Services.AddControllers();
// Додаємо Swagger/OpenAPI для документування API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "TransportLogistics.Api", Version = "v1" });

    // Визначаємо схему безпеки для JWT (Bearer Token)
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Додаємо вимоги безпеки для всіх операцій
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

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

// Реєстрація репозиторію для Driver
builder.Services.AddScoped<IDriverRepository, DriverRepository>();

// Реєстрація сервісу для Driver
builder.Services.AddScoped<IDriverService, DriverService>();

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