// TransportLogistics.Api/Validators/CreateDriverRequestValidator.cs
using FluentValidation;
using TransportLogistics.Api.DTOs;
using System;
// using System.Linq; // Видалено, якщо не використовується

namespace TransportLogistics.Api.Validators
{
    public class CreateDriverRequestValidator : AbstractValidator<CreateDriverRequest>
    {
        public CreateDriverRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.LicenseNumber)
                .NotEmpty().WithMessage("License number is required.")
                .Matches(@"^[A-Z0-9]{5,15}$").WithMessage("License number must be alphanumeric and 5-15 characters long.");

            RuleFor(x => x.DateOfBirth)
                .NotNull().WithMessage("Date of birth is required.")
                // Передаємо значення, а не nullable-тип до BeAValidAge
                .Must(date => BeAValidAge(date)).WithMessage("Driver must be at least 18 years old and not older than 90.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");
        }

        // Змінено сигнатуру: приймає DateTime, а не DateTime?
        private bool BeAValidAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age >= 18 && age <= 90;
        }
    }
}