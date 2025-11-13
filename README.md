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

Located in `migrations/` directory. Apply in order:
- `001_create_role_database_schema.sql` - Create `developer` role, `asset_management` database, and `kosan` schema
- `002_create_user_table.sql` - Create user authentication tables (user_login, pending_users, otp_codes, password_reset_tokens)
- `003_create_rooms_assets_table.sql` - Create room and asset management tables
- `004_create_master_table.sql` - Create asset categories and master data tables

**All migrations use the `kosan` schema and must be applied in numerical order.**

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
3. Fill in project details:
   - **Project Name**: `AssetManagement`
   - Choose your preferred region
4. Wait 2-3 minutes for provisioning

##### Step 2: Configure Data API and Get Connection String

**A. Enable Data API with Kosan Schema**

1. Go to `Dashboard → Project: AssetManagement → API`
2. Click "Data API Settings"
3. Enable "Use dedicated API schema for Data API"
4. Select **Schema**: `kosan`
5. Save settings

**B. Get Connection String**

Navigate to: `Dashboard → Settings → Database → Connection String`

Two options:
- **Pooled (Recommended for API)**: `postgresql://postgres.PROJECT_REF:PASSWORD@aws-0-REGION.pooler.supabase.com:6543/postgres`
- **Direct (For migrations)**: `postgresql://postgres.PROJECT_REF:PASSWORD@aws-0-REGION.pooler.supabase.com:5432/postgres`

Both connection strings use the default `public` schema. The `kosan` schema is configured separately for Data API access.

##### Step 3: Run Migrations

```bash
export SUPABASE_DB_URL="postgresql://postgres.PROJECT_REF:PASSWORD@aws-0-REGION.pooler.supabase.com:5432/postgres"

# Apply migrations in order (use Direct connection for migrations, port 5432)
psql $SUPABASE_DB_URL -f migrations/001_create_role_database_schema.sql
psql $SUPABASE_DB_URL -f migrations/002_create_user_table.sql
psql $SUPABASE_DB_URL -f migrations/003_create_rooms_assets_table.sql
psql $SUPABASE_DB_URL -f migrations/004_create_master_table.sql
```

**Notes:**
- Replace `PROJECT_REF` and `PASSWORD` with your Supabase credentials
- Use the **Direct connection** (port 5432) for running migrations
- The `kosan` schema will be created automatically by the first migration
- Migrations should be applied in numerical order

##### Step 4: Deploy API to Render

Reference the "Deploying to Render (API Only)" section below. Use the following environment variables for your `AssetManagement` Supabase project:

```env
ConnectionStrings__AssetManagementConnection=postgresql://postgres.PROJECT_REF:PASSWORD@aws-0-REGION.pooler.supabase.com:6543/postgres?sslmode=require&application_name=AssetAPI&search_path=kosan,public
Database__SchemaName=kosan
Jwt__Secret=your-secure-jwt-secret-32-chars-minimum
Jwt__Issuer=AssetManagementAPI
Jwt__Audience=AssetManagementClient
Email__SenderEmail=your-email@gmail.com
Email__Username=your-email@gmail.com
Email__Password=your-app-password
Cors__AllowedOrigins=https://your-frontend.com,https://www.your-frontend.com
```

Replace:
- `PROJECT_REF` with your Supabase project reference (from Settings → General)
- `PASSWORD` with your database password (from Settings → Database)
- `REGION` with your database region (e.g., `ap-southeast-1`)

Set these in your Render Web Service environment variables (see "Deploying to Render (API Only)" section).

#### Connection String Details

**Pooled Connection (Production) - AssetManagement Project:**
```
postgresql://postgres.PROJECT_REF:PASSWORD@aws-0-REGION.pooler.supabase.com:6543/postgres?sslmode=require&application_name=AssetAPI&search_path=kosan,public
```

**Features:**
- Routed through PgBouncer connection pooler
- 3000+ concurrent connections
- Transaction pooling mode
- **Schema**: Searches `kosan` schema first (your application schema), then `public` (system tables)
- **sslmode=require**: Enforces SSL/TLS encryption
- **application_name**: Identifies your app in database logs
- Best for: Production APIs, high-traffic applications

**Direct Connection (Migrations) - AssetManagement Project:**
```
postgresql://postgres.PROJECT_REF:PASSWORD@aws-0-REGION.pooler.supabase.com:5432/postgres
```

**Features:**
- Direct PostgreSQL connection
- Lower latency
- Limited concurrent connections (60-500 depending on plan)
- **Required for**: Initial schema creation and migrations
- Best for: Database migrations, admin tasks, schema changes

**Schema Configuration:**
- **kosan**: Application data schema (users, rooms, assets, etc.)
- **public**: PostgreSQL system schema
- Data API is configured to use `kosan` schema exclusively

#### Connection String Parameters

```
?sslmode=require
  • Enforce SSL/TLS encryption

?application_name=AssetAPI
  • Identify app in database logs

&search_path=kosan,public
  • Set default schema search path (kosan first, then public)

&connect_timeout=10
  • Connection timeout (seconds)

&statement_timeout=30000
  • Query timeout (milliseconds)
```

#### Data API Configuration

The Supabase Data API is configured with the following settings for the `AssetManagement` project:

**Data API Settings:**
- **Enabled**: Yes
- **Dedicated API Schema**: Enabled
- **Schema**: `kosan`
- **Auto API**: Generates REST endpoints for all tables in the kosan schema

**Available Endpoints via Data API:**
- `POST /rest/v1/user_login` - User authentication
- `POST /rest/v1/rooms` - Room management
- `POST /rest/v1/assets` - Asset tracking
- `GET /rest/v1/asset_categories` - Asset categories
- And more... (Auto-generated for all kosan schema tables)

**Access Data API:**
1. Dashboard → Project: AssetManagement → API
2. Your Data API URL: `https://PROJECT_REF.supabase.co/rest/v1/`
3. Authenticate using your Supabase API key (JWT)

**Note:** The primary API uses the direct PostgreSQL connection (code-first), while the Data API provides a REST interface directly to the kosan schema.

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

**Project: AssetManagement**

```
Dashboard → Project: AssetManagement → Database → Database Health

Monitor:
• CPU usage
• Memory usage
• Connection count (pooled vs direct)
• Query performance
```

Check active connections:
```sql
SELECT count(*) FROM pg_stat_activity WHERE usename = 'postgres';
```

Check connections per schema:
```sql
SELECT schemaname, count(*) FROM pg_stat_user_tables
WHERE schemaname = 'kosan' GROUP BY schemaname;
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

1. **Never commit connection strings** - Use environment variables (GitHub Secrets for CI/CD)
2. **Use separate Supabase projects** for dev/staging/production
3. **Rotate database passwords regularly** for AssetManagement project
   ```
   Dashboard → Project: AssetManagement → Settings → Database → Reset Database Password
   ```
4. **Enable SSL/TLS** in connection strings (`sslmode=require`)
5. **Protect the kosan schema** - Limit direct SQL access to authorized users only
6. **Implement Row-Level Security (RLS)** for multi-tenant access control
7. **Backup regularly** (Pro plan includes automatic backups)
8. **Monitor Data API access** - Dashboard → Project: AssetManagement → Logs
9. **Use strong JWT secrets** (minimum 32 characters)
10. **Validate and sanitize** all user inputs before inserting into kosan schema

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
