-- Role: developer
-- DROP ROLE IF EXISTS developer;

CREATE ROLE developer WITH
  LOGIN
  NOSUPERUSER
  INHERIT
  CREATEDB
  NOCREATEROLE
  NOREPLICATION
  NOBYPASSRLS
  ENCRYPTED PASSWORD 'SCRAM-SHA-256$4096:p4rIjSQfLtCo8vxDGEsX5w==$j4djYTOqYj2mldnLwnNrq+uZ3UQCBcGdEYDMHTt4ekk=:biG6OJIRqwCfJrhWmfaYs0uosrt3sODFb+sBIBEBYUY=';
  
-- Database: asset_management

-- DROP DATABASE IF EXISTS asset_management;

CREATE DATABASE asset_management
    WITH
    OWNER = developer
    ENCODING = 'UTF8'
    LC_COLLATE = 'en-US'
    LC_CTYPE = 'en-US'
    LOCALE_PROVIDER = 'libc'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;

-- SCHEMA: kosan

-- DROP SCHEMA IF EXISTS kosan ;

CREATE SCHEMA IF NOT EXISTS kosan
    AUTHORIZATION developer;