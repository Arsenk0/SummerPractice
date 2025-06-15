// TransportLogistics.Api/Data/Entities/Driver.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportLogistics.Api.Data.Entities
{
    public class Driver
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [MinLength(5)]
        public string LicenseNumber { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "date")]
        public DateTime DateOfBirth { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}