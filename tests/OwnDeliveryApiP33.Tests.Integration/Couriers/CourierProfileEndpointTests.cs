using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Infrastructure.Data;
using OwnDeliveryApiP33.Tests.Integration.Infrastructure;

namespace OwnDeliveryApiP33.Tests.Integration.Couriers;

public class CourierProfileEndpointTests : IClassFixture<DeliveryApiFactory>
{
    private readonly HttpClient _client;
    private readonly DeliveryApiFactory _factory;

    public CourierProfileEndpointTests(DeliveryApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ── GET /api/v1/couriers/me ──────────────────────────────────────────────

    [Fact]
    public async Task GetProfile_WithValidToken_Returns200WithProfile()
    {
        var email    = UniqueEmail();
        var password = "Profile1";
        var token    = await RegisterAndGetTokenAsync(email, password);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/couriers/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<CourierProfileResponse>();
        profile.Should().NotBeNull();
        profile!.Email.Should().Be(email.ToLower());
        profile.FirstName.Should().Be("Test");
        profile.LastName.Should().Be("Courier");
        profile.PhoneNumber.Should().Be("+380501234567");
        profile.IsActive.Should().BeTrue();
        profile.CourierId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProfile_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient(); // fresh client without auth header

        var response = await client.GetAsync("/api/v1/couriers/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_ReturnsCorrectCourierData()
    {
        var email    = UniqueEmail();
        var password = "Check1Pass";
        var token    = await RegisterAndGetTokenAsync(email, password, "Dmytro", "Petrenko", "+380671112233");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/couriers/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<CourierProfileResponse>();
        profile!.FirstName.Should().Be("Dmytro");
        profile.LastName.Should().Be("Petrenko");
        profile.PhoneNumber.Should().Be("+380671112233");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string UniqueEmail() => $"courier-{Guid.NewGuid():N}@example.com";

    private async Task<string> RegisterAndGetTokenAsync(
        string email,
        string password,
        string firstName = "Test",
        string lastName  = "Courier",
        string phone     = "+380501234567")
    {
        var request  = new RegisterCourierRequest(firstName, lastName, email, password, phone);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return body!.Token;
    }
}
