// TransportLogistics.Api/DTOs/CreateOrderRequest.cs
using TransportLogistics.Api.Data.Entities; // Для OrderStatus
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TransportLogistics.Api.DTOs
{
    public class CreateOrderRequest
    {
        [Required]
        [MaxLength(250)]
        public string OriginAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string DestinationAddress { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledPickupDate { get; set; }

        public DateTime? ScheduledDeliveryDate { get; set; }

        [Required]
        public int Status { get; set; } // <-- int для вхідних даних DTO

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

        // Список вантажів, які додаються до замовлення
        public List<CargoRequestDto> Cargo { get; set; } = new List<CargoRequestDto>(); // <-- Назва Cargo (однина) для DTO
    }

    // Вкладений DTO для Cargo у запитах Create/Update
    // Це може бути винесено в окремий файл CargoRequestDto.cs, якщо він використовується в інших DTO.
    public class CargoRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000.0)]
        public double WeightKg { get; set; }

        [Required]
        [Range(0.01, 10000.0)]
        public double VolumeM3 { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}