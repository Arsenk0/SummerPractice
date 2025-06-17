// TransportLogistics.Api/DTOs/UpdateDriverRequest.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace TransportLogistics.Api.DTOs
{
    public class UpdateDriverRequest
    {
        [Required]
        public Guid Id { get; set; }

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
        public DateTime DateOfBirth { get; set; }

        public bool IsAvailable { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }
}