# ── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore layer (cached until .csproj changes)
COPY OwnDeliveryApiP33/OwnDeliveryApiP33.csproj OwnDeliveryApiP33/
RUN dotnet restore OwnDeliveryApiP33/OwnDeliveryApiP33.csproj

# Copy source and publish
COPY OwnDeliveryApiP33/ OwnDeliveryApiP33/
RUN dotnet publish OwnDeliveryApiP33/OwnDeliveryApiP33.csproj \
        -c Release \
        -o /app/publish \
        --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# .NET 8 container default port
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OwnDeliveryApiP33.dll"]
