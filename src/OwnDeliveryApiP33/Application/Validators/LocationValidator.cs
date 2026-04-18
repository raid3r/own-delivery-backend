using FluentValidation;
using OwnDeliveryApiP33.Application.DTOs;

namespace OwnDeliveryApiP33.Application.Validators;

public class LocationValidator : AbstractValidator<LocationDto>
{
    public LocationValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");

        RuleFor(x => x.Accuracy)
            .GreaterThan(0).WithMessage("Accuracy must be greater than 0")
            .When(x => x.Accuracy.HasValue);

        RuleFor(x => x.Speed)
            .GreaterThanOrEqualTo(0).WithMessage("Speed must be greater than or equal to 0")
            .When(x => x.Speed.HasValue);
    }
}
