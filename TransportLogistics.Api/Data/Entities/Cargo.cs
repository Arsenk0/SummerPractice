using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportLogistics.Api.Data.Entities
{
    public class Cargo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty; // Назва вантажу (наприклад, "Палети з цеглою", "Меблі")

        [StringLength(500)]
        public string? Description { get; set; } // Детальний опис вантажу

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal WeightKg { get; set; } // Вага вантажу в кг

        [Column(TypeName = "decimal(18,2)")]
        public decimal? VolumeM3 { get; set; } // Об'єм вантажу в м³

        [StringLength(50)]
        public string? Dimensions { get; set; } // Розміри (наприклад, "120x80x100 см")

        public bool IsFragile { get; set; } // Чи є крихким
        public bool IsHazardous { get; set; } // Чи є небезпечним

        // Foreign Key до Order
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!; // Навігаційна властивість до Замовлення
    }
}