-- Migration: Grant permissions for password_reset_tokens table and sequence
-- Date: 2025-11-01
-- Reason: Fix permission denied error for developer user
--
-- Grant necessary permissions to developer user for password_reset_tokens table

-- Grant permissions on the sequence
GRANT USAGE, SELECT ON SEQUENCE kosan.password_reset_tokens_id_seq TO developer;

-- Grant permissions on the table
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE kosan.password_reset_tokens TO developer;

-- Verify permissions
DO $$
DECLARE
    v_sequence_privileges text;
    v_table_privileges text;
BEGIN
    -- Check sequence permissions
    SELECT privilege_type INTO v_sequence_privileges
    FROM information_schema.usage_privileges
    WHERE object_schema = 'kosan'
    AND object_name = 'password_reset_tokens_id_seq'
    AND grantee = 'developer'
    AND privilege_type = 'USAGE'
    LIMIT 1;

    -- Check table permissions
    SELECT privilege_type INTO v_table_privileges
    FROM information_schema.table_privileges
    WHERE table_schema = 'kosan'
    AND table_name = 'password_reset_tokens'
    AND grantee = 'developer'
    AND privilege_type = 'INSERT'
    LIMIT 1;

    IF v_sequence_privileges = 'USAGE' AND v_table_privileges = 'INSERT' THEN
        RAISE NOTICE 'SUCCESS: Permissions granted to developer user for password_reset_tokens';
    ELSE
        RAISE WARNING 'WARNING: Could not verify all permissions were granted';
    END IF;
END $$;
