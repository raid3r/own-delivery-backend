using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Services;
using OwnDeliveryApiP33.Application.Validators;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Tests.Unit.Services;

public class AuthServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly AuthService _sut;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IValidator<RegisterCourierRequest> _registerValidator;
    private readonly IValidator<LoginCourierRequest> _loginValidator;

    private static IConfiguration BuildJwtConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsAVeryLongSecretKeyForJWT_AtLeast32Characters!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        var config = BuildJwtConfig();
        _passwordHasher = new PasswordHasher<User>();
        _tokenService = new TokenService(config);
        _registerValidator = new RegisterCourierRequestValidator();
        _loginValidator = new LoginCourierRequestValidator();

        _sut = new AuthService(
            _context,
            _tokenService,
            _registerValidator,
            _loginValidator,
            new LoginRequestValidator(),
            _passwordHasher);
    }

    // ?? Register - Success Cases ????????????????????????????????????????????

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsCourierData()
    {
        var request = new RegisterCourierRequest(
            "John",
            "Doe",
            "john@example.com",
            "SecurePass1",
            "+380501234567");

        var result = await _sut.RegisterAsync(request);

        result.Should().NotBeNull();
        result.Email.Should().Be("john@example.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsTokenAndExpiration()
    {
        var request = new RegisterCourierRequest(
            "Alice",
            "Smith",
            "alice@example.com",
            "SecurePass1",
            "+380501234567");

        var result = await _sut.RegisterAsync(request);

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task RegisterAsync_SavesCourierToDatabase()
    {
        var request = new RegisterCourierRequest(
            "Bob",
            "Johnson",
            "bob@example.com",
            "SecurePass1",
            "+380501234567");

        await _sut.RegisterAsync(request);

        var savedUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == "bob@example.com");
        savedUser.Should().NotBeNull();
        savedUser!.FullName.Should().Contain("Bob");
        savedUser.FullName.Should().Contain("Johnson");
    }

    [Fact]
    public async Task RegisterAsync_HashesPassword()
    {
        var request = new RegisterCourierRequest(
            "Charlie",
            "Brown",
            "charlie@example.com",
            "SecurePass1",
            "+380501234567");

        await _sut.RegisterAsync(request);

        var savedUser = await _context.Users.SingleAsync(u => u.Email == "charlie@example.com");
        savedUser.PasswordHash.Should().NotBe(request.Password);
        savedUser.PasswordHash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RegisterAsync_NormalizesEmailToLowercase()
    {
        var request = new RegisterCourierRequest(
            "Dave",
            "Wilson",
            "Dave.Wilson@EXAMPLE.COM",
            "SecurePass1",
            "+380501234567");

        await _sut.RegisterAsync(request);

        var savedUser = await _context.Users.SingleAsync();
        savedUser.Email.Should().Be("dave.wilson@example.com");
    }

    [Fact]
    public async Task RegisterAsync_SetsStatusToActive()
    {
        var request = new RegisterCourierRequest(
            "Eve",
            "Taylor",
            "eve@example.com",
            "SecurePass1",
            "+380501234567");

        await _sut.RegisterAsync(request);

        var savedUser = await _context.Users.SingleAsync();
        savedUser.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task RegisterAsync_SetsCreatedAtToUtcNow()
    {
        var beforeCall = DateTime.UtcNow;
        var request = new RegisterCourierRequest(
            "Frank",
            "Miller",
            "frank@example.com",
            "SecurePass1",
            "+380501234567");

        await _sut.RegisterAsync(request);

        var afterCall = DateTime.UtcNow.AddSeconds(1);
        var savedUser = await _context.Users.SingleAsync();
        savedUser.CreatedAt.Should().BeCloseTo(beforeCall, TimeSpan.FromSeconds(2));
        savedUser.CreatedAt.Should().BeOnOrAfter(beforeCall);
        savedUser.CreatedAt.Should().BeOnOrBefore(afterCall);
    }

    // ?? Register - Validation Failures ??????????????????????????????????????

    [Theory]
    [InlineData("", "Doe", "test@example.com", "Pass1", "+380")]
    [InlineData("John", "", "test@example.com", "Pass1", "+380")]
    [InlineData("John", "Doe", "invalid-email", "Pass1", "+380")]
    [InlineData("John", "Doe", "test@example.com", "123", "+380")] // password too short
    [InlineData("John", "Doe", "test@example.com", "Pass1", "")] // no phone
    public async Task RegisterAsync_InvalidRequest_ThrowsValidationException(
        string firstName, string lastName, string email, string password, string phone)
    {
        var request = new RegisterCourierRequest(firstName, lastName, email, password, phone);

        var act = () => _sut.RegisterAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        var email = "duplicate@example.com";
        var first = new RegisterCourierRequest("John", "Doe", email, "Pass1234", "+380501234567");
        var second = new RegisterCourierRequest("Jane", "Smith", email, "Pass5678", "+380509876543");

        await _sut.RegisterAsync(first);

        var act = () => _sut.RegisterAsync(second);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*email already exists*");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmailCaseInsensitive_ThrowsInvalidOperationException()
    {
        var first = new RegisterCourierRequest("John", "Doe", "user@example.com", "Pass1234", "+380501234567");
        var second = new RegisterCourierRequest("Jane", "Smith", "USER@EXAMPLE.COM", "Pass5678", "+380509876543");

        await _sut.RegisterAsync(first);

        var act = () => _sut.RegisterAsync(second);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ?? Login - Success Cases ????????????????????????????????????????????????

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        await SeedUserAsync("login@example.com", "CorrectPass1");

        var result = await _sut.LoginAsync(
            new LoginCourierRequest("login@example.com", "CorrectPass1"));

        result.Should().NotBeNull();
        result.Email.Should().Be("login@example.com");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        await SeedUserAsync("token@example.com", "CorrectPass1");

        var result = await _sut.LoginAsync(
            new LoginCourierRequest("token@example.com", "CorrectPass1"));

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_CaseInsensitiveEmail_Succeeds()
    {
        await SeedUserAsync("case@example.com", "CorrectPass1");

        var result = await _sut.LoginAsync(
            new LoginCourierRequest("CASE@EXAMPLE.COM", "CorrectPass1"));

        result.Email.Should().Be("case@example.com");
    }

    [Fact]
    public async Task LoginAsync_ReturnsCourierData()
    {
        var user = await SeedUserAsync("data@example.com", "Pass1234");

        var result = await _sut.LoginAsync(
            new LoginCourierRequest("data@example.com", "Pass1234"));

        result.CourierId.Should().Be(user.Id);
    }

    // ?? Login - Validation Failures ??????????????????????????????????????????

    [Theory]
    [InlineData("invalid-email", "Pass1")]
    [InlineData("test@example.com", "")] // empty password
    [InlineData("", "Pass1")]
    public async Task LoginAsync_InvalidRequest_ThrowsValidationException(string email, string password)
    {
        var request = new LoginCourierRequest(email, password);

        var act = () => _sut.LoginAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // ?? Login - Authentication Failures ??????????????????????????????????????

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsUnauthorizedAccessException()
    {
        var request = new LoginCourierRequest("unknown@example.com", "Pass1234");

        var act = () => _sut.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        await SeedUserAsync("wrong@example.com", "CorrectPass1");

        var request = new LoginCourierRequest("wrong@example.com", "WrongPass");

        var act = () => _sut.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task LoginAsync_SimilarButDifferentPassword_ThrowsUnauthorizedAccessException()
    {
        await SeedUserAsync("similar@example.com", "SecurePass1");

        var request = new LoginCourierRequest("similar@example.com", "SecurePass2");

        var act = () => _sut.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ?? Register and Login Integration ???????????????????????????????????????

    [Fact]
    public async Task RegisterThenLogin_Succeeds()
    {
        var email = "flow@example.com";
        var password = "SecurePass1";
        var registerRequest = new RegisterCourierRequest("Test", "User", email, password, "+380501234567");
        var loginRequest = new LoginCourierRequest(email, password);

        var registerResult = await _sut.RegisterAsync(registerRequest);
        var loginResult = await _sut.LoginAsync(loginRequest);

        loginResult.CourierId.Should().Be(registerResult.CourierId);
        loginResult.Email.Should().Be(registerResult.Email);
    }

    [Fact]
    public async Task RegisterWithUppercaseEmailThenLoginWithLowercase_Succeeds()
    {
        var email = "mixedcase@example.com";
        var password = "SecurePass1";
        var registerRequest = new RegisterCourierRequest("Test", "User", email.ToUpper(), password, "+380501234567");
        var loginRequest = new LoginCourierRequest(email.ToLower(), password);

        await _sut.RegisterAsync(registerRequest);
        var loginResult = await _sut.LoginAsync(loginRequest);

        loginResult.Email.Should().Be(email.ToLower());
    }

    // ?? Helpers ??????????????????????????????????????????????????????????????

    private async Task<User> SeedUserAsync(string email, string password)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Seed User",
            Email = email.ToLower(),
            PhoneNumber = "+380501234567",
            Role = UserRole.Courier,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        
        var courier = new Courier
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Users.Add(user);
        _context.Couriers.Add(courier);
        await _context.SaveChangesAsync();
        return user;
    }

    public void Dispose() => _context.Dispose();
}
