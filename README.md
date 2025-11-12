# Asset Management API

A RESTful API for managing dormitory assets, built with ASP.NET Core and PostgreSQL. This API provides comprehensive authentication, room management, and asset tracking capabilities.

## Features

- **User Authentication**: JWT-based authentication with email verification
- **Room Management**: Create, read, update, and delete rooms
- **Asset Tracking**: Manage assets within rooms with pagination support
- **Asset Categories**: Categorize assets for better organization
- **Email Notifications**: OTP verification and password reset via email
- **Secure Password Storage**: PBKDF2-based password hashing
- **CORS Support**: Configurable cross-origin resource sharing
- **API Documentation**: Interactive Swagger/OpenAPI documentation

## Tech Stack

- **Framework**: ASP.NET Core 6.0+
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Authentication**: JWT Bearer Tokens
- **Validation**: FluentValidation
- **API Documentation**: Swagger/OpenAPI
- **Email**: SMTP (Gmail)

## Prerequisites

Before deploying, ensure you have:

- [.NET SDK 6.0+](https://dotnet.microsoft.com/download)
- PostgreSQL database (local or hosted)
- SMTP email credentials (Gmail recommended)
- Git

## Local Development

### 1. Clone the Repository

```bash
git clone https://github.com/adityawahyuuu/AssetManagement-API.git
cd AssetManagement-API
```

### 2. Configure Environment Variables

Create a `.env` file in the `API` directory for local development:

```bash
cd API
cp .env.example .env
```

Edit the `.env` file with your local configuration:

```env
# Database Configuration
ConnectionStrings__AssetManagementConnection=Host=localhost;Database=asset_management;Username=youruser;Password=yourpassword

# JWT Configuration (generate a secure random string, min 32 characters)
Jwt__Secret=YourSecureRandomString32CharactersOrMore

# Email Configuration (Gmail - use App Password)
Email__SenderEmail=your-email@gmail.com
Email__Username=your-email@gmail.com
Email__Password=your-gmail-app-password

# Swagger UI (optional)
Swagger__Username=admin
Swagger__Password=admin123
```

**Important:**
- ✅ `.env` is in `.gitignore` - your secrets are safe
- ✅ Never commit the `.env` file to version control
- ✅ Use `.env.example` as a template for team members
- ✅ For Gmail, create an [App Password](https://support.google.com/accounts/answer/185833)

### 3. Run Migrations

```bash
cd API
dotnet ef database update
```

### 4. Trust the HTTPS Development Certificate

For local HTTPS testing, trust the ASP.NET Core development certificate:

```bash
dotnet dev-certs https --trust
```

This allows your browser and tools to connect to the API over HTTPS without security warnings.

### 5. Run the Application

```bash
dotnet run --launch-profile https
```

The API will be available at:
- **HTTPS**: `https://localhost:7146` (recommended)
- **HTTP**: `http://localhost:5080` (will redirect to HTTPS)

**Note**: The application is configured to use HTTPS by default for security.

## Database Migration Guide

### Overview

Database migrations are version-controlled changes to your database schema. This project supports two migration strategies:

1. **Entity Framework Core Migrations** - Code-first approach (recommended for new changes)
2. **SQL Migrations** - Direct SQL scripts (used for existing migrations)

### Quick Commands

```bash
# Create a new migration
dotnet ef migrations add MigrationName --project API

# Apply all pending migrations
dotnet ef database update --project API

# Rollback to previous migration
dotnet ef database update PreviousMigrationName --project API

# Generate SQL script without applying
dotnet ef migrations script --project API

# List all migrations
dotnet ef migrations list --project API
```

### SQL Migration Files

Located in `migrations/` directory:
- `database_schema.sql` - Initial database schema
- `001_drop_otp_fk_constraint.sql` - OTP foreign key constraint modification
- `002_create_password_reset_tokens.sql` - Password reset functionality
- `003_grant_permissions_password_reset_tokens.sql` - Permission configuration
- `004_create_additional_tables.sql` - Additional tables if needed

### Migration Best Practices

1. **Always backup** your database before running migrations
2. **Test migrations** in a development environment first
3. **Review the SQL** before executing
4. **Keep a log** of which migrations have been applied
5. **Never modify** migration files after they've been applied

## API Documentation

For complete API documentation, including:
- Swagger UI setup and authentication
- Testing endpoints with Swagger
- Complete API endpoints reference
- Client-side integration examples (JavaScript, React, Axios)
- Response format documentation
- API security best practices
- Common API patterns and error handling

See **[API/README.md](./API/README.md)** for detailed information.

---

## Deployment

### Supabase PostgreSQL Deployment

#### Why Supabase?

| Feature | Supabase | Render PostgreSQL |
|---------|----------|-------------------|
| **Free Tier** | 500 MB, unlimited API requests | 90-day expiration |
| **Connection Pooling** | Built-in PgBouncer | Not included in free |
| **Backups** | Automatic (Pro+) | Manual |
| **Database UI** | Full-featured SQL editor | Limited |
| **Real-time** | Built-in capabilities | Not available |

#### Quick Start (15 Minutes)

##### Step 1: Create Supabase Project

1. Go to [supabase.com](https://supabase.com)
2. Click "New Project"
3. Fill in project details and choose region
4. Wait 2-3 minutes for provisioning

##### Step 2: Get Connection String

```
Dashboard → Settings → Database → Connection String
```

Two options:
- **Pooled (Recommended for API)**: `...pooler.supabase.com:6543/...`
- **Direct (For migrations)**: `...pooler.supabase.com:5432/...`

##### Step 3: Run Migrations

```bash
export SUPABASE_DB_URL="postgresql://postgres.xxxxx:password@aws-0-region.pooler.supabase.com:5432/postgres"

# Create schema
psql $SUPABASE_DB_URL -c "CREATE SCHEMA IF NOT EXISTS kosan;"

# Apply migrations
psql $SUPABASE_DB_URL -f migrations/database_schema.sql
psql $SUPABASE_DB_URL -f migrations/001_drop_otp_fk_constraint.sql
psql $SUPABASE_DB_URL -f migrations/002_create_password_reset_tokens.sql
psql $SUPABASE_DB_URL -f migrations/003_grant_permissions_password_reset_tokens.sql
```

##### Step 4: Deploy API to Render

Reference the "Deploying to Render (API Only)" section above. Use the following environment variables:

```env
ConnectionStrings__AssetManagementConnection=postgresql://postgres.xxxxx:password@aws-0-region.pooler.supabase.com:6543/postgres?sslmode=require&application_name=AssetAPI
Database__SchemaName=kosan
Jwt__Secret=your-secure-jwt-secret-32-chars-minimum
Jwt__Issuer=AssetManagementAPI
Jwt__Audience=AssetManagementClient
Email__SenderEmail=your-email@gmail.com
Email__Username=your-email@gmail.com
Email__Password=your-app-password
Cors__AllowedOrigins=https://your-frontend.com,https://www.your-frontend.com
```

Set these in your Render Web Service environment variables (see Deploying to Render section).

#### Connection String Details

**Pooled Connection (Production):**
```
postgresql://postgres.PROJECT_REF:PASSWORD@aws-0-REGION.pooler.supabase.com:6543/postgres
```

**Features:**
- Routed through PgBouncer connection pooler
- 3000+ concurrent connections
- Transaction pooling mode
- Best for: Production APIs, high-traffic applications

**Direct Connection (Migrations):**
```
postgresql://postgres.PROJECT_REF:PASSWORD@aws-0-REGION.pooler.supabase.com:5432/postgres
```

**Features:**
- Direct PostgreSQL connection
- Lower latency
- Limited concurrent connections (60-500 depending on plan)
- Best for: Database migrations, admin tasks

#### Connection String Parameters

```
?sslmode=require
  • Enforce SSL/TLS encryption

?application_name=AssetAPI
  • Identify app in database logs

&search_path=kosan,public
  • Set default schema search path

&connect_timeout=10
  • Connection timeout (seconds)

&statement_timeout=30000
  • Query timeout (milliseconds)
```

#### Cost Comparison

| Plan | Price | Storage | Connections | Best For |
|------|-------|---------|-------------|----------|
| **Free** | $0/month | 500 MB | 60 direct, 3000 pooled | MVP, Development |
| **Pro** | $25/month | 8 GB | 500 direct, 10000 pooled | Production, Small teams |
| **Team** | $599/month | Unlimited | Unlimited | Enterprise |

#### Performance Optimization

1. **Use Pooled Connections** in production (port 6543, not 5432)
2. **Create Indexes** for common queries:
   ```sql
   CREATE INDEX idx_assets_user_id ON kosan.assets(user_id);
   CREATE INDEX idx_rooms_user_id ON kosan.rooms(user_id);
   ```
3. **Enable Caching** for frequently accessed data
4. **Use Pagination** for large datasets
5. **Monitor Connection Usage** via dashboard

#### Connection Pooling Configuration

```csharp
// Program.cs
services.AddDbContext<AssetManagementDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30));

        npgsqlOptions.CommandTimeout(30);
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "kosan");
    })
);
```

#### Monitoring

```
Dashboard → Database → Database Health

Monitor:
• CPU usage
• Memory usage
• Connection count
• Query performance
```

Check active connections:
```sql
SELECT count(*) FROM pg_stat_activity;
```

#### Common Issues

**"Too Many Connections"**
- Switch to pooled connection (port 6543)
- Reduce application pool size
- Upgrade to Pro plan

**Slow Queries**
- Add missing indexes
- Implement query pagination
- Enable caching
- Optimize N+1 queries

**Connection Timeouts**
- Increase timeout in connection string
- Check firewall/network connectivity
- Verify SSL configuration

#### Backup Strategy

```bash
# Manual backup
pg_dump $SUPABASE_DB_URL > backup_$(date +%Y%m%d).sql

# Compressed backup
pg_dump $SUPABASE_DB_URL | gzip > backup_$(date +%Y%m%d).sql.gz

# Restore backup
psql $SUPABASE_DB_URL < backup_20231115.sql
```

#### Security Best Practices

1. **Never commit connection strings** - Use environment variables
2. **Use separate databases** for dev/staging/production
3. **Rotate passwords regularly**
   ```
   Dashboard → Settings → Database → Reset Database Password
   ```
4. **Enable SSL** (enforce in connection string)
5. **Implement Row-Level Security** (RLS)
6. **Backup regularly** (Pro plan or manual)

#### Resources

- [Supabase Documentation](https://supabase.com/docs)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)

### Deploying to Render (API Only)

#### Step 1: Create Web Service on Render

1. Go to [Render Dashboard](https://dashboard.render.com/)
2. Click "New +" → "Web Service"
3. Connect your GitHub repository
4. Configure the service:
   - **Name**: `asset-management-api`
   - **Region**: Same as your database
   - **Branch**: `main` (or your deployment branch)
   - **Root Directory**: `API`
   - **Runtime**: `.NET`
   - **Build Command**: `dotnet publish -c Release -o out`
   - **Start Command**: `cd out && dotnet API.dll`

#### Step 2: Configure Environment Variables

Add the following environment variables in Render Dashboard:

| Variable Name | Description | Example Value |
|--------------|-------------|---------------|
| `ASPNETCORE_ENVIRONMENT` | Application environment | `Production` |
| `ConnectionStrings__AssetManagementConnection` | PostgreSQL connection string | `postgresql://user:pass@host/db` |
| `Jwt__Secret` | JWT signing secret (min 32 chars) | `YourSecureRandomString32CharsOrMore` |
| `Jwt__Issuer` | JWT issuer name | `AssetManagementAPI` |
| `Jwt__Audience` | JWT audience | `AssetManagementClient` |
| `Email__SenderEmail` | SMTP sender email | `your-email@gmail.com` |
| `Email__Username` | SMTP username | `your-email@gmail.com` |
| `Email__Password` | SMTP password/app password | `your-app-password` |
| `Cors__AllowedOrigins` | Allowed frontend URLs | `https://your-frontend.com,https://www.your-frontend.com` |

**Important Notes:**
- Use double underscores (`__`) to represent nested configuration keys
- For Gmail, use an [App Password](https://support.google.com/accounts/answer/185833)
- Add all your frontend URLs to CORS allowed origins, separated by commas
- Keep your JWT secret secure and never commit it to version control

#### Step 3: Deploy

1. Click "Create Web Service"
2. Render will automatically build and deploy your application
3. Wait for deployment to complete (5-10 minutes)
4. Your API will be available at `https://your-service-name.onrender.com`

**HTTPS Configuration:**
- Render automatically provides free SSL/TLS certificates for all web services
- Your API is automatically accessible via HTTPS
- The application is configured with forwarded headers middleware to handle HTTPS
- HTTP requests are automatically redirected to HTTPS
- No additional HTTPS configuration needed

#### Step 4: Initialize Database

After first deployment, run migrations from your local machine with production database connection string:

```bash
dotnet ef database update --connection "your-production-connection-string" --project API
```

#### Environment Variables Reference

Reference the configuration keys from `API/appsettings.Production.json`:

##### Required Variables

- **ConnectionStrings__AssetManagementConnection**: PostgreSQL connection string
- **Jwt__Secret**: Secret key for JWT token signing (minimum 32 characters)
- **Email__SenderEmail**: Email address for sending notifications
- **Email__Username**: SMTP username
- **Email__Password**: SMTP password
- **Cors__AllowedOrigins**: Comma-separated list of allowed frontend URLs

##### Optional Variables

- **Jwt__Issuer**: JWT issuer (default: `AssetManagementAPI`)
- **Jwt__Audience**: JWT audience (default: `AssetManagementClient`)
- **Jwt__ExpirationMinutes**: Token expiration time (default: `1440`)
- **Email__SmtpHost**: SMTP server (default: `smtp.gmail.com`)
- **Email__SmtpPort**: SMTP port (default: `587`)
- **Email__SenderName**: Sender name (default: `Asset Management System`)
- **Swagger__Username**: Swagger UI username
- **Swagger__Password**: Swagger UI password
- **Swagger__AuthEnabled**: Enable Swagger UI basic auth (default: `true` in production)

---

## Contributors

- Aditya Wahyu ([@adityawahyuuu](https://github.com/adityawahyuuu))

---
