using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TransportLogistics.Api.Data;
using TransportLogistics.Api.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TransportLogistics.Api.DataSeeder
{
    public static class ApplicationDbContextSeed
    {
        public static async Task SeedEssentialsAsync(UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>("User"));
            }
            if (!await roleManager.RoleExistsAsync("Driver"))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>("Driver"));
            }

            var adminUser = new User
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                EmailConfirmed = true,
                FirstName = "Super",
                LastName = "Admin",
                RefreshToken = Guid.NewGuid().ToString(),
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            };

            if (userManager.Users.All(u => u.UserName != adminUser.UserName))
            {
                var result = await userManager.CreateAsync(adminUser, "AdminPassword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    Console.WriteLine($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        public static async Task SeedSampleDataAsync(ApplicationDbContext context, UserManager<User> userManager)
        {
            if (await context.Drivers.AnyAsync() || await context.Orders.AnyAsync() || await context.Vehicles.AnyAsync())
            {
                Console.WriteLine("Database already contains sample data. Skipping seeding.");
                return;
            }

            var adminUser = await userManager.FindByNameAsync("admin@example.com");
            if (adminUser == null)
            {
                Console.WriteLine("Admin user not found. Please ensure SeedEssentialsAsync was called first and succeeded.");
                return;
            }

            var vehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    Id = Guid.NewGuid(),
                    Make = "Mercedes",
                    Model = "Sprinter",
                    Year = 2020,
                    LicensePlate = "AA1234BC",
                    MaxWeightCapacityKg = 1500.0,
                    MaxVolumeCapacityM3 = 15.0,
                    Type = VehicleType.Van,
                    IsAvailable = true
                },
                new Vehicle
                {
                    Id = Guid.NewGuid(),
                    Make = "Volvo",
                    Model = "FH16",
                    Year = 2018,
                    LicensePlate = "BB5678DE",
                    MaxWeightCapacityKg = 20000.0,
                    MaxVolumeCapacityM3 = 80.0,
                    Type = VehicleType.Truck,
                    IsAvailable = true
                },
                new Vehicle
                {
                    Id = Guid.NewGuid(),
                    Make = "Ford",
                    Model = "Transit",
                    Year = 2022,
                    LicensePlate = "CC9012FG",
                    MaxWeightCapacityKg = 1200.0,
                    MaxVolumeCapacityM3 = 12.0,
                    Type = VehicleType.Van,
                    IsAvailable = true
                }
            };
            await context.Vehicles.AddRangeAsync(vehicles);
            await context.SaveChangesAsync();

            var driver2User = new User
            {
                UserName = "driver2@example.com",
                Email = "driver2@example.com",
                EmailConfirmed = true,
                FirstName = "Олена",
                LastName = "Сидоренко",
                RefreshToken = Guid.NewGuid().ToString(),
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            };
            if (userManager.Users.All(u => u.UserName != driver2User.UserName))
            {
                var result = await userManager.CreateAsync(driver2User, "DriverPassword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(driver2User, "Driver");
                }
                else
                {
                    Console.WriteLine($"Failed to create driver2 user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    driver2User = null;
                }
            }
            else
            {
                driver2User = await userManager.FindByNameAsync(driver2User.UserName);
            }

            var driver1 = new Driver
            {
                Id = Guid.NewGuid(),
                FirstName = "Іван",
                LastName = "Петров",
                LicenseNumber = "DRV001",
                DateOfBirth = new DateTime(1985, 5, 10, 0, 0, 0, DateTimeKind.Utc),
                UserId = adminUser.Id,
                IsAvailable = true
            };

            var driver2 = new Driver
            {
                Id = Guid.NewGuid(),
                FirstName = "Олена",
                LastName = "Сидоренко",
                LicenseNumber = "DRV002",
                DateOfBirth = new DateTime(1992, 11, 20, 0, 0, 0, DateTimeKind.Utc),
                UserId = driver2User?.Id ?? Guid.Empty,
                IsAvailable = true
            };
            await context.Drivers.AddRangeAsync(driver1, driver2);
            await context.SaveChangesAsync();

            var order1 = new Order
            {
                Id = Guid.NewGuid(),
                OriginAddress = "Чернівці, вул. Головна, 1",
                DestinationAddress = "Київ, просп. Перемоги, 10",
                CreationDate = DateTime.UtcNow,
                ScheduledPickupDate = DateTime.UtcNow.AddDays(1),
                TotalWeightKg = 500.0,
                TotalVolumeM3 = 3.0,
                Price = 2500.00m,
                Status = OrderStatus.Pending,
                ScheduledDeliveryDate = DateTime.UtcNow.AddDays(3),
                Notes = "Термінова доставка",
                DriverId = driver1.Id,
                VehicleId = vehicles[0].Id
            };

            var order2 = new Order
            {
                Id = Guid.NewGuid(),
                OriginAddress = "Львів, пл. Ринок, 1",
                DestinationAddress = "Одеса, вул. Дерибасівська, 5",
                CreationDate = DateTime.UtcNow,
                ScheduledPickupDate = DateTime.UtcNow.AddDays(2),
                TotalWeightKg = 15000.0,
                TotalVolumeM3 = 50.0,
                Price = 15000.00m,
                Status = OrderStatus.InTransit,
                ScheduledDeliveryDate = DateTime.UtcNow.AddDays(7),
                Notes = "Збірний вантаж",
                DriverId = driver2.Id,
                VehicleId = vehicles[1].Id
            };
            await context.Orders.AddRangeAsync(order1, order2);

            var cargo1_order1 = new Cargo
            {
                Id = Guid.NewGuid(),
                Name = "Електроніка",
                WeightKg = 200,
                VolumeM3 = 1.5,
                Quantity = 5,
                OrderId = order1.Id
            };
            var cargo2_order1 = new Cargo
            {
                Id = Guid.NewGuid(),
                Name = "Запчастини",
                WeightKg = 300,
                VolumeM3 = 1.5,
                Quantity = 10,
                OrderId = order1.Id
            };
            var cargo1_order2 = new Cargo
            {
                Id = Guid.NewGuid(),
                Name = "Будматеріали",
                WeightKg = 10000,
                VolumeM3 = 30,
                Quantity = 1,
                OrderId = order2.Id
            };
            await context.Cargos.AddRangeAsync(cargo1_order1, cargo2_order1, cargo1_order2);

            await context.SaveChangesAsync();
        }
    }
}