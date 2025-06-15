// TransportLogistics.Api/Data/Entities/Cargo.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportLogistics.Api.Data.Entities
{
    public class Cargo
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

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

        [Required]
        public Guid OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;
    }
}