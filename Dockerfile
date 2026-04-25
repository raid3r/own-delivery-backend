# ── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

RUN dotnet tool install -g dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Restore layer (cached until .csproj changes)
COPY src/OwnDeliveryApiP33/OwnDeliveryApiP33.csproj src/OwnDeliveryApiP33/
RUN dotnet restore src/OwnDeliveryApiP33/OwnDeliveryApiP33.csproj

# Copy source and publish
COPY src/OwnDeliveryApiP33/ src/OwnDeliveryApiP33/
RUN dotnet publish src/OwnDeliveryApiP33/OwnDeliveryApiP33.csproj \
        -c Release \
        -o /app/publish

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# .NET 8 container default port
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OwnDeliveryApiP33.dll"]
