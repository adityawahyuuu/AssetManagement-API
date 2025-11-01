-- Schema: kosan (or your configured schema)

-- Table: pending_users
-- Purpose: Store unverified user registration data temporarily
CREATE TABLE IF NOT EXISTS kosan.pending_users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    username VARCHAR(255) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP NOT NULL, -- Registration expires after X minutes (e.g., 30 min)
    CONSTRAINT pending_users_email_key UNIQUE (email)
);

CREATE INDEX idx_pending_users_email ON kosan.pending_users(email);
CREATE INDEX idx_pending_users_expires_at ON kosan.pending_users(expires_at);

-- Table: otp_codes
-- Purpose: Store OTP verification codes with expiration and attempt tracking
-- Note: OTP codes are used for both:
--   1. User registration (email in pending_users table)
--   2. Password reset (email in user_logins table)
-- Therefore, NO foreign key constraint is enforced on email column.
-- Email validation is handled at the application level.
CREATE TABLE IF NOT EXISTS kosan.otp_codes (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL,
    otp_code VARCHAR(6) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP NOT NULL, -- OTP expires after X minutes (e.g., 10 min)
    is_verified BOOLEAN DEFAULT FALSE,
    attempts INT DEFAULT 0, -- Track failed verification attempts
    max_attempts INT DEFAULT 5 -- Maximum allowed attempts
    -- NOTE: No FK constraint - allows OTP for both pending_users and user_logins
);

CREATE INDEX idx_otp_codes_email ON kosan.otp_codes(email);
CREATE INDEX idx_otp_codes_otp ON kosan.otp_codes(otp_code);
CREATE INDEX idx_otp_codes_expires_at ON kosan.otp_codes(expires_at);

-- Optional: Cleanup expired records periodically
-- You can create a scheduled job or stored procedure for this
CREATE OR REPLACE FUNCTION kosan.cleanup_expired_registrations()
RETURNS void AS $$
BEGIN
    -- Delete expired OTP codes
    DELETE FROM kosan.otp_codes WHERE expires_at < NOW();

    -- Delete expired pending users
    DELETE FROM kosan.pending_users WHERE expires_at < NOW();
END;
$$ LANGUAGE plpgsql;

-- Example: Create a scheduled job (requires pg_cron extension or external scheduler)
-- SELECT cron.schedule('cleanup-expired', '*/30 * * * *', 'SELECT kosan.cleanup_expired_registrations()');
