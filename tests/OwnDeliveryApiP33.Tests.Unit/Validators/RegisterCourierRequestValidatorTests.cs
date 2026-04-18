using FluentAssertions;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Validators;

namespace OwnDeliveryApiP33.Tests.Unit.Validators;

public class RegisterCourierRequestValidatorTests
{
    private readonly RegisterCourierRequestValidator _sut = new();

    private static RegisterCourierRequest ValidRequest() => new(
        FirstName: "John",
        LastName: "Doe",
        Email: "john.doe@example.com",
        Password: "SecurePass1",
        PhoneNumber: "+380501234567"
    );

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        var result = await _sut.ValidateAsync(ValidRequest());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EmptyFirstName_ShouldFailValidation(string firstName)
    {
        var result = await _sut.ValidateAsync(ValidRequest() with { FirstName = firstName });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCourierRequest.FirstName));
    }

    [Fact]
    public async Task TooLongFirstName_ShouldFailValidation()
    {
        var result = await _sut.ValidateAsync(ValidRequest() with { FirstName = new string('A', 101) });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCourierRequest.FirstName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EmptyLastName_ShouldFailValidation(string lastName)
    {
        var result = await _sut.ValidateAsync(ValidRequest() with { LastName = lastName });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCourierRequest.LastName));
    }

    [Fact]
    public async Task TooLongLastName_ShouldFailValidation()
    {
        var result = await _sut.ValidateAsync(ValidRequest() with { LastName = new string('B', 101) });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCourierRequest.LastName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    public async Task InvalidEmail_ShouldFailValidation(string email)
    {
        var result = await _sut.ValidateAsync(ValidRequest() with { Email = email });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCourierRequest.Email));
    }

    [Fact]
    public async Task TooLongEmail_ShouldFailValidation()
    {
        var longEmail = new string('a', 190) + "@example.com"; // > 200 chars
        var result = await _sut.ValidateAsync(ValidRequest() with { Email = longEmail });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCourierRequest.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Pass")]   // too short (< 8)
    [InlineData("password")] // no uppercase
    [InlineData("PASSWORD1")] // no lowercase
    [InlineData("Password")] // no digit
    [InlineData("     ")] // whitespace only
    public async Task InvalidPassword_ShouldFailValidation(string password)
    {
        var result = await _sut.ValidateAsync(ValidRequest() with { Password = password });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCourierRequest.Password));
    }

    [Fact]
    public async Task TooLongPassword_ShouldFailValidation()
    {
        var result = await _sut.ValidateAsync(ValidRequest() with { Password = new string('P', 101) });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCourierRequest.Password));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EmptyPhoneNumber_ShouldFailValidation(string phone)
    {
        var result = await _sut.ValidateAsync(ValidRequest() with { PhoneNumber = phone });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCourierRequest.PhoneNumber));
    }

    [Fact]
    public async Task TooLongPhoneNumber_ShouldFailValidation()
    {
        var result = await _sut.ValidateAsync(ValidRequest() with { PhoneNumber = new string('9', 21) });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCourierRequest.PhoneNumber));
    }

    [Fact]
    public async Task MaxLengthValues_ShouldPassValidation()
    {
        var passwordWith100Chars = "SecurePassword1" + new string('p', 85); // 15 + 85 = 100

        var request = ValidRequest() with
        {
            FirstName = new string('A', 100),
            LastName = new string('B', 100),
            Password = passwordWith100Chars,
            PhoneNumber = "+123456789012345"
        };

        var result = await _sut.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidLicenseNumber_ShouldPassValidation()
    {
        var request = ValidRequest() with { LicenseNumber = "ABC123" };
        var result = await _sut.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task TooLongLicenseNumber_ShouldFailValidation()
    {
        var request = ValidRequest() with { LicenseNumber = new string('A', 51) };
        var result = await _sut.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
    }
}
