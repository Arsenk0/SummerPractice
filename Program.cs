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
using Microsoft.AspNetCore.Mvc;
using FluentValidation.Results;
using System.Linq;
using TransportLogistics.Api.Exceptions;
using TransportLogistics.Api.DataSeeder;
using TransportLogistics.Api.Middleware;
using TransportLogistics.Api.UnitOfWork; // !!! ПЕРЕКОНАЙТЕСЬ, ЩО ЦЯ ДИРЕКТИВА ІСНУЄ ТА ПРАВИЛЬНА !!!
using AutoMapper;
using TransportLogistics.Api.Profiles;


var builder = WebApplication.CreateBuilder(args);

// Додаємо послуги до контейнера.
builder.Services.AddControllers();

// === Додаємо Fluent Validation ===
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateDriverRequestValidator>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var failures = context.ModelState
            .Where(e => e.Value != null && e.Value.Errors.Any())
            .SelectMany(e => e.Value!.Errors.Select(error => new FluentValidation.Results.ValidationFailure(e.Key, error.ErrorMessage)))
            .ToList();

        throw new TransportLogistics.Api.Exceptions.ValidationException(failures);
    };
});

// === Додаємо AutoMapper ===
builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);
// ==========================

// Додаємо Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TransportLogistics.Api", Version = "v1" });
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
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Реєстрація наших JWT та Auth сервісів
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// =======================================================================================
// >>>>> ПОЧАТОК РЕЄСТРАЦІЙ РЕПОЗИТОРІЇВ ТА СЕРВІСІВ <<<<<

// РЕЄСТРАЦІЯ UNIT OF WORK (ВИПРАВЛЕНО РЯДОК)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); // <<< --- ПЕРЕКОНАЙТЕСЬ, ЩО ТУТ ПРОСТО 'UnitOfWork'

// Реєстрація GenericRepository повинна залишатися
builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

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
app.UseExceptionHandlingMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// === Додаємо логіку сідінгу бази даних ===
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        context.Database.Migrate();

        await ApplicationDbContextSeed.SeedEssentialsAsync(userManager, roleManager);
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
