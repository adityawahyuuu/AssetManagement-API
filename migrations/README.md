# Database Migrations

This directory contains SQL migration scripts for the Asset Management API database.

## Migration 001: Drop OTP Foreign Key Constraint

**File:** `001_drop_otp_fk_constraint.sql`

**Purpose:** Remove the foreign key constraint on `otp_codes.email` that referenced `pending_users.email`. This constraint prevented the password reset feature from working because it only allowed OTP codes for emails in the `pending_users` table, not the `user_logins` table.

**Changes:**
- Drops `fk_otp_email` constraint from `kosan.otp_codes` table
- Allows OTP codes for both user registration and password reset
- Maintains data integrity at the application level

### How to Execute

#### Option 1: Using psql Command Line

```bash
# Connect to your database
psql -h localhost -U your_username -d your_database

# Execute the migration
\i migrations/001_drop_otp_fk_constraint.sql
```

#### Option 2: Using pgAdmin

1. Open pgAdmin and connect to your database
2. Right-click on your database → Query Tool
3. Open the file `migrations/001_drop_otp_fk_constraint.sql`
4. Click Execute (F5)
5. Verify the success message appears

#### Option 3: Using Database Connection String

```bash
psql postgresql://username:password@localhost:5432/database_name -f migrations/001_drop_otp_fk_constraint.sql
```

### Verification

After running the migration, verify the constraint was removed:

```sql
SELECT constraint_name, table_name
FROM information_schema.table_constraints
WHERE constraint_name = 'fk_otp_email'
AND table_schema = 'kosan';
```

**Expected result:** No rows returned (constraint does not exist)

### Rollback (if needed)

If you need to restore the constraint (not recommended):

```sql
ALTER TABLE kosan.otp_codes
ADD CONSTRAINT fk_otp_email
FOREIGN KEY (email) REFERENCES kosan.pending_users(email)
ON DELETE CASCADE;
```

**Note:** This will prevent the forgot-password feature from working!

---

## Migration Best Practices

1. **Always backup** your database before running migrations
2. **Test migrations** in a development environment first
3. **Review the SQL** before executing
4. **Keep a log** of which migrations have been applied
5. **Never modify** migration files after they've been applied

---

## Migration Log

| Migration | Applied Date | Applied By | Notes |
|-----------|--------------|------------|-------|
| 001_drop_otp_fk_constraint.sql | YYYY-MM-DD | [Your Name] | Initial migration for password reset feature |
