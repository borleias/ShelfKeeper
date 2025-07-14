-- ShelfKeeper Database Schema
-- Version 1.0

-- ========= Extensions =========
-- Aktiviert die `uuid-ossp`-Erweiterung, um `uuid_generate_v4()` nutzen zu können.
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ========= Triggers and Functions =========
-- Funktion, die `created_at` und `last_updated_at` beim Erstellen eines neuen Eintrags setzt.
CREATE OR REPLACE FUNCTION set_initial_timestamps()
RETURNS TRIGGER AS $$
BEGIN
    NEW.created_at := NOW();
    NEW.last_updated_at := NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Funktion, die `last_updated_at` bei jeder Aktualisierung eines Eintrags aktualisiert.
CREATE OR REPLACE FUNCTION update_last_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.last_updated_at := NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- ========= Tables =========

-- Users Table: Speichert die Benutzerkonten.
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    name VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    last_updated_at TIMESTAMPTZ NOT NULL
);

-- Authors Table: Speichert Autoren/Künstler, um Redundanz zu vermeiden.
CREATE TABLE authors (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) UNIQUE NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    last_updated_at TIMESTAMPTZ NOT NULL
);

-- Locations Table: Speichert die physischen Standorte der Medien für einen Benutzer.
CREATE TABLE locations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id),
    title VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMPTZ NOT NULL,
    last_updated_at TIMESTAMPTZ NOT NULL,
    UNIQUE(user_id, title) -- Ein Benutzer kann einen Standortnamen nicht doppelt vergeben.
);

-- MediaItems Table: Das Herzstück der Anwendung, speichert die Metadaten der Medien.
CREATE TABLE media_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id),
    title VARCHAR(255) NOT NULL,
    type VARCHAR(50) NOT NULL, -- z.B. 'Book', 'CD', 'DVD'
    year INT,
    isbn_upc VARCHAR(50),
    notes TEXT,
    progress VARCHAR(100),
    added_at TIMESTAMPTZ DEFAULT NOW(),
    location_id UUID REFERENCES locations(id),
    author_id UUID REFERENCES authors(id),
    created_at TIMESTAMPTZ NOT NULL,
    last_updated_at TIMESTAMPTZ NOT NULL
);

-- MediaImages Table: Speichert URLs zu Cover-Bildern.
CREATE TABLE media_images (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    media_item_id UUID NOT NULL REFERENCES media_items(id),
    image_url TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    last_updated_at TIMESTAMPTZ NOT NULL
);

-- MediaTags Table: Speichert die vom Benutzer definierten Tags.
CREATE TABLE media_tags (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id),
    name VARCHAR(50) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    last_updated_at TIMESTAMPTZ NOT NULL,
    UNIQUE(user_id, name) -- Ein Benutzer kann ein Tag nicht doppelt anlegen.
);

-- MediaItemTags Table: Verknüpfungstabelle für die n:m-Beziehung zwischen Medien und Tags.
CREATE TABLE media_item_tags (
    media_item_id UUID NOT NULL REFERENCES media_items(id),
    media_tag_id UUID NOT NULL REFERENCES media_tags(id),
    PRIMARY KEY (media_item_id, media_tag_id)
);

-- Subscriptions Table: Speichert die Abonnement-Daten der Benutzer.
CREATE TABLE subscriptions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id),
    plan VARCHAR(50) NOT NULL, -- z.B. 'Free', 'Plus', 'Premium'
    start_time TIMESTAMPTZ NOT NULL,
    end_time TIMESTAMPTZ NOT NULL,
    auto_renew BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL,
    last_updated_at TIMESTAMPTZ NOT NULL
);

-- ========= Indexes =========
-- Indizes zur Beschleunigung von Abfragen, die nach Benutzer filtern.
CREATE INDEX idx_locations_user_id ON locations(user_id);
CREATE INDEX idx_media_items_user_id ON media_items(user_id);
CREATE INDEX idx_media_tags_user_id ON media_tags(user_id);
CREATE INDEX idx_subscriptions_user_id ON subscriptions(user_id);
CREATE INDEX idx_media_images_media_item_id ON media_images(media_item_id);

-- ========= Attach Triggers =========
-- Hängt die Trigger an die Tabellen an, um die Zeitstempel automatisch zu verwalten.

-- Users
CREATE TRIGGER set_timestamps_users
BEFORE INSERT ON users
FOR EACH ROW
EXECUTE PROCEDURE set_initial_timestamps();

CREATE TRIGGER update_timestamp_users
BEFORE UPDATE ON users
FOR EACH ROW
EXECUTE PROCEDURE update_last_updated_at();

-- Authors
CREATE TRIGGER set_timestamps_authors
BEFORE INSERT ON authors
FOR EACH ROW
EXECUTE PROCEDURE set_initial_timestamps();

CREATE TRIGGER update_timestamp_authors
BEFORE UPDATE ON authors
FOR EACH ROW
EXECUTE PROCEDURE update_last_updated_at();

-- Locations
CREATE TRIGGER set_timestamps_locations
BEFORE INSERT ON locations
FOR EACH ROW
EXECUTE PROCEDURE set_initial_timestamps();

CREATE TRIGGER update_timestamp_locations
BEFORE UPDATE ON locations
FOR EACH ROW
EXECUTE PROCEDURE update_last_updated_at();

-- MediaItems
CREATE TRIGGER set_timestamps_media_items
BEFORE INSERT ON media_items
FOR EACH ROW
EXECUTE PROCEDURE set_initial_timestamps();

CREATE TRIGGER update_timestamp_media_items
BEFORE UPDATE ON media_items
FOR EACH ROW
EXECUTE PROCEDURE update_last_updated_at();

-- MediaImages
CREATE TRIGGER set_timestamps_media_images
BEFORE INSERT ON media_images
FOR EACH ROW
EXECUTE PROCEDURE set_initial_timestamps();

CREATE TRIGGER update_timestamp_media_images
BEFORE UPDATE ON media_images
FOR EACH ROW
EXECUTE PROCEDURE update_last_updated_at();

-- MediaTags
CREATE TRIGGER set_timestamps_media_tags
BEFORE INSERT ON media_tags
FOR EACH ROW
EXECUTE PROCEDURE set_initial_timestamps();

CREATE TRIGGER update_timestamp_media_tags
BEFORE UPDATE ON media_tags
FOR EACH ROW
EXECUTE PROCEDURE update_last_updated_at();

-- Subscriptions
CREATE TRIGGER set_timestamps_subscriptions
BEFORE INSERT ON subscriptions
FOR EACH ROW
EXECUTE PROCEDURE set_initial_timestamps();

CREATE TRIGGER update_timestamp_subscriptions
BEFORE UPDATE ON subscriptions
FOR EACH ROW
EXECUTE PROCEDURE update_last_updated_at();

-- ========= Views =========
-- Eine praktische View, die alle wichtigen Informationen zu einem Medium zusammenfasst.
CREATE OR REPLACE VIEW v_media_items_full AS
SELECT
    mi.id,
    mi.user_id,
    u.name as user_name,
    mi.title,
    mi.type,
    a.name as author,
    mi.year,
    l.title as location,
    mi.notes,
    mi.progress,
    mi.added_at,
    (SELECT STRING_AGG(mt.name, ', ')
     FROM media_item_tags mit
     JOIN media_tags mt ON mit.media_tag_id = mt.id
     WHERE mit.media_item_id = mi.id) as tags
FROM
    media_items mi
JOIN
    users u ON mi.user_id = u.id
LEFT JOIN
    authors a ON mi.author_id = a.id
LEFT JOIN
    locations l ON mi.location_id = l.id;