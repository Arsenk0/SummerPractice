using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Для List<Cargo>

namespace TransportLogistics.Api.Data.Entities
{
    public enum OrderStatus
    {
        Pending,       // Очікує підтвердження
        Scheduled,     // Заплановано
        InProgress,    // В дорозі
        Completed,     // Виконано
        Cancelled      // Скасовано
    }

    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string OriginAddress { get; set; } = string.Empty; // Адреса відправлення

        [Required]
        [StringLength(255)]
        public string DestinationAddress { get; set; } = string.Empty; // Адреса доставки

        public DateTime CreationDate { get; set; } = DateTime.UtcNow; // Дата створення замовлення

        public DateTime? ScheduledPickupDate { get; set; } // Запланована дата завантаження
        public DateTime? ActualPickupDate { get; set; }    // Фактична дата завантаження

        public DateTime? ScheduledDeliveryDate { get; set; } // Запланована дата доставки
        public DateTime? ActualDeliveryDate { get; set; }    // Фактична дата доставки

        [Required]
        public OrderStatus Status { get; set; } // Статус замовлення

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalWeightKg { get; set; } // Загальна вага вантажу

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalVolumeM3 { get; set; } // Загальний об'єм вантажу

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Ціна перевезення

        [StringLength(500)]
        public string? Notes { get; set; } // Додаткові примітки

        // Зв'язки (Foreign Keys)
        public Guid? DriverId { get; set; }
        public Driver? Driver { get; set; } // Навігаційна властивість до Водія

        public Guid? VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; } // Навігаційна властивість до Транспортного засобу

        // Зв'язок "один-до-багатьох" з Cargo.
        public ICollection<Cargo> Cargos { get; set; } = new List<Cargo>();
    }
}