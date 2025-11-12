-- Table: kosan.rooms

-- DROP TABLE IF EXISTS kosan.rooms;

CREATE TABLE IF NOT EXISTS kosan.rooms
(
    id integer NOT NULL DEFAULT nextval('kosan.rooms_id_seq'::regclass),
    user_id integer NOT NULL,
    name character varying(255) COLLATE pg_catalog."default" NOT NULL,
    length_m numeric(5,2) NOT NULL,
    width_m numeric(5,2) NOT NULL,
    door_position character varying(20) COLLATE pg_catalog."default",
    door_width_cm integer,
    window_position character varying(20) COLLATE pg_catalog."default",
    window_width_cm integer,
    power_outlet_positions text[] COLLATE pg_catalog."default",
    photo_url text COLLATE pg_catalog."default",
    notes text COLLATE pg_catalog."default",
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT rooms_pkey PRIMARY KEY (id),
    CONSTRAINT fk_rooms_user FOREIGN KEY (user_id)
        REFERENCES kosan.user_login (userid) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT rooms_door_position_check CHECK (door_position::text = ANY (ARRAY['north'::character varying, 'south'::character varying, 'east'::character varying, 'west'::character varying]::text[])),
    CONSTRAINT rooms_window_position_check CHECK (window_position::text = ANY (ARRAY['north'::character varying, 'south'::character varying, 'east'::character varying, 'west'::character varying]::text[]))
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS kosan.rooms
    OWNER to developer;
	
-- Index: idx_rooms_user_created

-- DROP INDEX IF EXISTS kosan.idx_rooms_user_created;

CREATE INDEX IF NOT EXISTS idx_rooms_user_created
    ON kosan.rooms USING btree
    (user_id ASC NULLS LAST, created_at DESC NULLS FIRST)
    TABLESPACE pg_default;
-- Index: idx_rooms_user_id

-- DROP INDEX IF EXISTS kosan.idx_rooms_user_id;

CREATE INDEX IF NOT EXISTS idx_rooms_user_id
    ON kosan.rooms USING btree
    (user_id ASC NULLS LAST)
    TABLESPACE pg_default;




-- Table: kosan.assets

-- DROP TABLE IF EXISTS kosan.assets;

CREATE TABLE IF NOT EXISTS kosan.assets
(
    id integer NOT NULL DEFAULT nextval('kosan.assets_id_seq'::regclass),
    room_id integer NOT NULL,
    user_id integer NOT NULL,
    name character varying(255) COLLATE pg_catalog."default" NOT NULL,
    category character varying(50) COLLATE pg_catalog."default",
    photo_url text COLLATE pg_catalog."default",
    length_cm integer NOT NULL,
    width_cm integer NOT NULL,
    height_cm integer NOT NULL,
    clearance_front_cm integer DEFAULT 0,
    clearance_sides_cm integer DEFAULT 0,
    clearance_back_cm integer DEFAULT 0,
    function_zone character varying(50) COLLATE pg_catalog."default",
    must_be_near_wall boolean DEFAULT false,
    must_be_near_window boolean DEFAULT false,
    must_be_near_outlet boolean DEFAULT false,
    can_rotate boolean DEFAULT true,
    cannot_adjacent_to integer[],
    purchase_date date,
    purchase_price numeric(12,2),
    condition character varying(50) COLLATE pg_catalog."default",
    notes text COLLATE pg_catalog."default",
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT assets_pkey PRIMARY KEY (id),
    CONSTRAINT fk_assets_room FOREIGN KEY (room_id)
        REFERENCES kosan.rooms (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT fk_assets_user FOREIGN KEY (user_id)
        REFERENCES kosan.user_login (userid) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT assets_category_check CHECK (category::text = ANY (ARRAY['tempat_tidur'::character varying, 'meja'::character varying, 'lemari'::character varying, 'kursi'::character varying, 'lainnya'::character varying]::text[])),
    CONSTRAINT assets_function_zone_check CHECK (function_zone::text = ANY (ARRAY['sleeping'::character varying, 'study'::character varying, 'storage'::character varying, 'leisure'::character varying]::text[])),
    CONSTRAINT assets_condition_check CHECK (condition::text = ANY (ARRAY['new'::character varying, 'good'::character varying, 'fair'::character varying, 'needs_repair'::character varying]::text[]))
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS kosan.assets
    OWNER to developer;

-- Index: idx_assets_name_search

-- DROP INDEX IF EXISTS kosan.idx_assets_name_search;

CREATE INDEX IF NOT EXISTS idx_assets_name_search
    ON kosan.assets USING gin
    (to_tsvector('english'::regconfig, name::text))
    TABLESPACE pg_default;
-- Index: idx_assets_room_category

-- DROP INDEX IF EXISTS kosan.idx_assets_room_category;

CREATE INDEX IF NOT EXISTS idx_assets_room_category
    ON kosan.assets USING btree
    (room_id ASC NULLS LAST, category COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx_assets_room_created

-- DROP INDEX IF EXISTS kosan.idx_assets_room_created;

CREATE INDEX IF NOT EXISTS idx_assets_room_created
    ON kosan.assets USING btree
    (room_id ASC NULLS LAST, created_at DESC NULLS FIRST)
    TABLESPACE pg_default;
-- Index: idx_assets_room_id

-- DROP INDEX IF EXISTS kosan.idx_assets_room_id;

CREATE INDEX IF NOT EXISTS idx_assets_room_id
    ON kosan.assets USING btree
    (room_id ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx_assets_user_id

-- DROP INDEX IF EXISTS kosan.idx_assets_user_id;

CREATE INDEX IF NOT EXISTS idx_assets_user_id
    ON kosan.assets USING btree
    (user_id ASC NULLS LAST)
    TABLESPACE pg_default;