using System;
using System.ComponentModel.DataAnnotations;

namespace TransportLogistics.Api.DTOs
{
    public class UpdateDriverRequest
    {
        [Required]
        public Guid Id { get; set; } // ID драйвера, якого оновлюємо

        [Required]
        [StringLength(50)]
        public string LicenseNumber { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        // Залишаємо IsAvailable на розсуд, чи хочемо ми дозволяти його оновлювати через цей DTO
        // public bool IsAvailable { get; set; } 
    }
}