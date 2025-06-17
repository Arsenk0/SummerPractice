//TransportLogistics.Api/DTOs/UserLoginRequest.cs
using System.ComponentModel.DataAnnotations;

namespace TransportLogistics.Api.DTOs
{
    public class UserLoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}