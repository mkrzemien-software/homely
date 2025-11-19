# Multi-stage Dockerfile for .NET 9 Backend with Environment Management
#
# This Dockerfile uses multi-stage builds for smaller production images
# and layer caching for faster builds.
#
# IMPORTANT: Connection strings and secrets are NOT embedded in the image.
# They MUST be provided via environment variables at runtime.
#
# Build examples:
#   docker build -t homely-backend --build-arg BUILD_ENV=Production .
#   docker build -t homely-backend --build-arg BUILD_ENV=Development .
#
# Run examples:
#   docker run -p 5000:5000 \
#     -e ASPNETCORE_ENVIRONMENT=Production \
#     -e ConnectionStrings__DefaultConnection="Host=..." \
#     -e Supabase__Url="https://..." \
#     -e Supabase__Key="..." \
#     -e JwtSettings__Secret="..." \
#     homely-backend

# =============================================================================
# Stage 1: Build
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Build argument for environment (Development, Production, etc.)
ARG BUILD_ENV=Production
ENV ASPNETCORE_ENVIRONMENT=${BUILD_ENV}

WORKDIR /app

# Copy solution and project files first (for layer caching)
COPY HomelyApi/*.sln ./
COPY HomelyApi/Homely.API/*.csproj ./Homely.API/

# Restore dependencies (cached layer if .csproj files don't change)
RUN dotnet restore

# Copy the rest of the source code
COPY HomelyApi/ .

# Build the application
WORKDIR /app/Homely.API
RUN dotnet build -c Release --no-restore

# =============================================================================
# Stage 2: Publish
# =============================================================================
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore

# =============================================================================
# Stage 3: Runtime
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Build argument for environment
ARG BUILD_ENV=Production

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy published application from publish stage
COPY --from=publish /app/publish .

# Verify that sensitive appsettings files are NOT in the image
RUN echo "Checking for sensitive files..." && \
    if [ -f "appsettings.Local.json" ]; then echo "WARNING: appsettings.Local.json found!"; exit 1; fi && \
    if [ -f "appsettings.Development.json" ] && [ "$(grep -c 'USE_ENVIRONMENT_VARIABLE' appsettings.Development.json || true)" -eq 0 ]; then echo "WARNING: appsettings.Development.json contains secrets!"; exit 1; fi && \
    if [ -f "appsettings.Production.json" ] && [ "$(grep -c 'USE_ENVIRONMENT_VARIABLE' appsettings.Production.json || true)" -eq 0 ]; then echo "WARNING: appsettings.Production.json contains secrets!"; exit 1; fi && \
    echo "✅ No sensitive files found in image"

# Create a non-root user for running the application (security best practice)
RUN useradd -m -u 1001 appuser && chown -R appuser:appuser /app
USER appuser

# Expose port (changed to 8080 for non-root user compatibility)
EXPOSE 8080

# Environment variables (can be overridden at runtime)
# IMPORTANT: These are defaults. Override with actual secrets at runtime!
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=${BUILD_ENV}

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "Homely.API.dll"]
