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

## Deploying to Render

### Step 1: Prepare Your Database

1. Create a PostgreSQL database on Render:
   - Go to [Render Dashboard](https://dashboard.render.com/)
   - Click "New +" → "PostgreSQL"
   - Name your database (e.g., `asset-management-db`)
   - Select a region close to your users
   - Click "Create Database"
   - Copy the **Internal Database URL** (starts with `postgresql://`)

### Step 2: Create Web Service on Render

1. Click "New +" → "Web Service"
2. Connect your GitHub repository
3. Configure the service:
   - **Name**: `asset-management-api` (or your preferred name)
   - **Region**: Same as your database
   - **Branch**: `main` (or your deployment branch)
   - **Root Directory**: `API`
   - **Runtime**: `.NET`
   - **Build Command**: `dotnet publish -c Release -o out`
   - **Start Command**: `cd out && dotnet API.dll`

### Step 3: Configure Environment Variables

Add the following environment variables in Render:

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
- The database connection string should be the Internal Database URL from Step 1
- For Gmail, use an [App Password](https://support.google.com/accounts/answer/185833)
- Add all your frontend URLs to CORS allowed origins, separated by commas
- Keep your JWT secret secure and never commit it to version control

### Step 4: Deploy

1. Click "Create Web Service"
2. Render will automatically build and deploy your application
3. Wait for the deployment to complete (5-10 minutes)
4. Your API will be available at `https://your-service-name.onrender.com`

**HTTPS Configuration:**
- ✅ Render automatically provides free SSL/TLS certificates for all web services
- ✅ Your API is automatically accessible via HTTPS
- ✅ The application is configured with forwarded headers middleware to properly handle HTTPS behind Render's reverse proxy
- ✅ HTTP requests are automatically redirected to HTTPS
- ✅ No additional HTTPS configuration needed

### Step 5: Initialize Database

After first deployment, run migrations:

1. Go to your web service in Render
2. Click "Shell" tab
3. Run:
   ```bash
   cd out
   dotnet ef database update --project /path/to/API.csproj
   ```

Alternatively, use your local machine with the production database connection string:

```bash
dotnet ef database update --connection "your-production-connection-string"
```

## Environment Variables Reference

### Required Variables

- **ConnectionStrings__AssetManagementConnection**: PostgreSQL connection string
- **Jwt__Secret**: Secret key for JWT token signing (minimum 32 characters)
- **Email__SenderEmail**: Email address for sending notifications
- **Email__Username**: SMTP username
- **Email__Password**: SMTP password
- **Cors__AllowedOrigins**: Comma-separated list of allowed frontend URLs

### Optional Variables

- **Jwt__Issuer**: JWT issuer (default: `AssetManagementAPI`)
- **Jwt__Audience**: JWT audience (default: `AssetManagementClient`)
- **Jwt__ExpirationMinutes**: Token expiration time (default: `1440` = 24 hours)
- **Email__SmtpHost**: SMTP server (default: `smtp.gmail.com`)
- **Email__SmtpPort**: SMTP port (default: `587`)
- **Email__SenderName**: Sender name (default: `Asset Management System`)
- **Swagger__Username**: Swagger UI username (default: `admin`)
- **Swagger__Password**: Swagger UI password (default: `admin123`)
- **Swagger__AuthEnabled**: Enable Swagger UI basic auth (default: `true` in production, `false` in development)

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

## Security Best Practices

1. **Use .env file for local development secrets**
   - Never commit `.env` to version control (already in `.gitignore`)
   - Use `.env.example` as a template for team members
   - Store database passwords, JWT secrets, and email credentials in `.env`

2. **Never expose sensitive environment variables** in your frontend code

3. **Store JWT tokens securely** (localStorage, sessionStorage, or httpOnly cookies)

4. **Always use HTTPS** in production

5. **Implement token refresh** logic for better security

6. **Handle token expiration** gracefully on the client side

7. **Validate CORS origins** carefully in production

8. **Use environment-specific configurations**:
   - Development: `.env` file (not committed)
   - Production: Environment variables on Render

9. **Enable rate limiting** if needed (consider Render's built-in features)

## Troubleshooting

### Common Issues

1. **Database Connection Errors**
   - Verify the connection string format
   - Ensure the database is running and accessible
   - Check network connectivity between services

2. **CORS Errors**
   - Add your frontend URL to `Cors__AllowedOrigins`
   - Ensure the URL format matches exactly (including protocol and port)
   - Check browser console for specific CORS error messages

3. **JWT Authentication Failures**
   - Verify the JWT secret is at least 32 characters
   - Ensure the token is sent with the `Bearer` prefix
   - Check token expiration time
   - Verify the token is being sent in the Authorization header

4. **Email Not Sending**
   - For Gmail, use an App Password, not your regular password
   - Enable "Less secure app access" or use OAuth2
   - Check SMTP settings and credentials
   - Verify email service is not blocked by firewall

5. **Migration Issues**
   - Ensure database connection is correct
   - Check if migrations folder exists
   - Run migrations manually if needed
   - Verify Entity Framework tools are installed

## Development Tools

### Running Tests

```bash
cd API.Test
dotnet test
```

### Database Migrations

Create a new migration:
```bash
dotnet ef migrations add MigrationName
```

Apply migrations:
```bash
dotnet ef database update
```

Rollback migration:
```bash
dotnet ef database update PreviousMigrationName
```

### Code Formatting

```bash
dotnet format
```

## Support

For issues, questions, or contributions:

- **GitHub Issues**: [Create an issue](https://github.com/adityawahyuuu/AssetManagement-API/issues)
- **Documentation**: Check this README and Swagger documentation
- **Email**: Contact your system administrator

## License

[Your License Here]

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

### Running SQL Migrations

```bash
export DATABASE_URL="postgresql://username:password@host:5432/database"

# Apply schema
psql $DATABASE_URL -f migrations/database_schema.sql
psql $DATABASE_URL -f migrations/001_drop_otp_fk_constraint.sql
psql $DATABASE_URL -f migrations/002_create_password_reset_tokens.sql
psql $DATABASE_URL -f migrations/003_grant_permissions_password_reset_tokens.sql
```

### Migration Best Practices

1. **Always backup before migrating**
2. **Test in non-production first** (Local → Dev → Staging → Production)
3. **Use transactions** for safe rollback capability
4. **Make migrations idempotent** (safe to run multiple times)
5. **Document breaking changes** clearly
6. **Keep migrations version controlled**
7. **Monitor migration execution**

### Production Migration Checklist

- [ ] Backup database before migration
- [ ] Test migration in staging environment
- [ ] Review generated SQL
- [ ] Plan rollback procedure
- [ ] Schedule during low-traffic window
- [ ] Monitor database performance
- [ ] Verify migration success
- [ ] Keep backup for 7-30 days

---

## Supabase PostgreSQL Deployment

### Why Supabase?

| Feature | Supabase | Render PostgreSQL |
|---------|----------|-------------------|
| **Free Tier** | 500 MB, unlimited API requests | 90-day expiration |
| **Connection Pooling** | Built-in PgBouncer | Not included in free |
| **Backups** | Automatic (Pro+) | Manual |
| **Database UI** | Full-featured SQL editor | Limited |
| **Real-time** | Built-in capabilities | Not available |

### Quick Start (15 Minutes)

#### Step 1: Create Supabase Project

1. Go to [supabase.com](https://supabase.com)
2. Click "New Project"
3. Fill in project details and choose region
4. Wait 2-3 minutes for provisioning

#### Step 2: Get Connection String

```
Dashboard → Settings → Database → Connection String
```

Two options:
- **Pooled (Recommended for API)**: `...pooler.supabase.com:6543/...`
- **Direct (For migrations)**: `...pooler.supabase.com:5432/...`

#### Step 3: Run Migrations

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

#### Step 4: Configure API

Add these environment variables to your deployment:

```env
ConnectionStrings__AssetManagementConnection=postgresql://postgres.xxxxx:password@aws-0-region.pooler.supabase.com:6543/postgres?sslmode=require&application_name=AssetAPI
Database__SchemaName=kosan
Jwt__Secret=your-secure-jwt-secret-32-chars-minimum
Email__SenderEmail=your-email@gmail.com
```

### Connection String Details

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

### Connection String Parameters

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

### Cost Comparison

| Plan | Price | Storage | Connections | Best For |
|------|-------|---------|-------------|----------|
| **Free** | $0/month | 500 MB | 60 direct, 3000 pooled | MVP, Development |
| **Pro** | $25/month | 8 GB | 500 direct, 10000 pooled | Production, Small teams |
| **Team** | $599/month | Unlimited | Unlimited | Enterprise |

### Performance Optimization

1. **Use Pooled Connections** in production (port 6543, not 5432)
2. **Create Indexes** for common queries:
   ```sql
   CREATE INDEX idx_assets_user_id ON kosan.assets(user_id);
   CREATE INDEX idx_rooms_user_id ON kosan.rooms(user_id);
   ```
3. **Enable Caching** for frequently accessed data
4. **Use Pagination** for large datasets
5. **Monitor Connection Usage** via dashboard

### Connection Pooling Configuration

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

### Monitoring

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

### Common Issues

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

### Backup Strategy

```bash
# Manual backup
pg_dump $SUPABASE_DB_URL > backup_$(date +%Y%m%d).sql

# Compressed backup
pg_dump $SUPABASE_DB_URL | gzip > backup_$(date +%Y%m%d).sql.gz

# Restore backup
psql $SUPABASE_DB_URL < backup_20231115.sql
```

### Security Best Practices

1. **Never commit connection strings** - Use environment variables
2. **Use separate databases** for dev/staging/production
3. **Rotate passwords regularly**
   ```
   Dashboard → Settings → Database → Reset Database Password
   ```
4. **Enable SSL** (enforce in connection string)
5. **Implement Row-Level Security** (RLS)
6. **Backup regularly** (Pro plan or manual)

### Resources

- [Supabase Documentation](https://supabase.com/docs)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)

---

## Contributors

- Aditya Wahyu ([@adityawahyuuu](https://github.com/adityawahyuuu))

---

**Happy Coding!** 🚀
