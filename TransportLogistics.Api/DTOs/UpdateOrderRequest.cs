// TransportLogistics.Api/DTOs/UpdateOrderRequest.cs
using TransportLogistics.Api.Data.Entities; // Для OrderStatus
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TransportLogistics.Api.DTOs
{
    public class UpdateOrderRequest
    {
        [Required]
        public Guid Id { get; set; } // ID замовлення, яке оновлюється

        [Required]
        [MaxLength(250)]
        public string OriginAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string DestinationAddress { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledPickupDate { get; set; }

        public DateTime? ActualPickupDate { get; set; } // Nullable

        public DateTime? ScheduledDeliveryDate { get; set; }

        public DateTime? ActualDeliveryDate { get; set; } // Nullable

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

        // Список вантажів для оновлення
        public List<CargoRequestDto> Cargo { get; set; } = new List<CargoRequestDto>(); // <-- Назва Cargo (однина) для DTO
    }
    // Клас CargoRequestDto може бути тут або винесений в окремий файл, якщо він використовується в багатьох DTO.
    // Якщо ви вже створили окремий файл CargoRequestDto.cs, то цей вкладений клас тут не потрібен.
}