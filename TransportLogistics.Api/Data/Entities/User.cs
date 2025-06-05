using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace TransportLogistics.Api.Data.Entities
{
    public class User : IdentityUser<Guid> // Вказуємо, що ID буде Guid
    {
        // Навігаційна властивість для зв'язку один-до-одного з Driver
        //public Driver? Driver { get; set; } // Цей клас Driver ми створимо пізніше
    }
}