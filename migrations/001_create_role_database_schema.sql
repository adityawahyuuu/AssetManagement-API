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
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    LOCALE_PROVIDER = 'libc'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;

-- SCHEMA: kosan

-- DROP SCHEMA IF EXISTS kosan ;

CREATE SCHEMA IF NOT EXISTS kosan
    AUTHORIZATION developer;