// TransportLogistics.Api/DTOs/TokenRequest.cs
using System.ComponentModel.DataAnnotations;

namespace TransportLogistics.Api.DTOs
{
    public class TokenRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}