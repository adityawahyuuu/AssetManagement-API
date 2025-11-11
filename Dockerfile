# ============================================
# Multi-Stage Dockerfile for Asset Management API
# Optimized for production deployment on Render
# ============================================

# ============================================
# Stage 1: Build Stage
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory
WORKDIR /src

# Copy solution and project files
COPY ["AssetManagement.sln", "./"]
COPY ["API/API.csproj", "API/"]
COPY ["API.Test/API.Test.csproj", "API.Test/"]

# Restore dependencies (cached layer if project files unchanged)
RUN dotnet restore "AssetManagement.sln"

# Copy all source code
COPY . .

# Build the API project
WORKDIR /src/API
RUN dotnet build "API.csproj" -c Release -o /app/build

# ============================================
# Stage 2: Publish Stage
# ============================================
FROM build AS publish

# Publish the application
RUN dotnet publish "API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# ============================================
# Stage 3: Runtime Stage
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set working directory
WORKDIR /app

# Install PostgreSQL client for health checks and migrations
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    postgresql-client \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && \
    useradd -r -g appuser appuser && \
    chown -R appuser:appuser /app

# Copy published application from publish stage
COPY --from=publish --chown=appuser:appuser /app/publish .

# Copy migrations folder for database setup
COPY --chown=appuser:appuser migrations/ ./migrations/

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:80 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Switch to non-root user
USER appuser

# Expose port 80
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:80/api/asset-categories || exit 1

# Set the entry point
ENTRYPOINT ["dotnet", "API.dll"]

# ============================================
# Build and Run Instructions
# ============================================
#
# Build the image:
#   docker build -t assetmanagement-api:latest .
#
# Run the container locally:
#   docker run -d \
#     --name assetmanagement-api \
#     -p 8080:80 \
#     -e ConnectionStrings__AssetManagementConnection="Host=localhost;Database=assetmanagement;Username=postgres;Password=yourpassword" \
#     -e Jwt__Secret="your-super-secret-jwt-key-min-32-chars" \
#     -e Email__SenderEmail="your-email@gmail.com" \
#     -e Email__Username="your-email@gmail.com" \
#     -e Email__Password="your-app-password" \
#     assetmanagement-api:latest
#
# Run database migrations:
#   docker exec assetmanagement-api psql $ConnectionStrings__AssetManagementConnection -f migrations/database_schema.sql
#
# View logs:
#   docker logs assetmanagement-api
#
# Stop and remove container:
#   docker stop assetmanagement-api
#   docker rm assetmanagement-api
#
# ============================================
# Production Deployment on Render
# ============================================
#
# Render will automatically:
# 1. Build this Dockerfile
# 2. Deploy the container
# 3. Inject environment variables from render.yaml
# 4. Configure networking and SSL
# 5. Run health checks
#
# Manual steps after first deployment:
# 1. Access Render Shell
# 2. Run database migrations (see DEPLOYMENT.md)
# 3. Verify API is accessible
# 4. Configure custom domain (optional)
#
# ============================================
