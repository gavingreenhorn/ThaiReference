-- Extend thai."Class" like ToneMark: Latin key + CompositeName FK + english_label + mediapath.
-- ToneMark uses columns (mark, name→CompositeName, mediapath, english_label).
-- Class already used "name" for the Latin key (low/mid/high), so rename that to "code"
-- and add "name" as the CompositeName FK (same column name as ToneMark).
--
-- Rollback:
--   UPDATE thai."Class" SET name = NULL;
--   ALTER TABLE thai."Class" DROP CONSTRAINT IF EXISTS "Class_name_fkey";
--   ALTER TABLE thai."Class" DROP COLUMN IF EXISTS mediapath;
--   ALTER TABLE thai."Class" DROP COLUMN IF EXISTS english_label;
--   ALTER TABLE thai."Class" DROP COLUMN IF EXISTS name;
--   ALTER TABLE thai."Class" RENAME COLUMN code TO name;
--   (seeded CompositeName / Word / Spelling / Transcription / Lexeme rows are left in place)

ALTER TABLE thai."Class" RENAME COLUMN name TO code;

ALTER TABLE thai."Class"
    ADD COLUMN name integer,
    ADD COLUMN mediapath text,
    ADD COLUMN english_label text;

ALTER TABLE thai."Class"
    ADD CONSTRAINT "Class_name_fkey"
    FOREIGN KEY (name) REFERENCES thai."CompositeName"(id);

CREATE UNIQUE INDEX IF NOT EXISTS uq_thai_class_code ON thai."Class"(code);

-- Ensure the three class rows exist (live DBs already have them; fresh installs may not).
INSERT INTO thai."Class"(id, code)
SELECT v.id, v.code
FROM (VALUES
    (1, 'low'),
    (2, 'mid'),
    (3, 'high')
) AS v(id, code)
WHERE NOT EXISTS (SELECT 1 FROM thai."Class" c WHERE c.code = v.code)
  AND NOT EXISTS (SELECT 1 FROM thai."Class" c WHERE c.id = v.id);

UPDATE thai."Class" c
SET code = v.code
FROM (VALUES
    (1, 'low'),
    (2, 'mid'),
    (3, 'high')
) AS v(id, code)
WHERE c.id = v.id AND c.code IS DISTINCT FROM v.code;

SELECT setval(
    pg_get_serial_sequence('thai."Class"', 'id'),
    GREATEST((SELECT COALESCE(MAX(id), 1) FROM thai."Class"), 1));

CREATE TABLE thai._class_seed_meta (
    kind text NOT NULL,
    token text,
    word_id integer,
    spelling_id integer,
    transcription_id integer,
    lexeme_id integer,
    composite_name_id integer,
    class_id integer
);

-- Atomic words: อักษร, ต่ำ, สูง (reuse กลาง if already seeded by V2).
-- Transcription no longer has phonemic (dropped in V2); only paiboon + IPA.
DO $$
DECLARE
    r record;
    tid integer;
    sid integer;
    wid integer;
    lid integer;
BEGIN
    FOR r IN
        SELECT * FROM (VALUES
            ('อักษร', 'àk-sɔ̌ɔn', 'ʔàksɔ̌ːn', 'letter',  'noun'),
            ('ต่ำ',  'dtàm',     'tam',     'low',     'adjective'),
            ('กลาง', 'glaang',   'klaːŋ',   'middle',  'noun'),
            ('สูง',  'sǔung',    'suːŋ',    'high',    'adjective')
        ) AS t(token, paiboon, ipa, gloss, pos)
    LOOP
        wid := NULL;
        sid := NULL;
        tid := NULL;
        lid := NULL;

        SELECT w.id, w.spelling_id, w.transcription
        INTO wid, sid, tid
        FROM thai."Word" w
        JOIN thai."Spelling" s ON s.id = w.spelling_id
        WHERE s.value = r.token
        ORDER BY w.id
        LIMIT 1;

        IF wid IS NULL THEN
            INSERT INTO thai."Transcription"(paiboon, "IPA")
            VALUES (r.paiboon, r.ipa)
            RETURNING id INTO tid;

            INSERT INTO thai."Spelling"(value)
            VALUES (r.token)
            RETURNING id INTO sid;

            INSERT INTO thai."Word"(spelling_id, transcription, is_primary)
            VALUES (sid, tid, true)
            RETURNING id INTO wid;

            INSERT INTO thai._class_seed_meta(kind, token, word_id, spelling_id, transcription_id)
            VALUES ('word', r.token, wid, sid, tid);
        ELSE
            INSERT INTO thai._class_seed_meta(kind, token, word_id, spelling_id, transcription_id)
            VALUES ('reuse', r.token, wid, sid, tid);
        END IF;

        SELECT l.id INTO lid
        FROM thai."Lexeme" l
        WHERE l.value = r.gloss
          AND l.part_of_speech IS NOT DISTINCT FROM r.pos
        LIMIT 1;

        IF lid IS NULL THEN
            INSERT INTO thai."Lexeme"(value, part_of_speech)
            VALUES (r.gloss, r.pos)
            RETURNING id INTO lid;

            INSERT INTO thai._class_seed_meta(kind, token, lexeme_id)
            VALUES ('lexeme', r.token, lid);
        END IF;

        IF NOT EXISTS (
            SELECT 1 FROM thai."WordLexeme" wl
            WHERE wl.word = wid AND wl.lexeme = lid
        ) THEN
            INSERT INTO thai."WordLexeme"(word, lexeme) VALUES (wid, lid);
        END IF;
    END LOOP;
END $$;

WITH w AS (
    SELECT s.value AS token, MIN(w.id) AS word_id
    FROM thai."Word" w
    JOIN thai."Spelling" s ON s.id = w.spelling_id
    WHERE s.value IN ('อักษร', 'ต่ำ', 'กลาง', 'สูง')
    GROUP BY s.value
),
classes(id, code, english_label, t1, t2) AS (
    VALUES
        (1, 'low',  'Low',  'อักษร', 'ต่ำ'),
        (2, 'mid',  'Mid',  'อักษร', 'กลาง'),
        (3, 'high', 'High', 'อักษร', 'สูง')
),
prepared AS (
    SELECT
        c.id,
        c.code,
        c.english_label,
        w1.word_id AS word1,
        w2.word_id AS word2
    FROM classes c
    JOIN w w1 ON w1.token = c.t1
    JOIN w w2 ON w2.token = c.t2
),
ins_cn AS (
    INSERT INTO thai."CompositeName"(word1, word2, word3, word4, word5)
    SELECT p.word1, p.word2, NULL, NULL, NULL
    FROM prepared p
    ORDER BY p.id
    RETURNING id
),
cn_ordered AS (
    SELECT id AS composite_name_id,
           row_number() OVER (ORDER BY id) AS rn
    FROM ins_cn
)
UPDATE thai."Class" cl
SET
    name = c.composite_name_id,
    english_label = p.english_label
FROM prepared p
JOIN cn_ordered c ON c.rn = p.id
WHERE cl.code = p.code;

INSERT INTO thai._class_seed_meta(kind, composite_name_id, class_id)
SELECT 'class', name, id FROM thai."Class" WHERE name IS NOT NULL;

REFRESH MATERIALIZED VIEW thai."WordView";
