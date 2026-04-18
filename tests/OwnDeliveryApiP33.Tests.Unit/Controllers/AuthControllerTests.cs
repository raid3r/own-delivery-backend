using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Services;
using OwnDeliveryApiP33.Application.Validators;
using OwnDeliveryApiP33.Controllers;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Tests.Unit.Controllers;

public class AuthControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly AuthController _sut;
    private readonly IAuthService _authService;

    private static IConfiguration BuildJwtConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]              = "UnitTestSuperSecretKey_AtLeast32Chars!",
                ["Jwt:Issuer"]           = "TestIssuer",
                ["Jwt:Audience"]         = "TestAudience",
                ["Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();

    public AuthControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
            .Options;
        _context = new ApplicationDbContext(options);

        var config = BuildJwtConfig();
        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        var tokenService = new TokenService(config);
        
        _authService = new AuthService(
            _context,
            tokenService,
            new RegisterCourierRequestValidator(),
            new LoginCourierRequestValidator(),
            new LoginRequestValidator(),
            passwordHasher);

        _sut = new AuthController(_authService, null!);
    }

    // ── Register ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidRequest_Returns201AndToken()
    {
        var request = new RegisterCourierRequest("Jane", "Smith", "jane@example.com", "SecurePass1", "+380501112233");

        var result = await _sut.Register(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var response = created.Value.Should().BeOfType<AuthResponse>().Subject;

        response.Email.Should().Be("jane@example.com");
        response.FirstName.Should().Be("Jane");
        response.LastName.Should().Be("Smith");
        response.Token.Should().NotBeNullOrWhiteSpace();
        response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Register_SavesToDatabase()
    {
        var request = new RegisterCourierRequest("Ivan", "Petrov", "ivan@example.com", "SecurePass1", "+380509876543");

        await _sut.Register(request, CancellationToken.None);

        var saved = await _context.Users.SingleOrDefaultAsync(u => u.Email == "ivan@example.com");
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        var email = "duplicate@example.com";
        var req1 = new RegisterCourierRequest("John", "Doe", email, "SecurePass1", "+380501234567");
        var req2 = new RegisterCourierRequest("Jane", "Doe", email, "SecurePass2", "+380509876543");

        await _sut.Register(req1, CancellationToken.None);
        var result = await _sut.Register(req2, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsBadRequest()
    {
        var request = new RegisterCourierRequest("John", "Doe", "invalid-email", "SecurePass1", "+380501234567");

        var result = await _sut.Register(request, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        var request = new RegisterCourierRequest("John", "Doe", "john@example.com", "weak", "+380501234567");

        var result = await _sut.Register(request, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ── Login ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndToken()
    {
        await SeedUserAsync("valid@example.com", "SecurePass1");

        var request = new LoginCourierRequest("valid@example.com", "SecurePass1");
        var result = await _sut.Login(request, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<AuthResponse>().Subject;

        response.Email.Should().Be("valid@example.com");
        response.Token.Should().NotBeNullOrWhiteSpace();
        response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
    {
        var request = new LoginCourierRequest("notexist@example.com", "SecurePass1");

        var result = await _sut.Login(request, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        await SeedUserAsync("existing@example.com", "CorrectPass1");

        var request = new LoginCourierRequest("existing@example.com", "WrongPass1");
        var result = await _sut.Login(request, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsBadRequest()
    {
        var request = new LoginCourierRequest("invalid-email", "SecurePass1");

        var result = await _sut.Login(request, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ── Helper Methods ──────────────────────────────────────────────────────────

    private async Task SeedUserAsync(string email, string password)
    {
        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Test",
            Email = email.ToLower(),
            PhoneNumber = "+380501234567",
            Role = UserRole.Courier,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        user.PasswordHash = passwordHasher.HashPassword(user, password);
        
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
    }

    public void Dispose() => _context.Dispose();
}
