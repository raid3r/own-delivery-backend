using FluentAssertions;
using Microsoft.Extensions.Configuration;
using OwnDeliveryApiP33.Application.Services;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;

namespace OwnDeliveryApiP33.Tests.Unit.Services;

public class TokenServiceTests
{
    private readonly TokenService _sut;
    private readonly IConfiguration _configuration;

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

    public TokenServiceTests()
    {
        _configuration = BuildJwtConfig();
        _sut = new TokenService(_configuration);
    }

    // ?? GenerateToken - Basic Functionality ??????????????????????????????????

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsTokenAndExpiresAt()
    {
        var user = CreateTestUser();

        var (token, expiresAt) = _sut.GenerateToken(user);

        token.Should().NotBeNullOrWhiteSpace();
        expiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwtToken()
    {
        var user = CreateTestUser();

        var (token, _) = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var canRead = handler.CanReadToken(token);
        canRead.Should().BeTrue();
    }

    [Fact]
    public void GenerateToken_TokenCanBeParsed()
    {
        var user = CreateTestUser();

        var (token, _) = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Should().NotBeNull();
    }

    // ?? GenerateToken - Claims ???????????????????????????????????????????????

    [Fact]
    public void GenerateToken_ContainsUserIdInSubjectClaim()
    {
        var user = CreateTestUser();

        var (token, _) = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Subject.Should().Be(user.Id.ToString());
    }

    [Fact]
    public void GenerateToken_ContainsEmailClaim()
    {
        var user = CreateTestUser("test@example.com");

        var (token, _) = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == "test@example.com");
    }

    [Fact]
    public void GenerateToken_ContainsRoleClaim()
    {
        var user = CreateTestUser(role: UserRole.Courier);

        var (token, _) = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Courier");
    }

    [Fact]
    public void GenerateToken_ContainsCorrectIssuer()
    {
        var user = CreateTestUser();

        var (token, _) = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be("TestIssuer");
    }

    [Fact]
    public void GenerateToken_ContainsCorrectAudience()
    {
        var user = CreateTestUser();

        var (token, _) = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Audiences.Should().Contain("TestAudience");
    }

    // ?? GenerateToken - Expiration ?????????????????????????????????????

    [Fact]
    public void GenerateToken_ExpiresAtIsInFuture()
    {
        var user = CreateTestUser();

        var (_, expiresAt) = _sut.GenerateToken(user);

        expiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateToken_ExpirationIsApproximately60Minutes()
    {
        var user = CreateTestUser();
        var before = DateTime.UtcNow;

        var (_, expiresAt) = _sut.GenerateToken(user);

        var duration = expiresAt - before;
        duration.Should().BeCloseTo(TimeSpan.FromMinutes(60), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateToken_TokenExpirationClaimMatchesReturnedExpiresAt()
    {
        var user = CreateTestUser();

        var (token, expiresAt) = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.ValidTo.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(1));
    }

    // ?? GenerateToken - Different Users ???????????????????????????????????????

    [Fact]
    public void GenerateToken_GeneratesDifferentTokensForDifferentUsers()
    {
        var user1 = CreateTestUser();
        var user2 = CreateTestUser();

        var (token1, _) = _sut.GenerateToken(user1);
        var (token2, _) = _sut.GenerateToken(user2);

        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateToken_DifferentUsersHaveDifferentSubjectClaims()
    {
        var user1 = CreateTestUser();
        var user2 = CreateTestUser();

        var (token1, _) = _sut.GenerateToken(user1);
        var (token2, _) = _sut.GenerateToken(user2);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken1 = handler.ReadJwtToken(token1);
        var jwtToken2 = handler.ReadJwtToken(token2);

        jwtToken1.Subject.Should().NotBe(jwtToken2.Subject);
    }

    // ?? GenerateToken - Configuration from appsettings.json ??????????????

    [Fact]
    public void GenerateToken_UsesConfigurationFromDependency()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "DifferentSecretKeyForJWT_AtLeast32Characters!!",
                ["Jwt:Issuer"] = "CustomIssuer",
                ["Jwt:Audience"] = "CustomAudience",
                ["Jwt:ExpiresInMinutes"] = "120"
            })
            .Build();

        var service = new TokenService(config);
        var user = CreateTestUser();

        var (_, expiresAt) = service.GenerateToken(user);

        // Check expiration is approximately 120 minutes instead of 60
        var duration = expiresAt - DateTime.UtcNow;
        duration.Should().BeCloseTo(TimeSpan.FromMinutes(120), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateToken_UsesCustomIssuerFromConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "AnotherSecretKeyForJWT_AtLeast32Characters!!",
                ["Jwt:Issuer"] = "MyCustomIssuer",
                ["Jwt:Audience"] = "MyCustomAudience",
                ["Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();

        var service = new TokenService(config);
        var user = CreateTestUser();

        var (token, _) = service.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be("MyCustomIssuer");
    }

    // ?? Helper Methods ???????????????????????????????????????????????????????

    private static User CreateTestUser(
        string? email = null,
        UserRole role = UserRole.Courier)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FullName = "John Doe",
            Email = email ?? "john@example.com",
            PhoneNumber = "+380501234567",
            PasswordHash = "hashedpassword",
            Role = role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
