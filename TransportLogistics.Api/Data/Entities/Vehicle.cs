// TransportLogistics.Api/Data/Entities/Vehicle.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TransportLogistics.Api.Data.Entities
{
    public enum VehicleType
    {
        Truck,
        Van,
        Trailer,
        Car
    }

    public class Vehicle
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        public int Year { get; set; }

        [Required]
        public VehicleType Type { get; set; }

        [Required]
        [Range(100.0, 50000.0)]
        public double MaxWeightCapacityKg { get; set; }

        [Required]
        [Range(1.0, 500.0)]
        public double MaxVolumeCapacityM3 { get; set; }

        public bool IsAvailable { get; set; } = true;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}