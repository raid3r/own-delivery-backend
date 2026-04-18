using FluentValidation;
using OwnDeliveryApiP33.Application.DTOs;

namespace OwnDeliveryApiP33.Application.Validators;

public class RateOrderRequestValidator : AbstractValidator<RateOrderRequest>
{
    public RateOrderRequestValidator()
    {
        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5).WithMessage("Score must be between 1 and 5");

        RuleFor(x => x.Comment)
            .MaximumLength(500).WithMessage("Comment must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Comment));
    }
}
