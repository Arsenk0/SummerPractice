// Program.cs

using Microsoft.EntityFrameworkCore;
using TransportLogistics.Api.Data;
using TransportLogistics.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using System.Collections.Generic;
using TransportLogistics.Api.Contracts;
using TransportLogistics.Api.Services;
using TransportLogistics.Api.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using TransportLogistics.Api.Validators;
using Microsoft.OpenApi.Models;
using TransportLogistics.Api.Middleware; // Додано для ExceptionHandlingMiddleware
using Microsoft.AspNetCore.Mvc; // Додано для ApiBehaviorOptions
using FluentValidation.Results; // Додано для FluentValidation.Results.ValidationFailure
using System.Linq; // Явно додано System.Linq для вирішення проблем з Select/SelectMany
using TransportLogistics.Api.Exceptions; // Додано для вашого ValidationException
using TransportLogistics.Api.DataSeeder; // !!! ДОДАНО для сідінгу !!!

var builder = WebApplication.CreateBuilder(args);

// Додаємо послуги до контейнера.

builder.Services.AddControllers();

// === Додаємо Fluent Validation ===
builder.Services.AddFluentValidationAutoValidation(); // Автоматична валідація
builder.Services.AddFluentValidationClientsideAdapters(); // Для клієнтської валідації (якщо використовується)
// Реєструємо всі валідатори з поточної збірки, де знаходяться ваші валідатори
builder.Services.AddValidatorsFromAssemblyContaining<CreateDriverRequestValidator>();

// Налаштовуємо, щоб FluentValidation кидав виняток при валідації
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        // Правильна логіка для перетворення ModelState на список FluentValidation.Results.ValidationFailure
        var failures = context.ModelState
            .Where(e => e.Value != null && e.Value.Errors.Any()) // Перевіряємо, що є помилки
            .SelectMany(e => e.Value.Errors.Select(error => new FluentValidation.Results.ValidationFailure(e.Key, error.ErrorMessage)))
            .ToList();

        throw new TransportLogistics.Api.Exceptions.ValidationException(failures);
    };
});


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
                }
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
builder.Services.AddScoped<IAuthService, AuthService>();

// =======================================================================================
// >>>>> ПОЧАТОК РЕЄСТРАЦІЙ РЕПОЗИТОРІЇВ ТА СЕРВІСІВ <<<<<

// Реєстрація репозиторіїв
builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
builder.Services.AddScoped<IDriverRepository, DriverRepository>();

builder.Services.AddScoped<IGenericRepository<Order, Guid>, GenericRepository<Order, Guid>>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddScoped<IGenericRepository<Vehicle, Guid>, GenericRepository<Vehicle, Guid>>();


// Реєстрація сервісів
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// >>>>> КІНЕЦЬ РЕЄСТРАЦІЙ <<<<<
// =======================================================================================


// === Додаємо налаштування JWT Authentication ===
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// ===============================================

var app = builder.Build();

// Конфігуруємо HTTP-пайплайн запитів.

// Додаємо Middleware для обробки винятків на самому початку
app.UseExceptionHandlingMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// === Додаємо логіку сідінгу бази даних ===
// Цей блок виконується після побудови app, але до його запуску.
// Використовуємо using-блок для створення Scope, щоб правильно отримати сервіси.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // Застосовуємо міграції, якщо є
        context.Database.Migrate();

        // Сідінг базових ролей та адміністратора
        await ApplicationDbContextSeed.SeedEssentialsAsync(userManager, roleManager);

        // Сідінг прикладних даних
        await ApplicationDbContextSeed.SeedSampleDataAsync(context, userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// === Кінець логіки сідінгу ===

app.Run();