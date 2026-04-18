using FluentValidation;
using OwnDeliveryApiP33.Application.DTOs;

namespace OwnDeliveryApiP33.Application.Validators;

public class DimensionsValidator : AbstractValidator<DimensionsDto>
{
    public DimensionsValidator()
    {
        RuleFor(x => x.Width)
            .GreaterThan(0).WithMessage("Width must be greater than 0");

        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("Length must be greater than 0");

        RuleFor(x => x.Height)
            .GreaterThan(0).WithMessage("Height must be greater than 0");
    }
}
