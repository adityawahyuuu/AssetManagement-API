-- Sequence: kosan.users_userid_seq

-- DROP SEQUENCE IF EXISTS kosan.users_userid_seq;

CREATE SEQUENCE IF NOT EXISTS kosan.users_userid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE kosan.users_userid_seq OWNER to developer;

-- Table: kosan.user_login

-- DROP TABLE IF EXISTS kosan.user_login;

CREATE TABLE IF NOT EXISTS kosan.user_login
(
    userid integer NOT NULL DEFAULT nextval('kosan.users_userid_seq'::regclass),
    email character varying(255) COLLATE pg_catalog."default" NOT NULL,
    password_hash character varying(255) COLLATE pg_catalog."default" NOT NULL,
    username character varying(255) COLLATE pg_catalog."default",
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    is_confirmed bit(1),
    CONSTRAINT users_pkey PRIMARY KEY (userid),
    CONSTRAINT users_email_key UNIQUE (email)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS kosan.user_login
    OWNER to developer;
-- Index: idx_user_email

-- DROP INDEX IF EXISTS kosan.idx_user_email;

CREATE UNIQUE INDEX IF NOT EXISTS idx_user_email
    ON kosan.user_login USING btree
    (email COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;




-- Sequence: kosan.pending_users_id_seq

-- DROP SEQUENCE IF EXISTS kosan.pending_users_id_seq;

CREATE SEQUENCE IF NOT EXISTS kosan.pending_users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE kosan.pending_users_id_seq OWNER to developer;

-- Table: kosan.pending_users

-- DROP TABLE IF EXISTS kosan.pending_users;

CREATE TABLE IF NOT EXISTS kosan.pending_users
(
    id integer NOT NULL DEFAULT nextval('kosan.pending_users_id_seq'::regclass),
    email character varying(255) COLLATE pg_catalog."default" NOT NULL,
    username character varying(255) COLLATE pg_catalog."default" NOT NULL,
    password_hash character varying(255) COLLATE pg_catalog."default" NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    expires_at timestamp without time zone NOT NULL,
    CONSTRAINT pending_users_pkey PRIMARY KEY (id),
    CONSTRAINT pending_users_email_key UNIQUE (email)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS kosan.pending_users
    OWNER to developer;
	



-- Sequence: kosan.otp_codes_id_seq

-- DROP SEQUENCE IF EXISTS kosan.otp_codes_id_seq;

CREATE SEQUENCE IF NOT EXISTS kosan.otp_codes_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE kosan.otp_codes_id_seq OWNER to developer;

-- Table: kosan.otp_codes

-- DROP TABLE IF EXISTS kosan.otp_codes;

CREATE TABLE IF NOT EXISTS kosan.otp_codes
(
    id integer NOT NULL DEFAULT nextval('kosan.otp_codes_id_seq'::regclass),
    email character varying(255) COLLATE pg_catalog."default" NOT NULL,
    otp_code character varying(6) COLLATE pg_catalog."default" NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    expires_at timestamp without time zone NOT NULL,
    is_verified boolean DEFAULT false,
    attempts integer DEFAULT 0,
    max_attempts integer DEFAULT 5,
    CONSTRAINT otp_codes_pkey PRIMARY KEY (id),
    CONSTRAINT fk_otp_email FOREIGN KEY (email)
        REFERENCES kosan.pending_users (email) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS kosan.otp_codes
    OWNER to developer;




-- Sequence: kosan.password_reset_tokens_id_seq

-- DROP SEQUENCE IF EXISTS kosan.password_reset_tokens_id_seq;

CREATE SEQUENCE IF NOT EXISTS kosan.password_reset_tokens_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE kosan.password_reset_tokens_id_seq OWNER to developer;

-- Table: kosan.password_reset_tokens

-- DROP TABLE IF EXISTS kosan.password_reset_tokens;

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
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS kosan.password_reset_tokens
    OWNER to developer;

-- Index: idx_password_reset_email

-- DROP INDEX IF EXISTS kosan.idx_password_reset_email;

CREATE INDEX IF NOT EXISTS idx_password_reset_email
    ON kosan.password_reset_tokens USING btree
    (email COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;

-- Index: idx_password_reset_token

-- DROP INDEX IF EXISTS kosan.idx_password_reset_token;

CREATE INDEX IF NOT EXISTS idx_password_reset_token
    ON kosan.password_reset_tokens USING btree
    (token COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
