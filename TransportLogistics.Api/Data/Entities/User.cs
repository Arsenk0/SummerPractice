using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace TransportLogistics.Api.Data.Entities {
    public class User : IdentityUser<Guid>
    {
        // Додаткові властивості користувача
        [StringLength(100)] // Додаємо обмеження довжини для бази даних
        public string? FirstName { get; set; } // Додано
        [StringLength(100)] // Додаємо обмеження довжини для бази даних
        public string? LastName { get; set; } // Додано

        // Навігаційна властивість для зв'язку один-до-одного з Driver
        public Driver? Driver { get; set; }
    }
}