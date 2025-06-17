using System;
using System.ComponentModel.DataAnnotations;

namespace TransportLogistics.Api.DTOs
{
    /// <summary>
    /// DTO для представлення інформації про вантаж у запитах Create/Update Order.
    /// Тепер включає Id для ідентифікації існуючих вантажів під час оновлення.
    /// </summary>
    public class CargoRequestDto
    {
        public Guid? Id { get; set; } // !!! ДОДАНО: Id, дозволяє null для нових вантажів !!!

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