using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;
using OwnDeliveryApiP33.Infrastructure.Data;
using OwnDeliveryApiP33.Tests.Integration.Infrastructure;

namespace OwnDeliveryApiP33.Tests.Integration.Auth;

public class AuthEndpointsTests : IClassFixture<DeliveryApiFactory>
{
    private readonly HttpClient _client;
    private readonly DeliveryApiFactory _factory;

    public AuthEndpointsTests(DeliveryApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ── POST /api/v1/auth/register ───────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidRequest_Returns201WithToken()
    {
        var request = new RegisterCourierRequest(
            "Olga", "Kovalenko", UniqueEmail(), "Password1", "+380501234567");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.Email.Should().Be(request.Email.ToLower());
        body.FirstName.Should().Be("Olga");
        body.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var email = UniqueEmail();
        var first  = new RegisterCourierRequest("A", "B", email, "Pass123", "+1");
        var second = new RegisterCourierRequest("C", "D", email, "Pass456", "+2");

        await _client.PostAsJsonAsync("/api/v1/auth/register", first);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", second);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_DuplicateEmailCaseInsensitive_Returns409()
    {
        var lowerEmail = UniqueEmail();
        var upperEmail = lowerEmail.ToUpper();

        var first  = new RegisterCourierRequest("A", "B", lowerEmail, "Pass123", "+1");
        var second = new RegisterCourierRequest("C", "D", upperEmail, "Pass456", "+2");

        await _client.PostAsJsonAsync("/api/v1/auth/register", first);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", second);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("", "Doe", "valid@test.com", "Pass123", "+1")]
    [InlineData("John", "", "valid2@test.com", "Pass123", "+1")]
    [InlineData("John", "Doe", "not-an-email", "Pass123", "+1")]
    [InlineData("John", "Doe", "valid3@test.com", "12345", "+1")] // password too short
    [InlineData("John", "Doe", "valid4@test.com", "Pass123", "")]
    public async Task Register_InvalidFields_Returns400(
        string firstName, string lastName, string email, string password, string phone)
    {
        var request = new RegisterCourierRequest(firstName, lastName, email, password, phone);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── POST /api/v1/auth/login ──────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithToken()
    {
        var email    = UniqueEmail();
        var password = "MyPass123";
        await RegisterAndSeedAsync(email, password);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginCourierRequest(email, password));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.Email.Should().Be(email.ToLower());
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var email = UniqueEmail();
        await RegisterAndSeedAsync(email, "CorrectPass1");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginCourierRequest(email, "WrongPass1"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginCourierRequest("ghost@example.com", "Pass123"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("not-an-email", "Pass123")]
    [InlineData("valid@test.com", "")]
    [InlineData("", "Pass123")]
    public async Task Login_InvalidRequest_Returns400(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginCourierRequest(email, password));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_CaseInsensitiveEmail_Returns200()
    {
        var email = UniqueEmail(); // lower case
        await RegisterAndSeedAsync(email, "Pass123");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginCourierRequest(email.ToUpper(), "Pass123"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Register → Login roundtrip ───────────────────────────────────────────

    [Fact]
    public async Task RegisterThenLogin_RoundTrip_Works()
    {
        var email    = UniqueEmail();
        var password = "RoundTrip1";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterCourierRequest("Round", "Trip", email, password, "+380501234567"));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginCourierRequest(email, password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        loginBody!.Token.Should().NotBeNullOrWhiteSpace();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Creates a unique email address for each test to avoid conflicts in shared DB.</summary>
    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";

    /// <summary>Seeds a user directly into the DB using the DI container.</summary>
    private async Task RegisterAndSeedAsync(string email, string password)
    {
        using var scope = _factory.Services.CreateScope();
        var context   = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher    = scope.ServiceProvider.GetRequiredService<PasswordHasher<User>>();

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
        user.PasswordHash = hasher.HashPassword(user, password);
        
        var courier = new Courier
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        context.Users.Add(user);
        context.Couriers.Add(courier);
        await context.SaveChangesAsync();
    }
}
