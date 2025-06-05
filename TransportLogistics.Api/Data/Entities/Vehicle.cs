using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Для ICollection<Order>

namespace TransportLogistics.Api.Data.Entities
{
    public class Vehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty; // Марка (наприклад, Volvo, Mercedes)

        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty; // Модель (наприклад, FH16, Actros)

        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; } = string.Empty; // Номерний знак

        [Required]
        public int Year { get; set; } // Рік випуску

        [Column(TypeName = "decimal(18,2)")] // Вантажопідйомність (наприклад, в тоннах)
        public decimal LoadCapacity { get; set; }

        [StringLength(50)]
        public string? VehicleType { get; set; } // Тип транспортного засобу (наприклад, "Вантажівка", "Фургон", "Рефрижератор")

        public bool IsAvailable { get; set; } = true; // Чи доступний зараз для замовлень

        // Зв'язок один-до-багатьох з Order (один транспортний засіб може бути у багатьох замовленнях)
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}