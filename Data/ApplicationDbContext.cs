using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TransportLogistics.Api.Data.Entities;
using System; // Потрібен для Guid
using System.Collections.Generic; // Потрібен для ICollection

namespace TransportLogistics.Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet для наших сутностей
        public DbSet<Driver> Drivers { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Cargo> Cargos { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Налаштування зв'язку один-до-одного між User та Driver
            modelBuilder.Entity<User>()
                .HasOne(u => u.Driver)
                .WithOne(d => d.User)
                .HasForeignKey<Driver>(d => d.UserId)
                .IsRequired(false);

            // Налаштування зв'язків для Order
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Cargos)
                .WithOne(c => c.Order)
                .HasForeignKey(c => c.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Якщо замовлення видаляється, вантажі видаляються також

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Vehicle)
                .WithMany(v => v.Orders)
                .HasForeignKey(o => o.VehicleId)
                .IsRequired(false);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Driver)
                .WithMany(d => d.Orders)
                .HasForeignKey(o => o.DriverId)
                .IsRequired(false);

            // Додаємо індекси та унікальні обмеження (для зручності та продуктивності)
            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate)
                .IsUnique();

            modelBuilder.Entity<Driver>()
                .HasIndex(d => d.LicenseNumber)
                .IsUnique();
        }
    }
}