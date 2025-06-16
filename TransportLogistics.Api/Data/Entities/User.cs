// TransportLogistics.Api/Data/Entities/User.cs
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportLogistics.Api.Data.Entities
{
    // Ваша існуюча сутність User
    public class User : IdentityUser<Guid> // Важливо, щоб було IdentityUser<Guid>
    {
        [MaxLength(100)]
        public string? FirstName { get; set; } // Nullable, якщо не обов'язкове
        [MaxLength(100)]
        public string? LastName { get; set; } // Nullable, якщо не обов'язкове

        // Додаємо поля для Refresh Token
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }

        // Навігаційна властивість для Driver (один-до-одного)
        public Driver? Driver { get; set; }
    }
}