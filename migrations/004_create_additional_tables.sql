-- Migration: Create additional tables for dorms, categories, and checkouts
-- Purpose: Add missing tables for complete asset management system

-- Table: dorms
-- Purpose: Store dormitory/housing information
CREATE TABLE IF NOT EXISTS kosan.dorms (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    address TEXT,
    manager VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT dorms_name_key UNIQUE (name)
);

CREATE INDEX idx_dorms_name ON kosan.dorms(name);

-- Table: asset_categories
-- Purpose: Store asset category master data (replace hardcoded categories)
CREATE TABLE IF NOT EXISTS kosan.asset_categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT asset_categories_name_key UNIQUE (name)
);

CREATE INDEX idx_asset_categories_name ON kosan.asset_categories(name);

-- Insert default categories
INSERT INTO kosan.asset_categories (name, description) VALUES
    ('tempat_tidur', 'Furnitur untuk tidur seperti kasur, ranjang'),
    ('meja', 'Meja untuk belajar atau bekerja'),
    ('lemari', 'Lemari untuk penyimpanan pakaian atau barang'),
    ('kursi', 'Kursi untuk duduk'),
    ('lainnya', 'Kategori lainnya')
ON CONFLICT (name) DO NOTHING;

-- Table: asset_checkouts
-- Purpose: Track asset checkout/return history
CREATE TABLE IF NOT EXISTS kosan.asset_checkouts (
    id SERIAL PRIMARY KEY,
    asset_id INTEGER NOT NULL,
    user_id INTEGER NOT NULL,
    checkout_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    return_date TIMESTAMP,
    status VARCHAR(20) DEFAULT 'checked-out',
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_checkouts_asset FOREIGN KEY (asset_id)
        REFERENCES kosan.assets (id)
        ON DELETE CASCADE,
    CONSTRAINT fk_checkouts_user FOREIGN KEY (user_id)
        REFERENCES kosan.user_login (userid)
        ON DELETE CASCADE,
    CONSTRAINT checkouts_status_check CHECK (status IN ('checked-out', 'returned'))
);

CREATE INDEX idx_checkouts_asset_id ON kosan.asset_checkouts(asset_id);
CREATE INDEX idx_checkouts_user_id ON kosan.asset_checkouts(user_id);
CREATE INDEX idx_checkouts_status ON kosan.asset_checkouts(status);
CREATE INDEX idx_checkouts_checkout_date ON kosan.asset_checkouts(checkout_date DESC);

-- Grant permissions
GRANT ALL ON TABLE kosan.dorms TO developer;
GRANT ALL ON TABLE kosan.dorms TO postgres;
GRANT USAGE, SELECT ON SEQUENCE kosan.dorms_id_seq TO developer;

GRANT ALL ON TABLE kosan.asset_categories TO developer;
GRANT ALL ON TABLE kosan.asset_categories TO postgres;
GRANT USAGE, SELECT ON SEQUENCE kosan.asset_categories_id_seq TO developer;

GRANT ALL ON TABLE kosan.asset_checkouts TO developer;
GRANT ALL ON TABLE kosan.asset_checkouts TO postgres;
GRANT USAGE, SELECT ON SEQUENCE kosan.asset_checkouts_id_seq TO developer;
