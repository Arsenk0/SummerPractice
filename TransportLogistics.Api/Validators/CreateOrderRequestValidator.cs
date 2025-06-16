// TransportLogistics.Api/Validators/CreateOrderRequestValidator.cs
using FluentValidation;
using TransportLogistics.Api.DTOs; // Додано для CargoRequestDto
using TransportLogistics.Api.Data.Entities;
using System;
using System.Linq;

namespace TransportLogistics.Api.Validators
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.OriginAddress)
                .NotEmpty().WithMessage("Origin address is required.")
                .MaximumLength(200).WithMessage("Origin address cannot exceed 200 characters.");

            RuleFor(x => x.DestinationAddress)
                .NotEmpty().WithMessage("Destination address is required.")
                .MaximumLength(200).WithMessage("Destination address cannot exceed 200 characters.");

            RuleFor(x => x.ScheduledPickupDate)
                .NotNull().WithMessage("Scheduled pickup date is required.")
                .GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-5)).WithMessage("Scheduled pickup date cannot be in the past.");

            RuleFor(x => x.ScheduledDeliveryDate)
                .NotNull().WithMessage("Scheduled delivery date is required.")
                .GreaterThan(x => x.ScheduledPickupDate)
                .WithMessage("Scheduled delivery date must be after scheduled pickup date.");

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

    // Валідатор для CargoRequestDto (раніше CargoRequest)
    // Тепер він знаходиться у цьому ж файлі, але валідує CargoRequestDto з DTOs папки
    public class CargoRequestValidator : AbstractValidator<CargoRequestDto> // Змінено на CargoRequestDto
    {
        public CargoRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Cargo name is required.")
                .MaximumLength(100).WithMessage("Cargo name cannot exceed 100 characters.");

            RuleFor(x => x.WeightKg)
                .GreaterThan(0).WithMessage("Cargo weight must be greater than 0 kg.");

            RuleFor(x => x.VolumeM3)
                .GreaterThan(0).WithMessage("Cargo volume must be greater than 0 m³.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Cargo quantity must be greater than 0.");
        }
    }
}
