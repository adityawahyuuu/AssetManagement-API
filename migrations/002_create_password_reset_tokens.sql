-- Migration: Create password_reset_tokens table and ensure proper FK constraints
-- Date: 2025-11-01
-- Reason: Separate OTP (registration) from password reset tokens
--
-- This migration:
-- 1. Ensures otp_codes FK constraint to pending_users exists (for registration)
-- 2. Creates password_reset_tokens table with FK to user_logins (for password reset)
-- 3. This allows OTP to be used exclusively for registration verification
--    while password reset uses secure tokens

-- =============================================================================
-- PART 1: Ensure otp_codes has FK constraint to pending_users
-- =============================================================================

-- Add FK constraint if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_name = 'fk_otp_email'
        AND table_schema = 'kosan'
        AND table_name = 'otp_codes'
    ) THEN
        ALTER TABLE kosan.otp_codes
        ADD CONSTRAINT fk_otp_email
        FOREIGN KEY (email)
        REFERENCES kosan.pending_users (email)
        ON DELETE CASCADE;

        RAISE NOTICE 'SUCCESS: fk_otp_email constraint added to otp_codes';
    ELSE
        RAISE NOTICE 'INFO: fk_otp_email constraint already exists';
    END IF;
END $$;

-- =============================================================================
-- PART 2: Create password_reset_tokens table
-- =============================================================================

-- Create sequence for password_reset_tokens
CREATE SEQUENCE IF NOT EXISTS kosan.password_reset_tokens_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 2147483647
    CACHE 1;

-- Create password_reset_tokens table
CREATE TABLE IF NOT EXISTS kosan.password_reset_tokens
(
    id integer NOT NULL DEFAULT nextval('kosan.password_reset_tokens_id_seq'::regclass),
    email character varying(255) COLLATE pg_catalog."default" NOT NULL,
    token character varying(255) COLLATE pg_catalog."default" NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    expires_at timestamp without time zone NOT NULL,
    is_used boolean DEFAULT false,
    CONSTRAINT password_reset_tokens_pkey PRIMARY KEY (id),
    CONSTRAINT fk_password_reset_email FOREIGN KEY (email)
        REFERENCES kosan.user_login (email) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_password_reset_email
    ON kosan.password_reset_tokens USING btree (email);

CREATE INDEX IF NOT EXISTS idx_password_reset_token
    ON kosan.password_reset_tokens USING btree (token);

-- =============================================================================
-- VERIFICATION
-- =============================================================================

DO $$
DECLARE
    v_table_exists boolean;
    v_fk_exists boolean;
    v_email_index_exists boolean;
    v_token_index_exists boolean;
BEGIN
    -- Check if table exists
    SELECT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'kosan'
        AND table_name = 'password_reset_tokens'
    ) INTO v_table_exists;

    -- Check if FK constraint exists
    SELECT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_name = 'fk_password_reset_email'
        AND table_schema = 'kosan'
        AND table_name = 'password_reset_tokens'
    ) INTO v_fk_exists;

    -- Check if email index exists
    SELECT EXISTS (
        SELECT 1 FROM pg_indexes
        WHERE schemaname = 'kosan'
        AND tablename = 'password_reset_tokens'
        AND indexname = 'idx_password_reset_email'
    ) INTO v_email_index_exists;

    -- Check if token index exists
    SELECT EXISTS (
        SELECT 1 FROM pg_indexes
        WHERE schemaname = 'kosan'
        AND tablename = 'password_reset_tokens'
        AND indexname = 'idx_password_reset_token'
    ) INTO v_token_index_exists;

    -- Report results
    IF v_table_exists AND v_fk_exists AND v_email_index_exists AND v_token_index_exists THEN
        RAISE NOTICE 'SUCCESS: password_reset_tokens table created with all constraints and indexes';
    ELSE
        IF NOT v_table_exists THEN
            RAISE EXCEPTION 'ERROR: password_reset_tokens table was not created';
        END IF;
        IF NOT v_fk_exists THEN
            RAISE EXCEPTION 'ERROR: fk_password_reset_email constraint missing';
        END IF;
        IF NOT v_email_index_exists THEN
            RAISE EXCEPTION 'ERROR: idx_password_reset_email index missing';
        END IF;
        IF NOT v_token_index_exists THEN
            RAISE EXCEPTION 'ERROR: idx_password_reset_token index missing';
        END IF;
    END IF;
END $$;

-- =============================================================================
-- SUMMARY
-- =============================================================================
-- Table created: kosan.password_reset_tokens
-- FK constraint: fk_password_reset_email (email -> user_logins.email, CASCADE DELETE)
-- Indexes: idx_password_reset_email, idx_password_reset_token
-- =============================================================================
