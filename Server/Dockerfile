# syntax=docker/dockerfile:1

# ---- Build stage: compile with the full SDK ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only the project files the API needs (no solution, no tests),
# so 'dotnet restore' is cached unless dependencies actually change.
COPY src/WorkVault.API/WorkVault.API.csproj ./src/WorkVault.API/
COPY src/WorkVault.Application/WorkVault.Application.csproj ./src/WorkVault.Application/
COPY src/WorkVault.Domain/WorkVault.Domain.csproj ./src/WorkVault.Domain/
COPY src/WorkVault.Infrastructure/WorkVault.Infrastructure.csproj ./src/WorkVault.Infrastructure/
COPY src/WorkVault.SharedKernel/WorkVault.SharedKernel.csproj ./src/WorkVault.SharedKernel/

# Restore just the API project and its dependency chain — NOT the solution,
# which would also try to restore the test project (not present in this image).
RUN dotnet restore src/WorkVault.API/WorkVault.API.csproj

# Copy only source (tests stay out of the production image)
COPY src/ ./src/
RUN dotnet publish src/WorkVault.API/WorkVault.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage: small image, runtime only ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WorkVault.API.dll"]