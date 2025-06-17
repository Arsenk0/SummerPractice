// TransportLogistics.Api/DTOs/UpdateOrderRequest.cs
using TransportLogistics.Api.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TransportLogistics.Api.DTOs
{
    public class UpdateOrderRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(250)]
        public string OriginAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string DestinationAddress { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledPickupDate { get; set; }

        public DateTime? ActualPickupDate { get; set; }

        public DateTime? ScheduledDeliveryDate { get; set; }

        public DateTime? ActualDeliveryDate { get; set; }

        [Required]
        public int Status { get; set; }

        [Range(0.01, double.MaxValue)]
        public double TotalWeightKg { get; set; }

        [Range(0.01, double.MaxValue)]
        public double TotalVolumeM3 { get; set; }

        [Range(0.01, (double)decimal.MaxValue)]
        public decimal Price { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public Guid? DriverId { get; set; }
        public Guid? VehicleId { get; set; }

        // Тепер посилаємося на CargoRequestDto, який є в окремому файлі
        public List<CargoRequestDto> Cargo { get; set; } = new List<CargoRequestDto>();
    }
}