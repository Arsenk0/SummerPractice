// TransportLogistics.Api/Validators/UpdateOrderRequestValidator.cs
using FluentValidation;
using TransportLogistics.Api.DTOs; // Додано для CargoRequestDto
using TransportLogistics.Api.Data.Entities;
using System;
using System.Linq;

namespace TransportLogistics.Api.Validators
{
    public class UpdateOrderRequestValidator : AbstractValidator<UpdateOrderRequest>
    {
        public UpdateOrderRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID is required.");

            RuleFor(x => x.OriginAddress)
                .NotEmpty().WithMessage("Origin address is required.")
                .MaximumLength(200).WithMessage("Origin address cannot exceed 200 characters.");

            RuleFor(x => x.DestinationAddress)
                .NotEmpty().WithMessage("Destination address is required.")
                .MaximumLength(200).WithMessage("Destination address cannot exceed 200 characters.");

            RuleFor(x => x.ScheduledPickupDate)
                .NotNull().WithMessage("Scheduled pickup date is required.")
                .GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-5))
                .WithMessage("Scheduled pickup date cannot be in the past.");

            RuleFor(x => x.ScheduledDeliveryDate)
                .NotNull().WithMessage("Scheduled delivery date is required.")
                .GreaterThan(x => x.ScheduledPickupDate)
                .WithMessage("Scheduled delivery date must be after scheduled pickup date.");

            RuleFor(x => x.ActualPickupDate)
                .LessThanOrEqualTo(DateTime.UtcNow).When(x => x.ActualPickupDate.HasValue)
                .WithMessage("Actual pickup date cannot be in the future.");

            RuleFor(x => x.ActualDeliveryDate)
                .GreaterThanOrEqualTo(x => x.ActualPickupDate)
                .When(x => x.ActualDeliveryDate.HasValue && x.ActualPickupDate.HasValue)
                .WithMessage("Actual delivery date must be after actual pickup date.")
                .LessThanOrEqualTo(DateTime.UtcNow).When(x => x.ActualDeliveryDate.HasValue)
                .WithMessage("Actual delivery date cannot be in the future.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid order status.")
                .Must(status => Enum.IsDefined(typeof(OrderStatus), status)).WithMessage("Invalid order status value.");

            RuleFor(x => x.TotalWeightKg)
                .GreaterThan(0).WithMessage("Total weight must be greater than 0 kg.");

            RuleFor(x => x.TotalVolumeM3)
                .GreaterThan(0).WithMessage("Total volume must be greater than 0 m³.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleForEach(x => x.Cargo)
                .SetValidator(new CargoRequestValidator()); // Валідація для кожного елемента в списку Cargo
        }
    }
}
