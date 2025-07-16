-- ShelfKeeper Database Schema
-- Version 1.0

-- ========= Extensions =========
-- Aktiviert die `uuid-ossp`-Erweiterung, um `uuid_generate_v4()` nutzen zu können.
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ========= Triggers and Functions =========
-- Funktion, die `created_at` und `last_updated_at` beim Erstellen eines neuen Eintrags setzt.
CREATE OR REPLACE FUNCTION set_initial_timestamps()
RETURNS TRIGGER AS $
BEGIN
    NEW.created_at := NOW();
    NEW.last_updated_at := NOW();
    RETURN NEW;
END;
$ language 'plpgsql';

-- Funktion, die `last_updated_at` bei jeder Aktualisierung eines Eintrags aktualisiert.
CREATE OR REPLACE FUNCTION update_last_updated_at()
RETURNS TRIGGER AS $
BEGIN
    NEW.last_updated_at := NOW();
    RETURN NEW;
END;
$ language 'plpgsql';

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