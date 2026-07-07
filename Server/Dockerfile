# syntax=docker/dockerfile:1

# ---- Build stage: compile with the full SDK ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution + all project files first, so 'dotnet restore' is cached
# unless dependencies actually change.
COPY WorkVault.sln .
COPY src/WorkVault.API/WorkVault.API.csproj ./src/WorkVault.API/
COPY src/WorkVault.Application/WorkVault.Application.csproj ./src/WorkVault.Application/
COPY src/WorkVault.Domain/WorkVault.Domain.csproj ./src/WorkVault.Domain/
COPY src/WorkVault.Infrastructure/WorkVault.Infrastructure.csproj ./src/WorkVault.Infrastructure/
COPY src/WorkVault.SharedKernel/WorkVault.SharedKernel.csproj ./src/WorkVault.SharedKernel/

RUN dotnet restore

# Copy the rest of the source and publish a release build
COPY . .
RUN dotnet publish src/WorkVault.API/WorkVault.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage: small image, runtime only ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WorkVault.API.dll"]