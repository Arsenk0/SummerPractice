using System;

namespace TransportLogistics.Api.DTOs
{
    public class DriverDto
    {
        public Guid Id { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool IsAvailable { get; set; }
        public Guid UserId { get; set; } // ID користувача, пов'язаного з водієм
        public string UserName { get; set; } = string.Empty; // Ім'я користувача (email)
        public string FirstName { get; set; } = string.Empty; // Ім'я користувача
        public string LastName { get; set; } = string.Empty; // Прізвище користувача
    }
}