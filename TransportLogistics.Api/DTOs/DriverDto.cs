// TransportLogistics.Api/DTOs/DriverDto.cs
using System;

namespace TransportLogistics.Api.DTOs
{
    public class DriverDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool IsAvailable { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; } // Nullable, оскільки User може бути не завантажений завжди
        public string? Email { get; set; }    // Nullable, оскільки User може бути не завантажений завжди
    }
}