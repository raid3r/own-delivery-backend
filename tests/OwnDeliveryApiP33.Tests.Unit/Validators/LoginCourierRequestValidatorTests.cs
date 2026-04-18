using FluentAssertions;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Validators;

namespace OwnDeliveryApiP33.Tests.Unit.Validators;

public class LoginCourierRequestValidatorTests
{
    private readonly LoginCourierRequestValidator _sut = new();

    private static LoginCourierRequest ValidRequest() =>
        new(Email: "courier@example.com", Password: "SecurePass1");

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        var result = await _sut.ValidateAsync(ValidRequest());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    public async Task InvalidEmail_ShouldFailValidation(string email)
    {
        var result = await _sut.ValidateAsync(ValidRequest() with { Email = email });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCourierRequest.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Pass")] // too short
    public async Task EmptyPassword_ShouldFailValidation(string password)
    {
        var result = await _sut.ValidateAsync(ValidRequest() with { Password = password });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCourierRequest.Password));
    }

    [Fact]
    public async Task BothFieldsInvalid_ShouldReturnErrors()
    {
        var result = await _sut.ValidateAsync(new LoginCourierRequest(Email: "bad-email", Password: ""));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}
