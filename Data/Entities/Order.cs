// TransportLogistics.Api/Data/Entities/Order.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportLogistics.Api.Data.Entities
{
    public enum OrderStatus
    {
        Pending,        // Очікує на обробку/призначення
        Assigned,       // Призначено водія та транспорт
        PickedUp,       // Вантаж забрано
        InTransit,      // В дорозі
        Delivered,      // Доставлено
        Cancelled,      // Скасовано
        Problem         // Виникла проблема (наприклад, пошкодження, затримка)
    }

    public class Order
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(250)]
        public string OriginAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string DestinationAddress { get; set; } = string.Empty;

        [Required]
        public DateTime CreationDate { get; set; } // Дата створення замовлення

        [Required]
        public DateTime ScheduledPickupDate { get; set; } // Запланована дата забору

        public DateTime? ActualPickupDate { get; set; } // Фактична дата забору (може бути null)

        public DateTime? ScheduledDeliveryDate { get; set; } // Запланована дата доставки (може бути null)

        public DateTime? ActualDeliveryDate { get; set; } // Фактична дата доставки (може бути null)

        [Required]
        public OrderStatus Status { get; set; } // Статус замовлення (використовуємо enum)

        [Required]
        [Range(0.01, 1000000.0)]
        public double TotalWeightKg { get; set; } // Загальна вага вантажу в кг

        [Required]
        [Range(0.01, 100000.0)]
        public double TotalVolumeM3 { get; set; } // Загальний об'єм вантажу в м³

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Використовуємо decimal для грошових значень
        [Range(0.01, 10000000.0)]
        public decimal Price { get; set; } // Ціна замовлення

        [MaxLength(1000)]
        public string? Notes { get; set; } // Додаткові нотатки

        // Foreign Keys
        public Guid? DriverId { get; set; } // Nullable, оскільки водій може бути призначений пізніше
        public Guid? VehicleId { get; set; } // Nullable, оскільки транспорт може бути призначений пізніше

        // Навігаційні властивості (для EF Core)
        [ForeignKey("DriverId")]
        public Driver? Driver { get; set; } // Nullable навігаційна властивість

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; } // Nullable навігаційна властивість

        public ICollection<Cargo> Cargos { get; set; } = new List<Cargo>(); // Колекція вантажів
    }
}