// TransportLogistics.Api/DTOs/UserRegistrationRequest.cs
using System.ComponentModel.DataAnnotations;

namespace TransportLogistics.Api.DTOs
{
    public class UserRegistrationRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }
    }
}