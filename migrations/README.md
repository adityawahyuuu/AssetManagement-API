# Database Migrations

This directory contains SQL migration scripts for the Asset Management API database schema and Entity Framework Core migrations.

## SQL Migration Files

- `001_create_role_database_schema.sql` - Create role and kosan schema
- `002_create_user_table.sql` - Create user_logins table
- `003_create_rooms_assets_table.sql` - Create rooms and assets tables
- `004_create_master_table.sql` - Create master configuration tables

## Applying Migrations

**Using Entity Framework Core (Recommended):**
```bash
dotnet ef database update --project API
```

**Using SQL Scripts Directly:**
```bash
export DB_URL="postgresql://user:pass@host:5432/database"
psql $DB_URL -f migrations/001_create_role_database_schema.sql
psql $DB_URL -f migrations/002_create_user_table.sql
psql $DB_URL -f migrations/003_create_rooms_assets_table.sql
psql $DB_URL -f migrations/004_create_master_table.sql
```

## GitHub Actions Validation

SQL migrations are automatically validated on pull requests via `.github/workflows/db-migration-check.yml`:
- File naming convention validation (NNN_description.sql)
- SQL syntax checking
- Duplicate migration number detection
- Automated testing on PostgreSQL 16
- Breaking change analysis
