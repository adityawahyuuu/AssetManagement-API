-- Migration: Drop foreign key constraint on otp_codes.email
-- Date: 2025-01-01
-- Reason: Allow OTP codes for both pending_users (registration) and user_logins (password reset)
--
-- The original FK constraint only allowed OTPs for emails in pending_users table,
-- which prevented password reset functionality for confirmed users in user_logins table.
--
-- This change makes OTP codes flexible while maintaining data integrity at the application level.

-- Drop the foreign key constraint
ALTER TABLE kosan.otp_codes
DROP CONSTRAINT IF EXISTS fk_otp_email;

-- Verify the constraint was dropped
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_name = 'fk_otp_email'
        AND table_schema = 'kosan'
        AND table_name = 'otp_codes'
    ) THEN
        RAISE NOTICE 'SUCCESS: fk_otp_email constraint has been dropped';
    ELSE
        RAISE EXCEPTION 'ERROR: fk_otp_email constraint still exists';
    END IF;
END $$;
