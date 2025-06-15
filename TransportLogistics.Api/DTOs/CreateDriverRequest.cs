// TransportLogistics.Api/DTOs/CreateDriverRequest.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace TransportLogistics.Api.DTOs
{
    public class CreateDriverRequest
    {
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

        [Required]
        public Guid UserId { get; set; } // ID користувача, до якого прив'язаний водій
    }
}