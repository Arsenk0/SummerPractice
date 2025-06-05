using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Для ICollection<Order>

namespace TransportLogistics.Api.Data.Entities
{
    public class Driver
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(20)]
        public string LicenseNumber { get; set; } = string.Empty; // Номер водійських прав

        public DateTime DateOfBirth { get; set; }

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        // Зв'язок один-до-одного з User (необов'язковий)
        public Guid? UserId { get; set; } // Foreign Key до AspNetUsers
        public User? User { get; set; } // Навігаційна властивість

        // Зв'язок один-до-багатьох з Order (один водій може виконувати багато замовлень)
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}