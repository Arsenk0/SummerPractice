// TransportLogistics.Api/DTOs/OrderResponse.cs
using System;
using System.Collections.Generic;

namespace TransportLogistics.Api.DTOs
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public string OriginAddress { get; set; } = string.Empty;
        public string DestinationAddress { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public DateTime ScheduledPickupDate { get; set; }
        public DateTime? ActualPickupDate { get; set; } // Nullable
        public DateTime? ScheduledDeliveryDate { get; set; } // <-- Змінено на Nullable DateTime?
        public DateTime? ActualDeliveryDate { get; set; } // Nullable
        public int Status { get; set; }
        public double TotalWeightKg { get; set; }
        public double TotalVolumeM3 { get; set; }
        public decimal Price { get; set; }
        public string? Notes { get; set; }

        public DriverInOrderDto? Driver { get; set; }
        public VehicleInOrderDto? Vehicle { get; set; }
        public List<CargoInOrderDto> Cargos { get; set; } = new List<CargoInOrderDto>();
    }

    public class DriverInOrderDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
    }

    public class VehicleInOrderDto
    {
        public Guid Id { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
    }

    public class CargoInOrderDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double WeightKg { get; set; }
        public double VolumeM3 { get; set; }
        public int Quantity { get; set; }
    }
}