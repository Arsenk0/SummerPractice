using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TransportLogistics.Api.Data.Entities; 

namespace TransportLogistics.Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet для наших сутностей, які ми створимо пізніше
        // public DbSet<Driver> Drivers { get; set; }
        // public DbSet<Vehicle> Vehicles { get; set; }
        // public DbSet<Route> Routes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Тут будуть наші Fluent API конфігурації

            // Закоментуйте цей блок тимчасово, поки не створимо клас Driver
            // modelBuilder.Entity<User>()
            //     .HasOne(u => u.Driver)
            //     .WithOne(d => d.User)
            //     .HasForeignKey<Driver>(d => d.UserId)
            //     .IsRequired(false);
        }
    }
}