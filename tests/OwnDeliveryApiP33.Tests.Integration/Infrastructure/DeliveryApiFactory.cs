using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Tests.Integration.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory that replaces the PostgreSQL provider with
/// an EF Core InMemory provider so integration tests have no external dependencies.
/// </summary>
public class DeliveryApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Capture DB name once so all scopes share the same in-memory store.
        var dbName = Guid.NewGuid().ToString();

        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Add InMemory database (unique per factory instance)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
        });
    }
}
