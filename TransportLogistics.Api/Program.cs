// Program.cs

using Microsoft.EntityFrameworkCore;
using TransportLogistics.Api.Data;
using TransportLogistics.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System; // Додано для TimeSpan
using System.Collections.Generic;
using TransportLogistics.Api.Contracts; // Для IJwtTokenService, IOrderRepository, IOrderService, IGenericRepository, IAuthService
using TransportLogistics.Api.Services;   // Для JwtTokenService, OrderService, AuthService
using TransportLogistics.Api.Repositories; // Для DriverRepository, OrderRepository, GenericRepository
using FluentValidation; // Додано для Fluent Validation
using FluentValidation.AspNetCore; // Додано для Fluent Validation ASP.NET Core інтеграції
using TransportLogistics.Api.Validators; // Додано для ваших валідаторів
using Microsoft.OpenApi.Models; // Додано для OpenApiInfo, OpenApiSecurityScheme, ParameterLocation, SecuritySchemeType, OpenApiReference, ReferenceType

var builder = WebApplication.CreateBuilder(args);

// Додаємо послуги до контейнера.

builder.Services.AddControllers();

// === Додаємо Fluent Validation ===
builder.Services.AddFluentValidationAutoValidation(); // Автоматична валідація
builder.Services.AddFluentValidationClientsideAdapters(); // Для клієнтської валідації (якщо використовується)
// Реєструємо всі валідатори з поточної збірки, де знаходяться ваші валідатори
builder.Services.AddValidatorsFromAssemblyContaining<CreateDriverRequestValidator>();

// Додаємо Swagger/OpenAPI для документування API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TransportLogistics.Api", Version = "v1" });

    // Визначаємо схему безпеки для JWT (Bearer Token)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Додаємо вимоги безпеки для всіх операцій
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
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
builder.Services.AddScoped<IAuthService, AuthService>(); // Переконайтеся, що IAuthService та AuthService зареєстровані

// =======================================================================================
// >>>>> ПОЧАТОК РЕЄСТРАЦІЙ РЕПОЗИТОРІЇВ ТА СЕРВІСІВ <<<<<

// Реєстрація репозиторіїв
builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>)); // Додано для загального репозиторію
builder.Services.AddScoped<IDriverRepository, DriverRepository>();

builder.Services.AddScoped<IGenericRepository<Order, Guid>, GenericRepository<Order, Guid>>(); // Це вже було
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddScoped<IGenericRepository<Vehicle, Guid>, GenericRepository<Vehicle, Guid>>();


// Реєстрація сервісів
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IOrderService, OrderService>();
// builder.Services.AddScoped<IVehicleService, VehicleService>(); // Додайте, якщо такий сервіс існує

// >>>>> КІНЕЦЬ РЕЄСТРАЦІЙ <<<<<
// =======================================================================================


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

//app.UseHttpsRedirection(); // Залишимо, якщо плануєте використовувати HTTPS

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
