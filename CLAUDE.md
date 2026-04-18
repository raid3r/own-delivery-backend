# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run API locally
dotnet run --project src/OwnDeliveryApiP33/OwnDeliveryApiP33.csproj

# Run all tests
dotnet test

# Run unit tests only
dotnet test tests/OwnDeliveryApiP33.Tests.Unit/

# Run integration tests only
dotnet test tests/OwnDeliveryApiP33.Tests.Integration/

# Run a single test method
dotnet test --filter "FullyQualifiedName~AuthServiceTests.RegisterAsync_WithValidData_ReturnsToken"

# Add EF Core migration
dotnet ef migrations add MigrationName --project src/OwnDeliveryApiP33

# Docker build and run
docker build -t own-delivery-api .
docker run -p 8080:8080 own-delivery-api
```

## Architecture

The project follows a layered Clean Architecture pattern:

- **Controllers** — ASP.NET Core API endpoints (`/api/v1/auth`, `/api/v1/couriers`, `/api/v1/orders`, `/api/v1/tariffs`). Thin — delegate all logic to services.
- **Application** — Business logic: Services (interfaces + implementations), DTOs, FluentValidation validators, custom exceptions, and extension methods.
- **Domain** — Core entities (User, Courier, Customer, Order, Payment, Rating, Tariff, Notification, etc.), enums (OrderStatus, CourierStatus, PaymentStatus), and value objects (Address, Location, Dimensions).
- **Infrastructure** — EF Core `ApplicationDbContext`, generic `Repository<T>`, specialized repositories, Unit of Work pattern, and global exception handler middleware.
- **Migrations** — EF Core migration files; migrations run automatically on startup via `db.Database.Migrate()`.

### Key patterns

- **Repository + Unit of Work**: `IUnitOfWork` manages transactions; repositories are lazy-initialized.
- **Service Layer**: All business logic lives in `Application/Services/`. Controllers call service interfaces only.
- **Value Objects**: `Address`, `Location`, and `Dimensions` are mapped with EF Core `OwnsOne`.
- **JWT Auth**: Access tokens (15 min) + refresh tokens (7 days). Token logic is in `ITokenService`.

## Tech Stack

- .NET 8.0 / ASP.NET Core 8.0
- Entity Framework Core 8.0 with SQL Server (in-memory for tests)
- ASP.NET Core Identity + JWT Bearer authentication
- FluentValidation 11
- Swagger/Swashbuckle 6 (available at `/swagger` in development)
- XUnit + NSubstitute + FluentAssertions (unit tests); `WebApplicationFactory` (integration tests)

## Testing Notes

- **Unit tests** use NSubstitute mocks and an in-memory EF context — no external dependencies.
- **Integration tests** use `DeliveryApiFactory` (a custom `WebApplicationFactory`) which swaps SQL Server for the in-memory EF provider. Tests are grouped under `Auth/`, `Couriers/`, and `Infrastructure/`.
- No database or server setup is needed to run any tests.

## Configuration

Environment-specific settings are resolved via `appsettings.{Environment}.json`. Development uses a local SQL Server (`SILVERSTONE\SQLEXPRESS`, database `OwnDeliveryDb`). The JWT key must be at least 32 characters.

For local development without SQL Server, point `ConnectionStrings:DefaultConnection` to an accessible instance, or rely on the in-memory provider used by tests.