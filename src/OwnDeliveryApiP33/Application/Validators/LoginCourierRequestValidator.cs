using FluentValidation;
using OwnDeliveryApiP33.Application.DTOs;

namespace OwnDeliveryApiP33.Application.Validators;

public class LoginCourierRequestValidator : AbstractValidator<LoginCourierRequest>
{
    public LoginCourierRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long");
    }
}
