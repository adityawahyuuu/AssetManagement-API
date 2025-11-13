-- Sequence: kosan.asset_categories_id_seq

-- DROP SEQUENCE IF EXISTS kosan.asset_categories_id_seq;

CREATE SEQUENCE IF NOT EXISTS kosan.asset_categories_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE kosan.asset_categories_id_seq OWNER to developer;

-- Table: kosan.asset_categories

-- DROP TABLE IF EXISTS kosan.asset_categories;

CREATE TABLE IF NOT EXISTS kosan.asset_categories
(
    id integer NOT NULL DEFAULT nextval('kosan.asset_categories_id_seq'::regclass),
    name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    description text COLLATE pg_catalog."default",
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT asset_categories_pkey PRIMARY KEY (id),
    CONSTRAINT asset_categories_name_key UNIQUE (name)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS kosan.asset_categories
    OWNER to developer;

-- Index: idx_asset_categories_name

-- DROP INDEX IF EXISTS kosan.idx_asset_categories_name;

CREATE INDEX IF NOT EXISTS idx_asset_categories_name
    ON kosan.asset_categories USING btree
    (name COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;

-- Insert default asset categories
INSERT INTO kosan.asset_categories (name, description)
VALUES
    ('tempat_tidur', 'Furniture for sleeping - beds, mattresses, pillows'),
    ('meja', 'Tables and desks - study tables, dining tables, work desks'),
    ('lemari', 'Storage furniture - wardrobes, cabinets, shelves'),
    ('kursi', 'Chairs and seating - office chairs, dining chairs, stools'),
    ('lainnya', 'Other assets and miscellaneous items')
ON CONFLICT (name) DO NOTHING;
