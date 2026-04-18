using FluentValidation;
using OwnDeliveryApiP33.Application.DTOs;

namespace OwnDeliveryApiP33.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.PickupAddress)
            .NotNull().WithMessage("Pickup address is required")
            .SetValidator(new AddressValidator());

        RuleFor(x => x.DeliveryAddress)
            .NotNull().WithMessage("Delivery address is required")
            .SetValidator(new AddressValidator());

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0");

        RuleFor(x => x.Dimensions)
            .NotNull().WithMessage("Dimensions are required")
            .SetValidator(new DimensionsValidator());

        RuleFor(x => x.TariffId)
            .NotEmpty().WithMessage("Tariff ID is required");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.SpecialInstructions)
            .MaximumLength(500).WithMessage("Special instructions must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.SpecialInstructions));

        RuleFor(x => x.ScheduledDeliveryTime)
            .GreaterThan(DateTime.UtcNow).WithMessage("Scheduled delivery time must be in the future")
            .When(x => x.ScheduledDeliveryTime.HasValue);
    }
}
