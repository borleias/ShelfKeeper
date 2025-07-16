-- ShelfKeeper Seed Data
-- This script populates the database with initial data for testing.

DO $$
DECLARE
    -- Users
    user_alice_id UUID := uuid_generate_v4();
    user_bob_id UUID := uuid_generate_v4();

    -- Authors
    author_tolkien_id UUID := uuid_generate_v4();
    author_herbert_id UUID := uuid_generate_v4();
    author_orwell_id UUID := uuid_generate_v4();
    author_rowling_id UUID := uuid_generate_v4();
    author_king_id UUID := uuid_generate_v4();

    -- Locations (Alice)
    location_alice_living_room_id UUID := uuid_generate_v4();
    location_alice_office_id UUID := uuid_generate_v4();

    -- Locations (Bob)
    location_bob_bedroom_id UUID := uuid_generate_v4();

    -- Tags (Alice)
    tag_alice_fantasy_id UUID := uuid_generate_v4();
    tag_alice_scifi_id UUID := uuid_generate_v4();
    tag_alice_classic_id UUID := uuid_generate_v4();
    tag_alice_horror_id UUID := uuid_generate_v4();

    -- Tags (Bob)
    tag_bob_thriller_id UUID := uuid_generate_v4();

    -- MediaItems (Alice)
    media_alice_hobbit_id UUID := uuid_generate_v4();
    media_alice_dune_id UUID := uuid_generate_v4();
    media_alice_1984_id UUID := uuid_generate_v4();
    media_alice_hp1_id UUID := uuid_generate_v4();
    media_alice_it_id UUID := uuid_generate_v4();

    -- MediaItems (Bob)
    media_bob_silence_id UUID := uuid_generate_v4();

    -- Subscriptions
    subscription_alice_id UUID := uuid_generate_v4();
    subscription_bob_id UUID := uuid_generate_v4();

BEGIN

    -- 1. Users
    INSERT INTO users (id, email, password_hash, name, created_at, last_updated_at, role) VALUES
    (user_alice_id, 'alice@example.com', '$2a$11$8s4b.yY2UoM1C.vCoLd6a.jV5.Y.b.U3yL9G.D.t.W.g.H.i.J.k', 'Alice', NOW(), NOW(), 'User'),
    (user_bob_id, 'bob@example.com', '$2a$11$8s4b.yY2UoM1C.vCoLd6a.jV5.Y.b.U3yL9G.D.t.W.g.H.i.J.k', 'Bob', NOW(), NOW(), 'User'),
    (user_admin_id, 'admin@example.com', '$2a$11$8s4b.yY2UoM1C.vCoLd6a.jV5.Y.b.U3yL9G.D.t.W.g.H.i.J.k', 'Admin User', NOW(), NOW(), 'Admin');

    -- 2. Authors
    INSERT INTO authors (id, name, created_at, last_updated_at) VALUES
    (author_tolkien_id, 'J.R.R. Tolkien', NOW(), NOW()),
    (author_herbert_id, 'Frank Herbert', NOW(), NOW()),
    (author_orwell_id, 'George Orwell', NOW(), NOW()),
    (author_rowling_id, 'J.K. Rowling', NOW(), NOW()),
    (author_king_id, 'Stephen King', NOW(), NOW());

    -- 3. Locations
    INSERT INTO locations (id, user_id, title, description, created_at, last_updated_at) VALUES
    (location_alice_living_room_id, user_alice_id, 'Living Room Shelf', 'The big one next to the TV.', NOW(), NOW()),
    (location_alice_office_id, user_alice_id, 'Office Desk', 'Top right corner.', NOW(), NOW()),
    (location_bob_bedroom_id, user_bob_id, 'Bedroom Nightstand', 'Where I read before sleep.', NOW(), NOW());

    -- 4. MediaTags
    INSERT INTO media_tags (id, user_id, name, created_at, last_updated_at) VALUES
    (tag_alice_fantasy_id, user_alice_id, 'Fantasy', NOW(), NOW()),
    (tag_alice_scifi_id, user_alice_id, 'Sci-Fi', NOW(), NOW()),
    (tag_alice_classic_id, user_alice_id, 'Classic', NOW(), NOW()),
    (tag_alice_horror_id, user_alice_id, 'Horror', NOW(), NOW()),
    (tag_bob_thriller_id, user_bob_id, 'Thriller', NOW(), NOW());

    -- 5. MediaItems
    INSERT INTO media_items (id, user_id, title, type, year, author_id, location_id, notes, progress, added_at, created_at, last_updated_at) VALUES
    (media_alice_hobbit_id, user_alice_id, 'The Hobbit', 'Book', 1937, author_tolkien_id, location_alice_living_room_id, 'First edition copy.', 'Read', NOW() - INTERVAL '5 days', NOW(), NOW()),
    (media_alice_dune_id, user_alice_id, 'Dune', 'Book', 1965, author_herbert_id, location_alice_office_id, 'A true masterpiece of science fiction.', 'Page 250', NOW() - INTERVAL '3 days', NOW(), NOW()),
    (media_alice_1984_id, user_alice_id, '1984', 'Book', 1949, author_orwell_id, NULL, 'Read for school, but still relevant.', 'Finished', NOW() - INTERVAL '10 days', NOW(), NOW()),
    (media_alice_hp1_id, user_alice_id, 'Harry Potter and the Sorcerer''s Stone', 'Book', 1997, author_rowling_id, location_alice_living_room_id, 'Childhood favorite.', 'Read', NOW() - INTERVAL '2 days', NOW(), NOW()),
    (media_alice_it_id, user_alice_id, 'It', 'Book', 1986, author_king_id, location_alice_office_id, 'Scary clown stuff.', 'Not started', NOW() - INTERVAL '1 day', NOW(), NOW()),
    (media_bob_silence_id, user_bob_id, 'The Silence of the Lambs', 'Book', 1988, NULL, location_bob_bedroom_id, 'Classic thriller.', 'Read', NOW(), NOW(), NOW());

    -- 6. MediaItemTags
    INSERT INTO media_item_tags (media_item_id, media_tag_id) VALUES
    (media_alice_hobbit_id, tag_alice_fantasy_id),
    (media_alice_dune_id, tag_alice_scifi_id),
    (media_alice_1984_id, tag_alice_classic_id),
    (media_alice_1984_id, tag_alice_scifi_id),
    (media_alice_hp1_id, tag_alice_fantasy_id),
    (media_alice_it_id, tag_alice_horror_id),
    (media_bob_silence_id, tag_bob_thriller_id);

    -- 7. MediaImages
    INSERT INTO media_images (id, media_item_id, image_url, created_at, last_updated_at) VALUES
    (uuid_generate_v4(), media_alice_hobbit_id, 'http://example.com/hobbit_cover.jpg', NOW(), NOW()),
    (uuid_generate_v4(), media_alice_dune_id, 'http://example.com/dune_cover.jpg', NOW(), NOW());

    -- 8. Subscriptions
    INSERT INTO subscriptions (id, user_id, plan, start_time, end_time, auto_renew, created_at, last_updated_at) VALUES
    (subscription_alice_id, user_alice_id, 'Premium', NOW(), NOW() + INTERVAL '1 year', TRUE, NOW(), NOW()),
    (subscription_bob_id, user_bob_id, 'Free', NOW(), NOW() + INTERVAL '1 year', FALSE, NOW(), NOW());

END $$;
