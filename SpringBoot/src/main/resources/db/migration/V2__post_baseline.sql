CREATE TABLE thai."Direction" (
    id integer NOT NULL,
    label character varying(8) NOT NULL,
    lexeme character varying(32) NOT NULL,
    composite_name_id integer NOT NULL,
    CONSTRAINT "Direction_pkey" PRIMARY KEY (id),
    CONSTRAINT "Direction_composite_name_id_fkey"
        FOREIGN KEY (composite_name_id) REFERENCES thai."CompositeName"(id)
);

CREATE UNIQUE INDEX uq_thai_direction_label ON thai."Direction"(label);
CREATE UNIQUE INDEX uq_thai_direction_composite_name ON thai."Direction"(composite_name_id);

CREATE TABLE thai._direction_seed_meta (
    kind text NOT NULL,
    token text,
    word_id integer,
    spelling_id integer,
    transcription_id integer,
    composite_name_id integer,
    direction_id integer
);

DO $$
DECLARE
    r record;
    tid integer;
    sid integer;
    wid integer;
BEGIN
    FOR r IN
        SELECT * FROM (VALUES
            ('เหนือ',     'nɨ̌a',        'nǔea',         'nɯ̌a'),
            ('ใต้',      'tâj',         'dtâi',         'taj'),
            ('ตะวันออก',  'tàwanʔɔ̀ːk',  'dtàwan-òk',    'tawanʔɔ̀ːk'),
            ('ตะวันตก',   'tàwantòk',    'dtàwan-dtòk',  'tawantok'),
            ('เฉียง',     'tɕʰǐaŋ',      'chǐang',       'tɕʰiaŋ')
        ) AS t(token, phonemic, paiboon, ipa)
    LOOP
        wid := NULL;
        sid := NULL;
        tid := NULL;

        SELECT w.id, w.spelling_id, w.transcription
        INTO wid, sid, tid
        FROM thai."Word" w
        JOIN thai."Spelling" s ON s.id = w.spelling_id
        WHERE s.value = r.token
        ORDER BY w.id
        LIMIT 1;

        IF wid IS NULL THEN
            INSERT INTO thai."Transcription"(phonemic, paiboon, "IPA")
            VALUES (r.phonemic, r.paiboon, r.ipa)
            RETURNING id INTO tid;

            INSERT INTO thai."Spelling"(value)
            VALUES (r.token)
            RETURNING id INTO sid;

            INSERT INTO thai."Word"(spelling_id, transcription, is_primary)
            VALUES (sid, tid, true)
            RETURNING id INTO wid;

            INSERT INTO thai._direction_seed_meta(kind, token, word_id, spelling_id, transcription_id)
            VALUES ('word', r.token, wid, sid, tid);
        ELSE
            INSERT INTO thai._direction_seed_meta(kind, token, word_id, spelling_id, transcription_id)
            VALUES ('reuse', r.token, wid, sid, tid);
        END IF;
    END LOOP;
END $$;

WITH w AS (
    SELECT s.value AS token, MIN(w.id) AS word_id
    FROM thai."Word" w
    JOIN thai."Spelling" s ON s.id = w.spelling_id
    WHERE s.value IN ('เหนือ', 'ใต้', 'ตะวันออก', 'ตะวันตก', 'เฉียง')
    GROUP BY s.value
),
dirs(id, label, lexeme, t1, t2, t3) AS (
    VALUES
        (1, 'N',  'north',     'เหนือ',     NULL,       NULL),
        (2, 'NE', 'northeast', 'ตะวันออก',  'เฉียง',    'เหนือ'),
        (3, 'E',  'east',      'ตะวันออก',  NULL,       NULL),
        (4, 'SE', 'southeast', 'ตะวันออก',  'เฉียง',    'ใต้'),
        (5, 'S',  'south',     'ใต้',       NULL,       NULL),
        (6, 'SW', 'southwest', 'ตะวันตก',   'เฉียง',    'ใต้'),
        (7, 'W',  'west',      'ตะวันตก',   NULL,       NULL),
        (8, 'NW', 'northwest', 'ตะวันตก',   'เฉียง',    'เหนือ')
),
prepared AS (
    SELECT
        d.id,
        d.label,
        d.lexeme,
        w1.word_id AS word1,
        w2.word_id AS word2,
        w3.word_id AS word3
    FROM dirs d
    JOIN w w1 ON w1.token = d.t1
    LEFT JOIN w w2 ON w2.token = d.t2
    LEFT JOIN w w3 ON w3.token = d.t3
),
ins_cn AS (
    INSERT INTO thai."CompositeName"(word1, word2, word3, word4, word5)
    SELECT p.word1, p.word2, p.word3, NULL, NULL
    FROM prepared p
    ORDER BY p.id
    RETURNING id
),
cn_ordered AS (
    SELECT id AS composite_name_id,
           row_number() OVER (ORDER BY id) AS rn
    FROM ins_cn
)
INSERT INTO thai."Direction"(id, label, lexeme, composite_name_id)
SELECT p.id, p.label, p.lexeme, c.composite_name_id
FROM prepared p
JOIN cn_ordered c ON c.rn = p.id;

INSERT INTO thai._direction_seed_meta(kind, composite_name_id, direction_id)
SELECT 'direction', composite_name_id, id FROM thai."Direction";

REFRESH MATERIALIZED VIEW thai."WordView";
-- Rename grid Position tables so "Position" can mean spatial prepositions.

ALTER TABLE thai."PositionWord" RENAME TO "GridPositionWord";
ALTER TABLE thai."Position" RENAME TO "GridPosition";

ALTER SEQUENCE thai."PositionWord_id_seq" RENAME TO "GridPositionWord_id_seq";
ALTER SEQUENCE thai."Position_id_seq" RENAME TO "GridPosition_id_seq";

ALTER TABLE thai."GridPositionWord" RENAME COLUMN position_id TO grid_position_id;

ALTER TABLE thai."GridPosition" RENAME CONSTRAINT "Position_pkey" TO "GridPosition_pkey";
ALTER TABLE thai."GridPosition" RENAME CONSTRAINT uq_thai_position_row_col TO uq_thai_grid_position_row_col;

ALTER TABLE thai."GridPositionWord" RENAME CONSTRAINT "PositionWord_pkey" TO "GridPositionWord_pkey";
ALTER TABLE thai."GridPositionWord" RENAME CONSTRAINT uq_thai_position_word_order TO uq_thai_grid_position_word_order;
ALTER TABLE thai."GridPositionWord" RENAME CONSTRAINT "PositionWord_position_id_fkey"
    TO "GridPositionWord_grid_position_id_fkey";
ALTER TABLE thai."GridPositionWord" RENAME CONSTRAINT "PositionWord_word_id_fkey"
    TO "GridPositionWord_word_id_fkey";
-- Spatial prepositions: Position → Word + Sentence → SentenceWord → Word

CREATE TABLE thai."Sentence" (
    id          integer                NOT NULL,
    translation character varying(128) NOT NULL,
    CONSTRAINT "Sentence_pkey" PRIMARY KEY (id)
);

CREATE TABLE thai."SentenceWord" (
    id          integer NOT NULL,
    sentence_id integer NOT NULL,
    word_id     integer NOT NULL,
    word_order  integer NOT NULL,
    CONSTRAINT "SentenceWord_pkey" PRIMARY KEY (id),
    CONSTRAINT "SentenceWord_sentence_id_fkey"
        FOREIGN KEY (sentence_id) REFERENCES thai."Sentence"(id) ON DELETE CASCADE,
    CONSTRAINT "SentenceWord_word_id_fkey"
        FOREIGN KEY (word_id) REFERENCES thai."Word"(id),
    CONSTRAINT uq_thai_sentence_word_order UNIQUE (sentence_id, word_order)
);

CREATE SEQUENCE thai."SentenceWord_id_seq"
    AS integer START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE CACHE 1;

ALTER SEQUENCE thai."SentenceWord_id_seq" OWNED BY thai."SentenceWord".id;

ALTER TABLE ONLY thai."SentenceWord"
    ALTER COLUMN id SET DEFAULT nextval('thai."SentenceWord_id_seq"'::regclass);

CREATE TABLE thai."Position" (
    id          integer NOT NULL,
    word_id     integer NOT NULL,
    sentence_id integer NOT NULL,
    CONSTRAINT "Position_pkey" PRIMARY KEY (id),
    CONSTRAINT "Position_word_id_fkey"
        FOREIGN KEY (word_id) REFERENCES thai."Word"(id),
    CONSTRAINT "Position_sentence_id_fkey"
        FOREIGN KEY (sentence_id) REFERENCES thai."Sentence"(id),
    CONSTRAINT uq_thai_position_word UNIQUE (word_id),
    CONSTRAINT uq_thai_position_sentence UNIQUE (sentence_id)
);

CREATE TABLE thai._position_seed_meta (
    kind text NOT NULL,
    token text,
    word_id integer,
    spelling_id integer,
    transcription_id integer,
    lexeme_id integer,
    sentence_id integer,
    position_id integer
);

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
            ('ลูก',       'lûk',        'lûuk',         'luːk',        'child',         'noun'),
            ('บอล',       'bɔːn',        'bɔɔn',         'bɔːn',        'ball',          'noun'),
            ('อยู่',      'jùː',         'yùu',          'juː',         'to be located', 'verb'),
            ('กล่อง',     'klɔ̂ːŋ',       'glɔ̂ng',        'klɔːŋ',       'box',           'noun'),
            ('บน',        'bon',         'bon',          'bon',         'on',            'preposition'),
            ('ใน',        'naj',         'nai',          'naj',         'in',            'preposition'),
            ('ใต้',       'tâj',         'dtâi',         'taj',         'under',         'preposition'),
            ('ข้าง',      'kʰâːŋ',       'kâang',        'kʰaːŋ',       'next to',       'preposition'),
            ('ระหว่าง',    'rawàːŋ',      'rá-wàang',     'rawaːŋ',      'between',       'preposition'),
            ('หน้า',      'nâː',         'nâa',          'naː',         'in front of',   'preposition'),
            ('หลัง',      'lǎŋ',         'lǎng',         'laŋ',         'behind',        'preposition'),
            ('นอก',       'nɔ̂ːk',        'nɔ̂ok',         'nɔːk',        'outside',       'preposition'),
            ('ท่ามกลาง',   'tʰâːmklaːŋ',  'tâam-glaang',  'tʰaːmklaːŋ',  'among',         'preposition')
        ) AS t(token, phonemic, paiboon, ipa, gloss, pos)
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
            INSERT INTO thai."Transcription"(phonemic, paiboon, "IPA")
            VALUES (r.phonemic, r.paiboon, r.ipa)
            RETURNING id INTO tid;

            INSERT INTO thai."Spelling"(value)
            VALUES (r.token)
            RETURNING id INTO sid;

            INSERT INTO thai."Word"(spelling_id, transcription, is_primary)
            VALUES (sid, tid, true)
            RETURNING id INTO wid;

            INSERT INTO thai._position_seed_meta(kind, token, word_id, spelling_id, transcription_id)
            VALUES ('word', r.token, wid, sid, tid);
        ELSE
            INSERT INTO thai._position_seed_meta(kind, token, word_id, spelling_id, transcription_id)
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

            INSERT INTO thai._position_seed_meta(kind, token, lexeme_id)
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

INSERT INTO thai."Sentence"(id, translation) VALUES
    (1, 'the ball is on the box'),
    (2, 'the ball is in the box'),
    (3, 'the ball is under the box'),
    (4, 'the ball is next to the box'),
    (5, 'the ball is between the boxes'),
    (6, 'the ball is in front of the box'),
    (7, 'the ball is behind the box'),
    (8, 'the ball is outside the box'),
    (9, 'the ball is among the boxes');

INSERT INTO thai._position_seed_meta(kind, sentence_id)
SELECT 'sentence', id FROM thai."Sentence";

WITH w AS (
    SELECT s.value AS token, MIN(w.id) AS word_id
    FROM thai."Word" w
    JOIN thai."Spelling" s ON s.id = w.spelling_id
    WHERE s.value IN (
        'ลูก', 'บอล', 'อยู่', 'กล่อง',
        'บน', 'ใน', 'ใต้', 'ข้าง', 'ระหว่าง', 'หน้า', 'หลัง', 'นอก', 'ท่ามกลาง')
    GROUP BY s.value
),
rows(sentence_id, word_order, token) AS (
    VALUES
        (1, 1, 'ลูก'), (1, 2, 'บอล'), (1, 3, 'อยู่'), (1, 4, 'บน'), (1, 5, 'กล่อง'),
        (2, 1, 'ลูก'), (2, 2, 'บอล'), (2, 3, 'อยู่'), (2, 4, 'ใน'), (2, 5, 'กล่อง'),
        (3, 1, 'ลูก'), (3, 2, 'บอล'), (3, 3, 'อยู่'), (3, 4, 'ใต้'), (3, 5, 'กล่อง'),
        (4, 1, 'ลูก'), (4, 2, 'บอล'), (4, 3, 'อยู่'), (4, 4, 'ข้าง'), (4, 5, 'กล่อง'),
        (5, 1, 'ลูก'), (5, 2, 'บอล'), (5, 3, 'อยู่'), (5, 4, 'ระหว่าง'), (5, 5, 'กล่อง'),
        (6, 1, 'ลูก'), (6, 2, 'บอล'), (6, 3, 'อยู่'), (6, 4, 'หน้า'), (6, 5, 'กล่อง'),
        (7, 1, 'ลูก'), (7, 2, 'บอล'), (7, 3, 'อยู่'), (7, 4, 'หลัง'), (7, 5, 'กล่อง'),
        (8, 1, 'ลูก'), (8, 2, 'บอล'), (8, 3, 'อยู่'), (8, 4, 'นอก'), (8, 5, 'กล่อง'),
        (9, 1, 'ลูก'), (9, 2, 'บอล'), (9, 3, 'อยู่'), (9, 4, 'ท่ามกลาง'), (9, 5, 'กล่อง')
)
INSERT INTO thai."SentenceWord"(sentence_id, word_id, word_order)
SELECT r.sentence_id, w.word_id, r.word_order
FROM rows r
JOIN w ON w.token = r.token
ORDER BY r.sentence_id, r.word_order;

WITH w AS (
    SELECT s.value AS token, MIN(w.id) AS word_id
    FROM thai."Word" w
    JOIN thai."Spelling" s ON s.id = w.spelling_id
    WHERE s.value IN ('บน', 'ใน', 'ใต้', 'ข้าง', 'ระหว่าง', 'หน้า', 'หลัง', 'นอก', 'ท่ามกลาง')
    GROUP BY s.value
),
heads(id, token) AS (
    VALUES
        (1, 'บน'),
        (2, 'ใน'),
        (3, 'ใต้'),
        (4, 'ข้าง'),
        (5, 'ระหว่าง'),
        (6, 'หน้า'),
        (7, 'หลัง'),
        (8, 'นอก'),
        (9, 'ท่ามกลาง')
)
INSERT INTO thai."Position"(id, word_id, sentence_id)
SELECT h.id, w.word_id, h.id
FROM heads h
JOIN w ON w.token = h.token;

INSERT INTO thai._position_seed_meta(kind, position_id, sentence_id, word_id)
SELECT 'position', p.id, p.sentence_id, p.word_id FROM thai."Position" p;

REFRESH MATERIALIZED VIEW thai."WordView";
-- Position heads → CompositeName (like Direction); fix ท่ามกลาง; add ids 10–14.

-- ── 1. Position: CompositeName + lexeme ──────────────────────────────────────

ALTER TABLE thai."Position"
    ADD COLUMN composite_name_id integer,
    ADD COLUMN lexeme character varying(32);

WITH created AS (
    INSERT INTO thai."CompositeName"(word1, word2, word3, word4, word5)
    SELECT p.word_id, NULL, NULL, NULL, NULL
    FROM thai."Position" p
    ORDER BY p.id
    RETURNING id, word1
)
UPDATE thai."Position" p
SET composite_name_id = c.id,
    lexeme = CASE p.id
        WHEN 1 THEN 'on'
        WHEN 2 THEN 'in'
        WHEN 3 THEN 'under'
        WHEN 4 THEN 'next to'
        WHEN 5 THEN 'between'
        WHEN 6 THEN 'in front of'
        WHEN 7 THEN 'behind'
        WHEN 8 THEN 'outside'
        WHEN 9 THEN 'among'
    END
FROM created c
WHERE p.word_id = c.word1;

ALTER TABLE thai."Position"
    ALTER COLUMN composite_name_id SET NOT NULL,
    ALTER COLUMN lexeme SET NOT NULL;

ALTER TABLE thai."Position"
    DROP CONSTRAINT IF EXISTS uq_thai_position_word,
    DROP CONSTRAINT IF EXISTS "Position_word_id_fkey";

ALTER TABLE thai."Position"
    DROP COLUMN word_id;

ALTER TABLE thai."Position"
    ADD CONSTRAINT "Position_composite_name_id_fkey"
        FOREIGN KEY (composite_name_id) REFERENCES thai."CompositeName"(id);

CREATE UNIQUE INDEX uq_thai_position_composite_name
    ON thai."Position"(composite_name_id);

-- ── 2. Atomic words for composites + new heads ───────────────────────────────

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
            ('ท่าม',  'tʰâːm',   'thâam',  'tʰaːm',  'midst',   'noun'),
            ('กลาง',  'klaːŋ',    'glaang', 'klaːŋ',  'middle',  'noun'),
            ('ทาง',   'tʰaːŋ',    'thaang', 'tʰaːŋ',  'way',     'noun'),
            ('ด้าน',  'dâːn',     'dâan',   'daːn',   'side',    'noun'),
            ('ซ้าย',  'sáːj',     'sáai',   'saːj',   'left',    'noun'),
            ('ขวา',   'kʰwǎː',    'kwǎa',   'kʰwaː',  'right',   'noun'),
            ('เหนือ', 'nɯ̌ːa',    'nǔea',   'nɯːa',   'above',   'preposition'),
            ('ข้าม',  'kʰâːm',    'khâam',  'kʰaːm',  'across',  'preposition'),
            ('มุม',   'múm',      'mum',    'mum',    'corner',  'noun')
        ) AS t(token, phonemic, paiboon, ipa, gloss, pos)
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
            INSERT INTO thai."Transcription"(phonemic, paiboon, "IPA")
            VALUES (r.phonemic, r.paiboon, r.ipa)
            RETURNING id INTO tid;

            INSERT INTO thai."Spelling"(value)
            VALUES (r.token)
            RETURNING id INTO sid;

            INSERT INTO thai."Word"(spelling_id, transcription, is_primary)
            VALUES (sid, tid, true)
            RETURNING id INTO wid;

            INSERT INTO thai._position_seed_meta(kind, token, word_id, spelling_id, transcription_id)
            VALUES ('word', r.token, wid, sid, tid);
        ELSE
            INSERT INTO thai._position_seed_meta(kind, token, word_id, spelling_id, transcription_id)
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

            INSERT INTO thai._position_seed_meta(kind, token, lexeme_id)
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

-- ── 3. Fix id 9: ท่ามกลาง → ท่าม + กลาง ─────────────────────────────────────

WITH w AS (
    SELECT s.value AS token, MIN(w.id) AS word_id
    FROM thai."Word" w
    JOIN thai."Spelling" s ON s.id = w.spelling_id
    WHERE s.value IN ('ท่าม', 'กลาง')
    GROUP BY s.value
)
UPDATE thai."CompositeName" cn
SET word1 = w1.word_id,
    word2 = w2.word_id,
    word3 = NULL,
    word4 = NULL,
    word5 = NULL
FROM thai."Position" p
JOIN w w1 ON w1.token = 'ท่าม'
JOIN w w2 ON w2.token = 'กลาง'
WHERE p.id = 9
  AND p.composite_name_id = cn.id;

DELETE FROM thai."SentenceWord" WHERE sentence_id = 9;

WITH w AS (
    SELECT s.value AS token, MIN(w.id) AS word_id
    FROM thai."Word" w
    JOIN thai."Spelling" s ON s.id = w.spelling_id
    WHERE s.value IN ('ลูก', 'บอล', 'อยู่', 'ท่าม', 'กลาง', 'กล่อง')
    GROUP BY s.value
),
rows(word_order, token) AS (
    VALUES
        (1, 'ลูก'), (2, 'บอล'), (3, 'อยู่'), (4, 'ท่าม'), (5, 'กลาง'), (6, 'กล่อง')
)
INSERT INTO thai."SentenceWord"(sentence_id, word_id, word_order)
SELECT 9, w.word_id, r.word_order
FROM rows r
JOIN w ON w.token = r.token
ORDER BY r.word_order;

-- ── 4. New positions 10–14 ───────────────────────────────────────────────────

INSERT INTO thai."Sentence"(id, translation) VALUES
    (10, 'the ball is above the box'),
    (11, 'the ball is across from the box'),
    (12, 'the ball is at the corner of the box'),
    (13, 'the ball is to the left of the box'),
    (14, 'the ball is to the right of the box');

INSERT INTO thai._position_seed_meta(kind, sentence_id)
SELECT 'sentence', id FROM thai."Sentence" WHERE id BETWEEN 10 AND 14;

WITH w AS (
    SELECT s.value AS token, MIN(w.id) AS word_id
    FROM thai."Word" w
    JOIN thai."Spelling" s ON s.id = w.spelling_id
    WHERE s.value IN (
        'ลูก', 'บอล', 'อยู่', 'กล่อง',
        'เหนือ', 'ข้าม', 'มุม',
        'ทาง', 'ด้าน', 'ซ้าย', 'ขวา')
    GROUP BY s.value
),
rows(sentence_id, word_order, token) AS (
    VALUES
        (10, 1, 'ลูก'), (10, 2, 'บอล'), (10, 3, 'อยู่'), (10, 4, 'เหนือ'), (10, 5, 'กล่อง'),
        (11, 1, 'ลูก'), (11, 2, 'บอล'), (11, 3, 'อยู่'), (11, 4, 'ข้าม'), (11, 5, 'กล่อง'),
        (12, 1, 'ลูก'), (12, 2, 'บอล'), (12, 3, 'อยู่'), (12, 4, 'มุม'), (12, 5, 'กล่อง'),
        (13, 1, 'ลูก'), (13, 2, 'บอล'), (13, 3, 'อยู่'),
            (13, 4, 'ทาง'), (13, 5, 'ด้าน'), (13, 6, 'ซ้าย'), (13, 7, 'กล่อง'),
        (14, 1, 'ลูก'), (14, 2, 'บอล'), (14, 3, 'อยู่'),
            (14, 4, 'ทาง'), (14, 5, 'ด้าน'), (14, 6, 'ขวา'), (14, 7, 'กล่อง')
)
INSERT INTO thai."SentenceWord"(sentence_id, word_id, word_order)
SELECT r.sentence_id, w.word_id, r.word_order
FROM rows r
JOIN w ON w.token = r.token
ORDER BY r.sentence_id, r.word_order;

WITH w AS (
    SELECT s.value AS token, MIN(w.id) AS word_id
    FROM thai."Word" w
    JOIN thai."Spelling" s ON s.id = w.spelling_id
    WHERE s.value IN ('เหนือ', 'ข้าม', 'มุม', 'ทาง', 'ด้าน', 'ซ้าย', 'ขวา')
    GROUP BY s.value
),
heads(id, lexeme, t1, t2, t3) AS (
    VALUES
        (10, 'above',           'เหนือ', NULL,   NULL),
        (11, 'across',          'ข้าม',  NULL,   NULL),
        (12, 'at the corner',   'มุม',   NULL,   NULL),
        (13, 'to the left of',  'ทาง',   'ด้าน', 'ซ้าย'),
        (14, 'to the right of', 'ทาง',   'ด้าน', 'ขวา')
),
prepared AS (
    SELECT
        h.id,
        h.lexeme,
        w1.word_id AS word1,
        w2.word_id AS word2,
        w3.word_id AS word3
    FROM heads h
    JOIN w w1 ON w1.token = h.t1
    LEFT JOIN w w2 ON w2.token = h.t2
    LEFT JOIN w w3 ON w3.token = h.t3
),
ins_cn AS (
    INSERT INTO thai."CompositeName"(word1, word2, word3, word4, word5)
    SELECT p.word1, p.word2, p.word3, NULL, NULL
    FROM prepared p
    ORDER BY p.id
    RETURNING id
),
cn_ordered AS (
    SELECT id AS composite_name_id,
           row_number() OVER (ORDER BY id) AS rn
    FROM ins_cn
),
prepared_ordered AS (
    SELECT p.*, row_number() OVER (ORDER BY p.id) AS rn
    FROM prepared p
)
INSERT INTO thai."Position"(id, composite_name_id, sentence_id, lexeme)
SELECT po.id, c.composite_name_id, po.id, po.lexeme
FROM prepared_ordered po
JOIN cn_ordered c ON c.rn = po.rn;

INSERT INTO thai._position_seed_meta(kind, position_id, sentence_id)
SELECT 'position', p.id, p.sentence_id
FROM thai."Position" p
WHERE p.id BETWEEN 10 AND 14;

REFRESH MATERIALIZED VIEW thai."WordView";
-- Drop phonemic transcription; keep paiboon + IPA only.

DROP MATERIALIZED VIEW IF EXISTS thai."WordView";

ALTER TABLE thai."Transcription"
    DROP COLUMN IF EXISTS phonemic;

CREATE MATERIALIZED VIEW thai."WordView" AS
 SELECT w.id,
    w.spelling_id,
    w.transcription AS transcription_id,
    sp.value AS spelling,
    tr.paiboon,
    tr."IPA" AS ipa,
    COALESCE(array_agg(l.value ORDER BY l.value) FILTER (WHERE (l.value IS NOT NULL)), ARRAY[]::text[]) AS lexemes
   FROM thai."Word" w
     JOIN thai."Spelling" sp ON sp.id = w.spelling_id
     LEFT JOIN thai."Transcription" tr ON tr.id = w.transcription
     LEFT JOIN thai."WordLexeme" wl ON wl.word = w.id
     LEFT JOIN thai."Lexeme" l ON l.id = wl.lexeme
  GROUP BY w.id, w.spelling_id, w.transcription, sp.value, tr.paiboon, tr."IPA"
  WITH NO DATA;

CREATE UNIQUE INDEX "WordView_id_idx" ON thai."WordView" USING btree (id);
CREATE INDEX "WordView_spelling_id_idx" ON thai."WordView" USING btree (spelling_id);
CREATE INDEX "WordView_transcription_id_idx" ON thai."WordView" USING btree (transcription_id);

UPDATE thai."Transcription" t
SET paiboon = v.paiboon,
    "IPA"   = v.ipa
FROM thai."Word" w
JOIN thai."Spelling" s ON s.id = w.spelling_id
JOIN (
    VALUES
        ('ลูก',      'lûuk',         'luːk'),
        ('บอล',      'bɔɔn',         'bɔːn'),
        ('อยู่',     'yùu',          'juː'),
        ('กล่อง',    'glɔ̂ng',        'klɔːŋ'),
        ('บน',       'bon',          'bon'),
        ('ใน',       'nai',          'naj'),
        ('ใต้',      'dtâi',         'taj'),
        ('ข้าง',     'kâang',        'kʰaːŋ'),
        ('ระหว่าง',   'rá-wàang',     'rawaːŋ'),
        ('หน้า',     'nâa',          'naː'),
        ('หลัง',     'lǎng',         'laŋ'),
        ('นอก',      'nɔ̂ok',         'nɔːk'),
        ('ท่ามกลาง',  'thâam-glaang', 'tʰaːm klaːŋ'),
        ('ท่าม',     'thâam',        'tʰaːm'),
        ('กลาง',     'glaang',       'klaːŋ'),
        ('ทาง',      'thaang',       'tʰaːŋ'),
        ('ด้าน',     'dâan',         'daːn'),
        ('ซ้าย',     'sáai',         'saːj'),
        ('ขวา',      'kwǎa',         'kʰwaː'),
        ('เหนือ',    'nǔea',         'nɯ̌a'),
        ('ข้าม',     'khâam',        'kʰaːm'),
        ('มุม',      'mum',          'mum')
) AS v(token, paiboon, ipa) ON v.token = s.value
WHERE t.id = w.transcription;

REFRESH MATERIALIZED VIEW thai."WordView";
-- Drop Position entries for across (11) and corner (12).
-- Keeps Word / Lexeme / Spelling / Transcription for ข้าม and มุม.

DELETE FROM thai._position_seed_meta
WHERE (kind = 'position' AND position_id IN (11, 12))
   OR (kind = 'sentence' AND sentence_id IN (11, 12));

DELETE FROM thai."SentenceWord"
WHERE sentence_id IN (11, 12);

-- Capture composite names, then drop Position (releases FKs), then orphans.
CREATE TEMP TABLE _v7_drop_cn AS
SELECT composite_name_id AS id
FROM thai."Position"
WHERE id IN (11, 12);

DELETE FROM thai."Position"
WHERE id IN (11, 12);

DELETE FROM thai."Sentence"
WHERE id IN (11, 12);

DELETE FROM thai."CompositeName" cn
USING _v7_drop_cn d
WHERE cn.id = d.id;

DROP TABLE _v7_drop_cn;

-- ============================================================================
-- PHASE 1: Insert new Spelling entries (157 new lexemes)
-- ============================================================================
INSERT INTO thai."Spelling" (id, value) VALUES (518, 'กรมท่า');
INSERT INTO thai."Spelling" (id, value) VALUES (519, 'กรรณิการ์');
INSERT INTO thai."Spelling" (id, value) VALUES (520, 'กระมุท');
INSERT INTO thai."Spelling" (id, value) VALUES (521, 'กรัก');
INSERT INTO thai."Spelling" (id, value) VALUES (522, 'กฤษณา');
INSERT INTO thai."Spelling" (id, value) VALUES (523, 'กลาโหม');
INSERT INTO thai."Spelling" (id, value) VALUES (524, 'กลีบ');
INSERT INTO thai."Spelling" (id, value) VALUES (525, 'กะบัง');
INSERT INTO thai."Spelling" (id, value) VALUES (526, 'กะปิ');
INSERT INTO thai."Spelling" (id, value) VALUES (527, 'กะลิง');
INSERT INTO thai."Spelling" (id, value) VALUES (528, 'กา');
INSERT INTO thai."Spelling" (id, value) VALUES (529, 'กากี');
INSERT INTO thai."Spelling" (id, value) VALUES (530, 'การเวท');
INSERT INTO thai."Spelling" (id, value) VALUES (531, 'กาฬ');
INSERT INTO thai."Spelling" (id, value) VALUES (532, 'กุหลาบ');
INSERT INTO thai."Spelling" (id, value) VALUES (533, 'กุ้ง');
INSERT INTO thai."Spelling" (id, value) VALUES (534, 'ก้าน');
INSERT INTO thai."Spelling" (id, value) VALUES (535, 'ก้าม');
INSERT INTO thai."Spelling" (id, value) VALUES (536, 'ขจี');
INSERT INTO thai."Spelling" (id, value) VALUES (537, 'ขน');
INSERT INTO thai."Spelling" (id, value) VALUES (538, 'ขาบ');
INSERT INTO thai."Spelling" (id, value) VALUES (539, 'ขี้ผึ้ง');
INSERT INTO thai."Spelling" (id, value) VALUES (540, 'ขุ่น');
INSERT INTO thai."Spelling" (id, value) VALUES (541, 'ข้าวโพด');
INSERT INTO thai."Spelling" (id, value) VALUES (542, 'ครั่ง');
INSERT INTO thai."Spelling" (id, value) VALUES (543, 'ครุฑ');
INSERT INTO thai."Spelling" (id, value) VALUES (544, 'คอ');
INSERT INTO thai."Spelling" (id, value) VALUES (545, 'คำ');
INSERT INTO thai."Spelling" (id, value) VALUES (546, 'จำปา');
INSERT INTO thai."Spelling" (id, value) VALUES (547, 'จีน');
INSERT INTO thai."Spelling" (id, value) VALUES (548, 'ชมพู');
INSERT INTO thai."Spelling" (id, value) VALUES (549, 'ชมพูนุช');
INSERT INTO thai."Spelling" (id, value) VALUES (550, 'ชมพู่');
INSERT INTO thai."Spelling" (id, value) VALUES (551, 'ชาด');
INSERT INTO thai."Spelling" (id, value) VALUES (552, 'ซีด');
INSERT INTO thai."Spelling" (id, value) VALUES (553, 'ดอก');
INSERT INTO thai."Spelling" (id, value) VALUES (554, 'ดำ');
INSERT INTO thai."Spelling" (id, value) VALUES (555, 'ดิน');
INSERT INTO thai."Spelling" (id, value) VALUES (556, 'ตะพุ่น');
INSERT INTO thai."Spelling" (id, value) VALUES (557, 'ตะแบก');
INSERT INTO thai."Spelling" (id, value) VALUES (558, 'ตัด');
INSERT INTO thai."Spelling" (id, value) VALUES (559, 'ตั้งแช');
INSERT INTO thai."Spelling" (id, value) VALUES (560, 'ถั่ว');
INSERT INTO thai."Spelling" (id, value) VALUES (561, 'ทับทิม');
INSERT INTO thai."Spelling" (id, value) VALUES (562, 'นก');
INSERT INTO thai."Spelling" (id, value) VALUES (563, 'นกยูง');
INSERT INTO thai."Spelling" (id, value) VALUES (564, 'นวล');
INSERT INTO thai."Spelling" (id, value) VALUES (565, 'นาก');
INSERT INTO thai."Spelling" (id, value) VALUES (566, 'นาง');
INSERT INTO thai."Spelling" (id, value) VALUES (567, 'นิล');
INSERT INTO thai."Spelling" (id, value) VALUES (568, 'นิลุบล');
INSERT INTO thai."Spelling" (id, value) VALUES (569, 'น้ำตาล');
INSERT INTO thai."Spelling" (id, value) VALUES (570, 'น้ำผึ้ง');
INSERT INTO thai."Spelling" (id, value) VALUES (571, 'น้ำเงิน');
INSERT INTO thai."Spelling" (id, value) VALUES (572, 'บวบ');
INSERT INTO thai."Spelling" (id, value) VALUES (573, 'บัว');
INSERT INTO thai."Spelling" (id, value) VALUES (574, 'บาน');
INSERT INTO thai."Spelling" (id, value) VALUES (575, 'ปน');
INSERT INTO thai."Spelling" (id, value) VALUES (576, 'ประภัสสร');
INSERT INTO thai."Spelling" (id, value) VALUES (577, 'ปีก');
INSERT INTO thai."Spelling" (id, value) VALUES (578, 'ปู');
INSERT INTO thai."Spelling" (id, value) VALUES (579, 'ปูน');
INSERT INTO thai."Spelling" (id, value) VALUES (580, 'ผล');
INSERT INTO thai."Spelling" (id, value) VALUES (581, 'ผักตบ');
INSERT INTO thai."Spelling" (id, value) VALUES (582, 'ผุ่น');
INSERT INTO thai."Spelling" (id, value) VALUES (583, 'ผ่อง');
INSERT INTO thai."Spelling" (id, value) VALUES (584, 'ฝรั่ง');
INSERT INTO thai."Spelling" (id, value) VALUES (585, 'ฝาด');
INSERT INTO thai."Spelling" (id, value) VALUES (586, 'พราย');
INSERT INTO thai."Spelling" (id, value) VALUES (587, 'พวง');
INSERT INTO thai."Spelling" (id, value) VALUES (588, 'พิกุล');
INSERT INTO thai."Spelling" (id, value) VALUES (589, 'พิราบ');
INSERT INTO thai."Spelling" (id, value) VALUES (590, 'ฟาง');
INSERT INTO thai."Spelling" (id, value) VALUES (591, 'ภู');
INSERT INTO thai."Spelling" (id, value) VALUES (592, 'มณี');
INSERT INTO thai."Spelling" (id, value) VALUES (593, 'มหาดไทย');
INSERT INTO thai."Spelling" (id, value) VALUES (594, 'มอ');
INSERT INTO thai."Spelling" (id, value) VALUES (595, 'มะตูม');
INSERT INTO thai."Spelling" (id, value) VALUES (596, 'มะปราง');
INSERT INTO thai."Spelling" (id, value) VALUES (597, 'มะพูด');
INSERT INTO thai."Spelling" (id, value) VALUES (598, 'มะลิ');
INSERT INTO thai."Spelling" (id, value) VALUES (599, 'มังคุด');
INSERT INTO thai."Spelling" (id, value) VALUES (600, 'มุก');
INSERT INTO thai."Spelling" (id, value) VALUES (601, 'ม่วง');
INSERT INTO thai."Spelling" (id, value) VALUES (602, 'ยอ');
INSERT INTO thai."Spelling" (id, value) VALUES (603, 'รง');
INSERT INTO thai."Spelling" (id, value) VALUES (604, 'รงกา');
INSERT INTO thai."Spelling" (id, value) VALUES (605, 'รัก');
INSERT INTO thai."Spelling" (id, value) VALUES (606, 'ลิ้นจี่');
INSERT INTO thai."Spelling" (id, value) VALUES (607, 'ศักดิ์');
INSERT INTO thai."Spelling" (id, value) VALUES (608, 'สด');
INSERT INTO thai."Spelling" (id, value) VALUES (609, 'สนทยา');
INSERT INTO thai."Spelling" (id, value) VALUES (610, 'สวาด');
INSERT INTO thai."Spelling" (id, value) VALUES (611, 'สัก');
INSERT INTO thai."Spelling" (id, value) VALUES (612, 'สังข์');
INSERT INTO thai."Spelling" (id, value) VALUES (613, 'สัมฤทธิ์');
INSERT INTO thai."Spelling" (id, value) VALUES (614, 'สิงห์');
INSERT INTO thai."Spelling" (id, value) VALUES (615, 'สุก');
INSERT INTO thai."Spelling" (id, value) VALUES (616, 'สุข');
INSERT INTO thai."Spelling" (id, value) VALUES (617, 'ส้ม');
INSERT INTO thai."Spelling" (id, value) VALUES (618, 'หมอก');
INSERT INTO thai."Spelling" (id, value) VALUES (619, 'หมาก');
INSERT INTO thai."Spelling" (id, value) VALUES (620, 'หมึก');
INSERT INTO thai."Spelling" (id, value) VALUES (621, 'หมู');
INSERT INTO thai."Spelling" (id, value) VALUES (622, 'หรดาล');
INSERT INTO thai."Spelling" (id, value) VALUES (623, 'หลุด');
INSERT INTO thai."Spelling" (id, value) VALUES (624, 'หว้า');
INSERT INTO thai."Spelling" (id, value) VALUES (625, 'หัว');
INSERT INTO thai."Spelling" (id, value) VALUES (626, 'ห่าน');
INSERT INTO thai."Spelling" (id, value) VALUES (627, 'อม');
INSERT INTO thai."Spelling" (id, value) VALUES (628, 'อสุรา');
INSERT INTO thai."Spelling" (id, value) VALUES (629, 'ออก');
INSERT INTO thai."Spelling" (id, value) VALUES (630, 'อัคนี');
INSERT INTO thai."Spelling" (id, value) VALUES (631, 'อังกาบ');
INSERT INTO thai."Spelling" (id, value) VALUES (632, 'อัญชัน');
INSERT INTO thai."Spelling" (id, value) VALUES (633, 'อำพัน');
INSERT INTO thai."Spelling" (id, value) VALUES (634, 'อิฐ');
INSERT INTO thai."Spelling" (id, value) VALUES (635, 'อุบล');
INSERT INTO thai."Spelling" (id, value) VALUES (636, 'อ่อน');
INSERT INTO thai."Spelling" (id, value) VALUES (637, 'เกสร');
INSERT INTO thai."Spelling" (id, value) VALUES (638, 'เขม่า');
INSERT INTO thai."Spelling" (id, value) VALUES (639, 'เขียว');
INSERT INTO thai."Spelling" (id, value) VALUES (640, 'เข้ม');
INSERT INTO thai."Spelling" (id, value) VALUES (641, 'เงิน');
INSERT INTO thai."Spelling" (id, value) VALUES (642, 'เดช');
INSERT INTO thai."Spelling" (id, value) VALUES (643, 'เทศ');
INSERT INTO thai."Spelling" (id, value) VALUES (644, 'เทา');
INSERT INTO thai."Spelling" (id, value) VALUES (645, 'เนื้อ');
INSERT INTO thai."Spelling" (id, value) VALUES (646, 'เบจ');
INSERT INTO thai."Spelling" (id, value) VALUES (647, 'เปลือก');
INSERT INTO thai."Spelling" (id, value) VALUES (648, 'เป็ด');
INSERT INTO thai."Spelling" (id, value) VALUES (649, 'เผือก');
INSERT INTO thai."Spelling" (id, value) VALUES (650, 'เพรช');
INSERT INTO thai."Spelling" (id, value) VALUES (651, 'เพลิง');
INSERT INTO thai."Spelling" (id, value) VALUES (652, 'เพียง');
INSERT INTO thai."Spelling" (id, value) VALUES (653, 'เมฆ');
INSERT INTO thai."Spelling" (id, value) VALUES (654, 'เม็ด');
INSERT INTO thai."Spelling" (id, value) VALUES (655, 'เลา');
INSERT INTO thai."Spelling" (id, value) VALUES (656, 'เลือด');
INSERT INTO thai."Spelling" (id, value) VALUES (657, 'เลื่อม');
INSERT INTO thai."Spelling" (id, value) VALUES (658, 'เสน');
INSERT INTO thai."Spelling" (id, value) VALUES (659, 'เหลือง');
INSERT INTO thai."Spelling" (id, value) VALUES (660, 'แกม');
INSERT INTO thai."Spelling" (id, value) VALUES (661, 'แก่');
INSERT INTO thai."Spelling" (id, value) VALUES (662, 'แค');
INSERT INTO thai."Spelling" (id, value) VALUES (663, 'แดง');
INSERT INTO thai."Spelling" (id, value) VALUES (664, 'แพรก');
INSERT INTO thai."Spelling" (id, value) VALUES (665, 'แมว');
INSERT INTO thai."Spelling" (id, value) VALUES (666, 'แลบ');
INSERT INTO thai."Spelling" (id, value) VALUES (667, 'โรย');
INSERT INTO thai."Spelling" (id, value) VALUES (668, 'โศก');
INSERT INTO thai."Spelling" (id, value) VALUES (669, 'ใบ');
INSERT INTO thai."Spelling" (id, value) VALUES (670, 'ไผ่');
INSERT INTO thai."Spelling" (id, value) VALUES (671, 'ไพร');
INSERT INTO thai."Spelling" (id, value) VALUES (672, 'ไพล');
INSERT INTO thai."Spelling" (id, value) VALUES (673, 'ไหม้');
INSERT INTO thai."Spelling" (id, value) VALUES (674, 'ไหล');

SELECT setval('thai."Spelling_id_seq"', 674);

-- ============================================================================
-- PHASE 2: Insert new Transcription entries
-- ============================================================================
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (616, 'krom-thâ', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (617, 'kan-ní-ka', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (618, 'krà-múth', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (619, 'kràk', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (620, 'krì-sǎna', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (621, 'kla-ǒm', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (622, 'klìp', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (623, 'kà-bang', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (624, 'kà-pì', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (625, 'kà-ling', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (626, 'ka', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (627, 'ka-ki', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (628, 'kan-wêt', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (629, 'kan', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (630, 'kù-làp', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (631, 'kûng', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (632, 'kân', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (633, 'kâm', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (634, 'khǎchi', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (635, 'khǒn', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (636, 'khàp', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (637, 'khî-phûeng', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (638, 'khùn', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (639, 'khâo-phôt', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (640, 'khrâng', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (641, 'khrút', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (642, 'khô', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (643, 'kham', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (644, 'cham-pa', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (645, 'chin', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (646, 'chom-phu', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (647, 'chom-phu-nút', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (648, 'chom-phû', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (649, 'chât', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (650, 'sît', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (651, 'dòk', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (652, 'dam', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (653, 'din', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (654, 'tà-phûn', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (655, 'tà-bàek', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (656, 'tàt', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (657, 'tâng-châe', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (658, 'thùa', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (659, 'tháp-thim', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (660, 'nôk', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (661, 'nôk-yung', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (662, 'nuan', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (663, 'nâk', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (664, 'nang', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (665, 'nin', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (666, 'ní-lubon', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (667, 'nám-tan', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (668, 'nám-phûeng', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (669, 'nám-ngoen', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (670, 'bùap', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (671, 'bua', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (672, 'ban', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (673, 'pon', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (674, 'prà-phát-sǒn', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (675, 'pìk', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (676, 'pu', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (677, 'pun', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (678, 'phǒn', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (679, 'phàk-tòp', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (680, 'phùn', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (681, 'phòng', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (682, 'fràng', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (683, 'fàt', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (684, 'phrai', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (685, 'phuang', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (686, 'phí-kun', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (687, 'phí-râp', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (688, 'fang', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (689, 'phu', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (690, 'm-ni', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (691, 'mâhad-thai', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (692, 'mo', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (693, 'má-tum', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (694, 'má-prang', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (695, 'má-phût', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (696, 'má-lí', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (697, 'mang-khút', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (698, 'múk', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (699, 'mûang', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (700, 'yo', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (701, 'rong', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (702, 'rong-ka', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (703, 'rák', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (704, 'lín-chì', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (705, 'sàk', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (706, 'sòt', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (707, 'sònth-ya', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (708, 'swàt', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (709, 'sàk', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (710, 'sàng', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (711, 'samruet', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (712, 'sìng', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (713, 'sùk', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (714, 'sùk', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (715, 'sôm', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (716, 'mòk', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (717, 'màk', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (718, 'mùek', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (719, 'mǔ', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (720, 'ǒn-dan', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (721, 'lùt', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (722, 'wâ', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (723, 'hǔa', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (724, 'àn', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (725, 'om', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (726, '-sù-ra', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (727, 'òk', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (728, 'àk-ni', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (729, 'àng-kàp', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (730, 'àn-chan', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (731, 'am-phan', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (732, 'ìt', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (733, 'ù-bon', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (734, 'òn', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (735, 'kè-sǒn', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (736, 'khèma', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (737, 'khǐao', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (738, 'khêm', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (739, 'ngoen', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (740, 'dèt', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (741, 'thêt', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (742, 'thao', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (743, 'núea', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (744, 'bèt', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (745, 'pèluek', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (746, 'pèt', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (747, 'phùeak', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (748, 'phêrot', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (749, 'pheling', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (750, 'phiang', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (751, 'mêk', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (752, 'mêt', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (753, 'lao', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (754, 'lûeat', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (755, 'lûeam', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (756, 'sěn', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (757, 'ělueng', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (758, 'kaem', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (759, 'kàe', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (760, 'khâe', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (761, 'daeng', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (762, 'phâenk', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (763, 'maeo', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (764, 'lâep', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (765, 'roi', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (766, 'sòk', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (767, 'bai', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (768, 'phài', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (769, 'phrai', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (770, 'phlai', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (771, 'mâi', NULL);
INSERT INTO thai."Transcription" (id, paiboon, "IPA") VALUES (772, 'lǎi', NULL);

SELECT setval('thai."Transcription_id_seq"', 772);

-- ============================================================================
-- PHASE 3: Insert new Word entries (linking Spelling + Transcription)
-- ============================================================================
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (537, 616, 518, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (538, 617, 519, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (539, 618, 520, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (540, 619, 521, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (541, 620, 522, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (542, 621, 523, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (543, 622, 524, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (544, 623, 525, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (545, 624, 526, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (546, 625, 527, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (547, 626, 528, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (548, 627, 529, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (549, 628, 530, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (550, 629, 531, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (551, 630, 532, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (552, 631, 533, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (553, 632, 534, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (554, 633, 535, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (555, 634, 536, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (556, 635, 537, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (557, 636, 538, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (558, 637, 539, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (559, 638, 540, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (560, 639, 541, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (561, 640, 542, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (562, 641, 543, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (563, 642, 544, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (564, 643, 545, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (565, 644, 546, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (566, 645, 547, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (567, 646, 548, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (568, 647, 549, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (569, 648, 550, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (570, 649, 551, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (571, 650, 552, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (572, 651, 553, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (573, 652, 554, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (574, 653, 555, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (575, 654, 556, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (576, 655, 557, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (577, 656, 558, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (578, 657, 559, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (579, 658, 560, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (580, 659, 561, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (581, 660, 562, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (582, 661, 563, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (583, 662, 564, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (584, 663, 565, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (585, 664, 566, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (586, 665, 567, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (587, 666, 568, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (588, 667, 569, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (589, 668, 570, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (590, 669, 571, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (591, 670, 572, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (592, 671, 573, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (593, 672, 574, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (594, 673, 575, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (595, 674, 576, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (596, 675, 577, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (597, 676, 578, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (598, 677, 579, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (599, 678, 580, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (600, 679, 581, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (601, 680, 582, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (602, 681, 583, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (603, 682, 584, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (604, 683, 585, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (605, 684, 586, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (606, 685, 587, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (607, 686, 588, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (608, 687, 589, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (609, 688, 590, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (610, 689, 591, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (611, 690, 592, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (612, 691, 593, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (613, 692, 594, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (614, 693, 595, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (615, 694, 596, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (616, 695, 597, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (617, 696, 598, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (618, 697, 599, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (619, 698, 600, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (620, 699, 601, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (621, 700, 602, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (622, 701, 603, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (623, 702, 604, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (624, 703, 605, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (625, 704, 606, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (626, 705, 607, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (627, 706, 608, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (628, 707, 609, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (629, 708, 610, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (630, 709, 611, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (631, 710, 612, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (632, 711, 613, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (633, 712, 614, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (634, 713, 615, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (635, 714, 616, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (636, 715, 617, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (637, 716, 618, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (638, 717, 619, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (639, 718, 620, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (640, 719, 621, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (641, 720, 622, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (642, 721, 623, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (643, 722, 624, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (644, 723, 625, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (645, 724, 626, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (646, 725, 627, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (647, 726, 628, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (648, 727, 629, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (649, 728, 630, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (650, 729, 631, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (651, 730, 632, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (652, 731, 633, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (653, 732, 634, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (654, 733, 635, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (655, 734, 636, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (656, 735, 637, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (657, 736, 638, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (658, 737, 639, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (659, 738, 640, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (660, 739, 641, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (661, 740, 642, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (662, 741, 643, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (663, 742, 644, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (664, 743, 645, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (665, 744, 646, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (666, 745, 647, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (667, 746, 648, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (668, 747, 649, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (669, 748, 650, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (670, 749, 651, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (671, 750, 652, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (672, 751, 653, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (673, 752, 654, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (674, 753, 655, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (675, 754, 656, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (676, 755, 657, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (677, 756, 658, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (678, 757, 659, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (679, 758, 660, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (680, 759, 661, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (681, 760, 662, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (682, 761, 663, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (683, 762, 664, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (684, 763, 665, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (685, 764, 666, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (686, 765, 667, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (687, 766, 668, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (688, 767, 669, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (689, 768, 670, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (690, 769, 671, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (691, 770, 672, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (692, 771, 673, true);
INSERT INTO thai."Word" (id, transcription, spelling_id, is_primary) VALUES (693, 772, 674, true);

SELECT setval('thai."Word_id_seq"', 693);

-- ============================================================================
-- PHASE 4: Create HueFamily table
-- ============================================================================
CREATE TABLE thai."HueFamily" (
    id SERIAL PRIMARY KEY,
    thai_name VARCHAR(50) NOT NULL UNIQUE,
    english_name VARCHAR(50) NOT NULL,
    hex_representative CHAR(7) NOT NULL,
    display_order SMALLINT NOT NULL UNIQUE
);

-- ============================================================================
-- PHASE 5: Create Color table
-- ============================================================================
CREATE TABLE thai."Color" (
    id SERIAL PRIMARY KEY,
    hue_family_id INTEGER NOT NULL REFERENCES thai."HueFamily"(id),
    composite_name_id INTEGER REFERENCES thai."CompositeName"(id),
    english_label VARCHAR(100) NOT NULL,
    hex CHAR(7) NOT NULL UNIQUE,
    luminance_order SMALLINT NOT NULL,
    notes TEXT
);

CREATE INDEX idx_color_family ON thai."Color"(hue_family_id);
CREATE INDEX idx_color_composite ON thai."Color"(composite_name_id);

-- ============================================================================
-- PHASE 6: Insert HueFamily data
-- ============================================================================
INSERT INTO thai."HueFamily" (thai_name, english_name, hex_representative, display_order) VALUES
('แดง', 'Red', '#c9242b', 1),
('ส้ม', 'Orange', '#f36e31', 2),
('เหลือง', 'Yellow', '#f4d25d', 3),
('เขียว', 'Green', '#21723e', 4),
('น้ำเงินเขียว', 'Cyan', '#008d9b', 5),
('ฟ้า', 'Sky Blue', '#3476ae', 6),
('น้ำเงิน', 'Navy Blue', '#0c4da2', 7),
('ม่วง', 'Purple', '#702c7d', 8),
('ชมพู', 'Pink', '#f698a3', 9),
('น้ำตาล', 'Brown', '#7b4a34', 10),
('เทา', 'Grey', '#7f8585', 11),
('ดำ', 'Black', '#000000', 12),
('ขาว', 'White', '#f1ede2', 13);

-- ============================================================================
-- PHASE 7: Create helper function to get Word ID by Thai spelling
-- ============================================================================
CREATE OR REPLACE FUNCTION thai.get_word_id_by_spelling(p_thai VARCHAR)
RETURNS INTEGER AS $$
DECLARE
    v_word_id INTEGER;
BEGIN
    SELECT w.id INTO v_word_id
    FROM thai."Word" w
    JOIN thai."Spelling" s ON w.spelling_id = s.id
    WHERE s.value = p_thai
    LIMIT 1;
    RETURN v_word_id;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- PHASE 8: Insert CompositeName and Color entries
-- ============================================================================

-- Red (แดง) - 11 colors
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (32, thai.get_word_id_by_spelling('นาก'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'แดง'), 32, 'Rose Gold', '#cb7371', 1, 'gold-copper alloy');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (33, thai.get_word_id_by_spelling('ชาด'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'แดง'), 33, 'Cinnabar', '#E34234', 2, 'vermilion pigment');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (34, thai.get_word_id_by_spelling('น้ำ'), thai.get_word_id_by_spelling('ครั่ง'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'แดง'), 34, 'Lac Red', '#dc403e', 3, 'lac insect dye');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (35, thai.get_word_id_by_spelling('แดง'), thai.get_word_id_by_spelling('มณี'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'แดง'), 35, 'Ruby Red', '#ca3c3d', 4, 'gem red');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (36, thai.get_word_id_by_spelling('แดง'), thai.get_word_id_by_spelling('ชาด'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'แดง'), 36, 'Vermilion', '#c9242b', 5, 'cinnabar red pigment');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (37, thai.get_word_id_by_spelling('ดิน'), thai.get_word_id_by_spelling('แดง'), thai.get_word_id_by_spelling('เทศ'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'แดง'), 37, 'Foreign Red Clay', '#98262a', 6, 'foreign earth red');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (38, thai.get_word_id_by_spelling('แดง'), thai.get_word_id_by_spelling('สด'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'แดง'), 38, 'Bright Red', '#FF0000', 7, 'vivid red');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (39, thai.get_word_id_by_spelling('ดิน'), thai.get_word_id_by_spelling('แดง'), thai.get_word_id_by_spelling('ตัด'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'แดง'), 39, 'Deep Red Clay', '#76202a', 8, 'deep earth red');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (40, thai.get_word_id_by_spelling('แดง'), thai.get_word_id_by_spelling('ตัด'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'แดง'), 40, 'Deep Red', '#951519', 9, 'cut/sharp red');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (41, thai.get_word_id_by_spelling('แดง'), thai.get_word_id_by_spelling('เข้ม'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'แดง'), 41, 'Dark Red', '#8B0000', 10, 'dark red');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (42, thai.get_word_id_by_spelling('เลือด'), thai.get_word_id_by_spelling('หมู'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'แดง'), 42, 'Maroon', '#800000', 11, 'pig blood');

-- Orange (ส้ม) - 18 colors
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (43, thai.get_word_id_by_spelling('จำปา'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 43, 'Champak', '#eea31f', 1, 'champak/frangipani flower');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (44, thai.get_word_id_by_spelling('ภู'), thai.get_word_id_by_spelling('อัคนี'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 44, 'Volcano', '#e09179', 2, 'fire mountain');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (45, thai.get_word_id_by_spelling('เมฆ'), thai.get_word_id_by_spelling('สนทยา'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 45, 'Twilight Cloud', '#f68b32', 3, 'twilight cloud');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (46, thai.get_word_id_by_spelling('เปลือก'), thai.get_word_id_by_spelling('ข้าวโพด'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 46, 'Corn Husk', '#f58465', 4, 'corn husk');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (47, thai.get_word_id_by_spelling('มะตูม'), thai.get_word_id_by_spelling('สุก'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 47, 'Ripe Bael', '#d88529', 5, 'ripe bael fruit');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (48, thai.get_word_id_by_spelling('น้ำผึ้ง'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 48, 'Honey', '#d77c29', 6, 'honey');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (49, thai.get_word_id_by_spelling('หมาก'), thai.get_word_id_by_spelling('สุก'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 49, 'Ripe Betel', '#f36e31', 7, 'ripe betel nut');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (50, thai.get_word_id_by_spelling('เหลือง'), thai.get_word_id_by_spelling('ออก'), thai.get_word_id_by_spelling('น้ำตาล'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 50, 'Ochre', '#CC7722', 8, 'yellow toward brown');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (51, thai.get_word_id_by_spelling('เสน'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 51, 'Sappanwood', '#f25821', 9, 'sappanwood dye');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (52, thai.get_word_id_by_spelling('อิฐ'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 52, 'Brick', '#c1621a', 10, 'brick');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (53, thai.get_word_id_by_spelling('ดอก'), thai.get_word_id_by_spelling('ทับทิม'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 53, 'Pomegranate Flower', '#dd5734', 11, 'pomegranate blossom');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (54, thai.get_word_id_by_spelling('ก้าม'), thai.get_word_id_by_spelling('ปู'), thai.get_word_id_by_spelling('อสุรา'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 54, 'Demon Crab', '#d35425', 12, 'demon crab claw');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (55, thai.get_word_id_by_spelling('สัก'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 55, 'Teak', '#b95831', 13, 'teak wood');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (56, thai.get_word_id_by_spelling('ลูก'), thai.get_word_id_by_spelling('พิกุล'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 56, 'Bullet Wood', '#dc4c33', 14, 'bullet wood fruit');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (57, thai.get_word_id_by_spelling('ส้ม'), thai.get_word_id_by_spelling('แดง'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 57, 'Orange Red', '#FF4500', 15, 'orange-red');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (58, thai.get_word_id_by_spelling('ก้าน'), thai.get_word_id_by_spelling('ดอก'), thai.get_word_id_by_spelling('กรรณิการ์'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 58, 'Night Jasmine Stem', '#d14c28', 16, 'night jasmine stem');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (59, thai.get_word_id_by_spelling('ก้าม'), thai.get_word_id_by_spelling('ปู'), thai.get_word_id_by_spelling('หลุด'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 59, 'Molted Crab', '#dc4028', 17, 'molted crab claw');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (60, thai.get_word_id_by_spelling('น้ำ'), thai.get_word_id_by_spelling('หมาก'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ส้ม'), 60, 'Betel Juice', '#a13422', 18, 'betel nut juice');

-- Yellow (เหลือง) - 24 colors
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (61, thai.get_word_id_by_spelling('ดอก'), thai.get_word_id_by_spelling('บวบ'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 61, 'Gourd Flower', '#ffe95e', 1, 'loofah flower');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (62, thai.get_word_id_by_spelling('หรดาล'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 62, 'Orpiment', '#ffde2c', 2, 'arsenic yellow pigment');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (63, thai.get_word_id_by_spelling('เลื่อม'), thai.get_word_id_by_spelling('เหลือง'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 63, 'Yellow Shimmer', '#e7d992', 3, 'yellow shimmer');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (64, thai.get_word_id_by_spelling('หญ้า'), thai.get_word_id_by_spelling('แพรก'), thai.get_word_id_by_spelling('อ่อน'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 64, 'Young Bermuda Grass', '#dcd990', 4, 'young bermuda grass');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (65, thai.get_word_id_by_spelling('จันทร์'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 65, 'Moon', '#f4d25d', 5, 'moon');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (66, thai.get_word_id_by_spelling('เหลือง'), thai.get_word_id_by_spelling('ไพล'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 66, 'Ginger Yellow', '#ead415', 6, 'cassumunar ginger');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (67, thai.get_word_id_by_spelling('นวล'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 67, 'Cream', '#dbcc90', 7, 'soft/creamy');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (68, thai.get_word_id_by_spelling('เลื่อม'), thai.get_word_id_by_spelling('ประภัสสร'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 68, 'Radiant Shimmer', '#cec9a2', 8, 'radiant shimmer');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (69, thai.get_word_id_by_spelling('ลูก'), thai.get_word_id_by_spelling('จันทร์'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 69, 'Nutmeg', '#f2c04e', 9, 'nutmeg');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (70, thai.get_word_id_by_spelling('ไม้'), thai.get_word_id_by_spelling('กฤษณา'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 70, 'Agarwood', '#e5be82', 10, 'agarwood/aloeswood');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (71, thai.get_word_id_by_spelling('นวล'), thai.get_word_id_by_spelling('จันทร์'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 71, 'Moonlight', '#dcc169', 11, 'soft moonlight');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (72, thai.get_word_id_by_spelling('ทอง'), thai.get_word_id_by_spelling('ดอก'), thai.get_word_id_by_spelling('บวบ'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 72, 'Gourd Gold', '#ddc153', 12, 'gourd flower gold');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (73, thai.get_word_id_by_spelling('เหลือง'), thai.get_word_id_by_spelling('อำพัน'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 73, 'Amber', '#FFBF00', 13, 'amber');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (74, thai.get_word_id_by_spelling('ฟาง'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 74, 'Straw', '#daba8a', 14, 'rice straw');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (75, thai.get_word_id_by_spelling('รง'), thai.get_word_id_by_spelling('ทอง'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 75, 'Gamboge Gold', '#e8bb1f', 15, 'gamboge resin gold');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (76, thai.get_word_id_by_spelling('ทอง'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 76, 'Gold', '#d1b654', 16, 'gold');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (77, thai.get_word_id_by_spelling('มุก'), thai.get_word_id_by_spelling('สุข'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 77, 'Lucky Pearl', '#c7ae7b', 17, 'lustrous pearl');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (78, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('มะพูด'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 78, 'Garcinia Green', '#bcb531', 18, 'garcinia fruit green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (79, thai.get_word_id_by_spelling('ทอง'), thai.get_word_id_by_spelling('ชมพูนุช'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 79, 'Rose Gold', '#dba73b', 19, 'rose gold');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (80, thai.get_word_id_by_spelling('เหลือง'), thai.get_word_id_by_spelling('เพียง'), thai.get_word_id_by_spelling('ทอง'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 80, 'Golden Yellow', '#c0ac23', 20, 'yellow like gold');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (81, thai.get_word_id_by_spelling('ขี้ผึ้ง'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 81, 'Beeswax', '#d09b2a', 21, 'beeswax');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (82, thai.get_word_id_by_spelling('เหลือง'), thai.get_word_id_by_spelling('ดิน'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 82, 'Earth Yellow', '#be9c25', 22, 'earthy yellow');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (83, thai.get_word_id_by_spelling('สัมฤทธิ์'), thai.get_word_id_by_spelling('ศักดิ์'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 83, 'Noble Bronze', '#a1966d', 23, 'prestigious bronze');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (84, thai.get_word_id_by_spelling('ทอง'), thai.get_word_id_by_spelling('คำ'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เหลือง'), 84, 'Pure Gold', '#bd9425', 24, 'pure gold');

-- Green (เขียว) - 20 colors
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (85, thai.get_word_id_by_spelling('เหลือง'), thai.get_word_id_by_spelling('แกม'), thai.get_word_id_by_spelling('เขียว'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 85, 'Lime', '#BFFF00', 1, 'yellow-tinted green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (86, thai.get_word_id_by_spelling('โศก'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 86, 'Ashoka', '#84c299', 2, 'ashoka tree');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (87, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ไข่'), thai.get_word_id_by_spelling('กา'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 87, 'Crow Egg Green', '#93b692', 3, 'crow egg green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (88, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('นก'), thai.get_word_id_by_spelling('กะลิง'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 88, 'Parakeet Green', '#88ab68', 4, 'parakeet green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (89, thai.get_word_id_by_spelling('ตอง'), thai.get_word_id_by_spelling('อ่อน'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 89, 'Young Banana Leaf', '#919b4a', 5, 'young banana leaf');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (90, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('นวล'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 90, 'Soft Green', '#58986f', 6, 'pale green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (91, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ถั่ว'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 91, 'Pea Green', '#6d953a', 7, 'bean/pea green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (92, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('รงกา'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 92, 'Gamboge Tree Green', '#5f8d56', 8, 'gamboge tree green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (93, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ไพร'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 93, 'Forest Green', '#687840', 9, 'forest green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (94, thai.get_word_id_by_spelling('ไม้'), thai.get_word_id_by_spelling('ไผ่'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 94, 'Bamboo', '#278766', 10, 'bamboo');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (95, thai.get_word_id_by_spelling('มรกต'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 95, 'Emerald', '#009058', 11, 'emerald');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (96, thai.get_word_id_by_spelling('มหาดไทย'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 96, 'Interior Ministry', '#657249', 12, 'ministry olive drab');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (97, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ขี้'), thai.get_word_id_by_spelling('ม้า'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 97, 'Horse Dung Green', '#6b6b3e', 13, 'horse dung green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (98, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ขจี'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 98, 'Verdant Green', '#008068', 14, 'lush verdant');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (99, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ตอง'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 99, 'Banana Leaf Green', '#21723e', 15, 'banana leaf green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (100, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ดิน'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 100, 'Earth Green', '#356740', 16, 'earthy green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (101, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('เข้ม'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 101, 'Dark Green', '#006400', 17, 'dark green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (102, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ใบ'), thai.get_word_id_by_spelling('แค'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 102, 'Sesbania Leaf', '#145139', 18, 'sesbania leaf');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (103, thai.get_word_id_by_spelling('ปีก'), thai.get_word_id_by_spelling('กา'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 103, 'Crow Wing', '#364540', 19, 'crow wing');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (104, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ก้าม'), thai.get_word_id_by_spelling('ปู'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เขียว'), 104, 'Crab Claw Green', '#17372a', 20, 'dark crab claw green');

-- Cyan (น้ำเงินเขียว) - 27 colors
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (105, thai.get_word_id_by_spelling('ฟ้า'), thai.get_word_id_by_spelling('เมฆ'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 105, 'Cloud Blue', '#87CEEB', 1, 'cloud blue/azure');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (106, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ขุ่น'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 106, 'Turquoise', '#40E0D0', 2, 'murky green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (107, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ขี้'), thai.get_word_id_by_spelling('นก'), thai.get_word_id_by_spelling('การเวท'), NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 107, 'Magic Bird Dropping', '#7dbfb8', 3, 'magic bird dropping');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (108, thai.get_word_id_by_spelling('น้ำเงิน'), thai.get_word_id_by_spelling('เขียว'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 108, 'Cyan', '#00CED1', 4, 'cyan');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (109, thai.get_word_id_by_spelling('ก้าน'), thai.get_word_id_by_spelling('มะลิ'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 109, 'Jasmine Stem', '#69b496', 5, 'jasmine stem');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (110, thai.get_word_id_by_spelling('น้ำ'), thai.get_word_id_by_spelling('ไหล'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 110, 'Flowing Water', '#5aaebd', 6, 'flowing water');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (111, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ตั้งแช'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 111, 'Chinese Green', '#45ab87', 7, 'Chinese green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (112, thai.get_word_id_by_spelling('ตา'), thai.get_word_id_by_spelling('แมว'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 112, 'Cat Eye', '#4d94ac', 8, 'cat eye');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (113, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ตะพุ่น'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 113, 'Tapoon Green', '#5192a1', 9, 'tapoon plant green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (114, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ก้าน'), thai.get_word_id_by_spelling('ตอง'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 114, 'Banana Stem Green', '#269a82', 10, 'banana leaf stem');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (115, thai.get_word_id_by_spelling('ฟ้า'), thai.get_word_id_by_spelling('เขียว'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 115, 'Blue Green', '#0D98BA', 11, 'blue-green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (116, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ไข่'), thai.get_word_id_by_spelling('ครุฑ'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 116, 'Garuda Egg', '#00968f', 12, 'garuda egg green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (117, thai.get_word_id_by_spelling('ขน'), thai.get_word_id_by_spelling('คอ'), thai.get_word_id_by_spelling('นาง'), thai.get_word_id_by_spelling('นกยูง'), NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 117, 'Peacock Neck Feather', '#008d9b', 13, 'peacock neck feather');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (118, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('หัว'), thai.get_word_id_by_spelling('เป็ด'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 118, 'Duck Head', '#008080', 14, 'teal/duck head');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (119, thai.get_word_id_by_spelling('ฟ้า'), thai.get_word_id_by_spelling('เข้ม'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 119, 'Cerulean', '#007BA7', 15, 'deep sky blue');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (120, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ขี้'), thai.get_word_id_by_spelling('ทอง'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 120, 'Verdigris', '#00796e', 16, 'gold dross/verdigris');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (121, thai.get_word_id_by_spelling('น้ำเงิน'), thai.get_word_id_by_spelling('นก'), thai.get_word_id_by_spelling('พิราบ'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 121, 'Pigeon Blue', '#0f697c', 17, 'pigeon blue');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (122, thai.get_word_id_by_spelling('ตา'), thai.get_word_id_by_spelling('กุ้ง'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 122, 'Shrimp Eye', '#296169', 18, 'shrimp eye');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (123, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('ขาบ'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 123, 'Indigo Tinted Green', '#00676e', 19, 'indigo-tinted green');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (124, thai.get_word_id_by_spelling('สัมฤทธิ์'), thai.get_word_id_by_spelling('คุณ'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 124, 'Virtuous Bronze', '#165d5a', 20, 'virtuous bronze');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (125, thai.get_word_id_by_spelling('เข้ม'), thai.get_word_id_by_spelling('ขาบ'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 125, 'Deep Indigo', '#005465', 21, 'deep indigo');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (126, thai.get_word_id_by_spelling('คราม'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 126, 'Indigo', '#16466e', 22, 'indigo plant dye');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (127, thai.get_word_id_by_spelling('ขาบ'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 127, 'Dark Indigo', '#1a465a', 23, 'dark indigo');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (128, thai.get_word_id_by_spelling('เขม่า'), thai.get_word_id_by_spelling('ยาง'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 128, 'Rubber Soot', '#223640', 24, 'rubber soot');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (129, thai.get_word_id_by_spelling('เมฆ'), thai.get_word_id_by_spelling('คราม'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 129, 'Indigo Cloud', '#072f4f', 25, 'indigo cloud');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (130, thai.get_word_id_by_spelling('ขาบ'), thai.get_word_id_by_spelling('ดำ'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 130, 'Black Indigo', '#042738', 26, 'black indigo');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (131, thai.get_word_id_by_spelling('เขียว'), thai.get_word_id_by_spelling('นิล'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงินเขียว'), 131, 'Sapphire Green', '#002835', 27, 'sapphire/dark gem green');

-- Sky Blue (ฟ้า) - 4 colors
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (132, thai.get_word_id_by_spelling('ขาว'), thai.get_word_id_by_spelling('ขาบ'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ฟ้า'), 132, 'Indigo White', '#c1d4ce', 1, 'indigo-tinted white');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (133, thai.get_word_id_by_spelling('เมฆ'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ฟ้า'), 133, 'Cloud', '#93adbd', 2, 'cloud');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (134, thai.get_word_id_by_spelling('มอ'), thai.get_word_id_by_spelling('คราม'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ฟ้า'), 134, 'Faded Indigo', '#577896', 3, 'faded indigo');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (135, thai.get_word_id_by_spelling('ฟ้า'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ฟ้า'), 135, 'Sky Blue', '#3476ae', 4, 'sky');

-- Navy Blue (น้ำเงิน) - 5 colors
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (136, thai.get_word_id_by_spelling('ท้อง'), thai.get_word_id_by_spelling('ฟ้า'), thai.get_word_id_by_spelling('คราม'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงิน'), 136, 'Indigo Sky', '#4169E1', 1, 'indigo sky');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (137, thai.get_word_id_by_spelling('ฟ้า'), thai.get_word_id_by_spelling('แก่'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงิน'), 137, 'Old Sky', '#1E5AA8', 2, 'dark sky blue');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (138, thai.get_word_id_by_spelling('ไพล'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงิน'), 138, 'Sapphire', '#0F52BA', 3, 'sapphire blue');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (139, thai.get_word_id_by_spelling('คราม'), thai.get_word_id_by_spelling('ฝรั่ง'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงิน'), 139, 'Western Indigo', '#0c4da2', 4, 'western indigo');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (140, thai.get_word_id_by_spelling('กรมท่า'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำเงิน'), 140, 'Navy', '#1b2c55', 5, 'navy (Dept of Harbors)');

-- Purple (ม่วง) - 18 colors
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (141, thai.get_word_id_by_spelling('ม่วง'), thai.get_word_id_by_spelling('ซีด'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 141, 'Mauve', '#E0B0FF', 1, 'faded purple/mauve');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (142, thai.get_word_id_by_spelling('เผือก'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 142, 'Taro', '#bd9dc2', 2, 'taro');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (143, thai.get_word_id_by_spelling('ม่วง'), thai.get_word_id_by_spelling('แกม'), thai.get_word_id_by_spelling('แดง'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 143, 'Violet', '#EE82EE', 3, 'red-tinted purple');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (144, thai.get_word_id_by_spelling('ม่วง'), thai.get_word_id_by_spelling('ดอก'), thai.get_word_id_by_spelling('ตะแบก'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 144, 'Lagerstroemia', '#b579a6', 4, 'lagerstroemia flower');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (145, thai.get_word_id_by_spelling('ม่วง'), thai.get_word_id_by_spelling('ดอก'), thai.get_word_id_by_spelling('ผักตบ'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 145, 'Water Hyacinth', '#776fa5', 5, 'water hyacinth flower');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (146, thai.get_word_id_by_spelling('พวง'), thai.get_word_id_by_spelling('อังกาบ'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 146, 'Philippine Violet', '#876aa4', 6, 'Philippine violet bunch');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (147, thai.get_word_id_by_spelling('ม่วง'), thai.get_word_id_by_spelling('เขียว'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 147, 'Green Purple', '#7f616d', 7, 'greenish purple');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (148, thai.get_word_id_by_spelling('ม่วง'), thai.get_word_id_by_spelling('ชาด'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 148, 'Vermilion Purple', '#a5546a', 8, 'vermilion purple');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (149, thai.get_word_id_by_spelling('ม่วง'), thai.get_word_id_by_spelling('ตอง'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 149, 'Banana Leaf Purple', '#9f4a57', 9, 'banana leaf purple');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (150, thai.get_word_id_by_spelling('ดอก'), thai.get_word_id_by_spelling('อัญชัน'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 150, 'Butterfly Pea', '#695095', 10, 'butterfly pea flower');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (151, thai.get_word_id_by_spelling('ม่วง'), thai.get_word_id_by_spelling('เข้ม'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 151, 'Plum', '#8E4585', 11, 'deep purple/plum');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (152, thai.get_word_id_by_spelling('ม่วง'), thai.get_word_id_by_spelling('น้ำเงิน'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 152, 'Blue Violet', '#8A2BE2', 12, 'blue violet');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (153, thai.get_word_id_by_spelling('แดง'), thai.get_word_id_by_spelling('อม'), thai.get_word_id_by_spelling('ม่วง'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 153, 'Red Violet', '#C71585', 13, 'red-tinted purple');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (154, thai.get_word_id_by_spelling('ม่วง'), thai.get_word_id_by_spelling('เม็ด'), thai.get_word_id_by_spelling('มะปราง'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 154, 'Plum Mango Seed', '#702c7d', 14, 'plum mango seed');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (155, thai.get_word_id_by_spelling('ม่วง'), thai.get_word_id_by_spelling('แก่'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 155, 'Deep Purple', '#432a64', 15, 'deep/old purple');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (156, thai.get_word_id_by_spelling('ม่วง'), thai.get_word_id_by_spelling('คราม'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 156, 'Indigo Purple', '#3a2d53', 16, 'indigo purple');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (157, thai.get_word_id_by_spelling('ลูก'), thai.get_word_id_by_spelling('หว้า'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 157, 'Java Plum', '#400643', 17, 'java plum fruit');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (158, thai.get_word_id_by_spelling('กลาโหม'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ม่วง'), 158, 'Defense Purple', '#2a0139', 18, 'Ministry of Defense purple');

-- Pink (ชมพู) - 22 colors
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (159, thai.get_word_id_by_spelling('เหลือง'), thai.get_word_id_by_spelling('อม'), thai.get_word_id_by_spelling('ชมพู'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 159, 'Peach', '#FFDAB9', 1, 'peach');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (160, thai.get_word_id_by_spelling('เหลือง'), thai.get_word_id_by_spelling('ส้ม'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 160, 'Apricot', '#FBCEB1', 2, 'apricot');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (161, thai.get_word_id_by_spelling('ควาย'), thai.get_word_id_by_spelling('เผือก'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 161, 'Albino Buffalo', '#efc5b5', 3, 'albino buffalo');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (162, thai.get_word_id_by_spelling('กลีบ'), thai.get_word_id_by_spelling('บัว'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 162, 'Lotus Petal', '#edbbc6', 4, 'lotus petal');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (163, thai.get_word_id_by_spelling('เลื่อม'), thai.get_word_id_by_spelling('พราย'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 163, 'Glitter Shimmer', '#e3c0ab', 5, 'glitter shimmer');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (164, thai.get_word_id_by_spelling('ดิน'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 164, 'Earth', '#e3a377', 6, 'earth/soil');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (165, thai.get_word_id_by_spelling('กุหลาบ'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 165, 'Rose', '#f698a3', 7, 'rose flower');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (166, thai.get_word_id_by_spelling('ช้าง'), thai.get_word_id_by_spelling('เผือก'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 166, 'White Elephant', '#e09a83', 8, 'white elephant');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (167, thai.get_word_id_by_spelling('แดง'), thai.get_word_id_by_spelling('ดอก'), thai.get_word_id_by_spelling('กระมุท'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 167, 'Lotus Bud Pink', '#d6999e', 9, 'lotus bud red');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (168, thai.get_word_id_by_spelling('ผุ่น'), thai.get_word_id_by_spelling('แดง'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 168, 'Red Powder', '#df92a3', 10, 'red powder');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (169, thai.get_word_id_by_spelling('นิลุบล'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 169, 'Blue Water Lily', '#ac83a8', 11, 'blue water lily (Pali)');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (170, thai.get_word_id_by_spelling('ดอก'), thai.get_word_id_by_spelling('บาน'), thai.get_word_id_by_spelling('เย็น'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 170, 'Four O''Clock Flower', '#f27091', 12, 'four o''clock flower');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (171, thai.get_word_id_by_spelling('ดอก'), thai.get_word_id_by_spelling('อุบล'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 171, 'Water Lily', '#dd6d91', 13, 'water lily flower');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (172, thai.get_word_id_by_spelling('บัว'), thai.get_word_id_by_spelling('โรย'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 172, 'Wilted Lotus', '#ca6a7a', 14, 'faded lotus');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (173, thai.get_word_id_by_spelling('ท้อง'), thai.get_word_id_by_spelling('สิงห์'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 173, 'Lion''s Belly', '#dc577e', 15, 'lion''s belly');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (174, thai.get_word_id_by_spelling('กะปิ'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 174, 'Shrimp Paste', '#a5505b', 16, 'shrimp paste');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (175, thai.get_word_id_by_spelling('เกสร'), thai.get_word_id_by_spelling('ชมพู่'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 175, 'Rose Apple Stamen', '#db3b7b', 17, 'rose apple stamen');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (176, thai.get_word_id_by_spelling('แดง'), thai.get_word_id_by_spelling('ยอ'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 176, 'Noni Red', '#c93b5e', 18, 'noni dye red');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (177, thai.get_word_id_by_spelling('บาน'), thai.get_word_id_by_spelling('เย็น'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 177, 'Four O''Clock', '#FF1493', 19, 'four o''clock flower');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (178, thai.get_word_id_by_spelling('ลิ้นจี่'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 178, 'Lychee', '#a62b4c', 20, 'lychee fruit');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (179, thai.get_word_id_by_spelling('แดง'), thai.get_word_id_by_spelling('กุหลาบ'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 179, 'Rose Red', '#FF007F', 21, 'rose red');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (180, thai.get_word_id_by_spelling('เปลือก'), thai.get_word_id_by_spelling('มังคุด'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ชมพู'), 180, 'Mangosteen Peel', '#6e2c4f', 22, 'mangosteen peel');

-- Brown (น้ำตาล) - 13 colors
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (181, thai.get_word_id_by_spelling('กากี'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำตาล'), 181, 'Khaki', '#a19078', 1, 'khaki');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (182, thai.get_word_id_by_spelling('น้ำตาล'), thai.get_word_id_by_spelling('เหลือง'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำตาล'), 182, 'Yellow Brown', '#967117', 2, 'brownish yellow');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (183, thai.get_word_id_by_spelling('กรัก'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำตาล'), 183, 'Catechu', '#8a5929', 3, 'acacia catechu dye');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (184, thai.get_word_id_by_spelling('ดิน'), thai.get_word_id_by_spelling('แดง'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำตาล'), 184, 'Red Clay', '#984c36', 4, 'red clay');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (185, thai.get_word_id_by_spelling('น้ำตาล'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำตาล'), 185, 'Brown', '#7b4a34', 5, 'sugar/brown');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (186, thai.get_word_id_by_spelling('สัมฤทธิ์'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำตาล'), 186, 'Bronze', '#584e42', 6, 'bronze');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (187, thai.get_word_id_by_spelling('สัมฤทธิ์'), thai.get_word_id_by_spelling('โชค'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำตาล'), 187, 'Lucky Bronze', '#5c4606', 7, 'lucky bronze');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (188, thai.get_word_id_by_spelling('สัมฤทธิ์'), thai.get_word_id_by_spelling('ผล'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำตาล'), 188, 'Fruitful Bronze', '#613f1c', 8, 'fruitful bronze');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (189, thai.get_word_id_by_spelling('ฝาด'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำตาล'), 189, 'Tannic', '#6f2e28', 9, 'tannin/bark');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (190, thai.get_word_id_by_spelling('น้ำตาล'), thai.get_word_id_by_spelling('ไหม้'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำตาล'), 190, 'Burnt Sugar', '#522e1a', 10, 'caramel');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (191, thai.get_word_id_by_spelling('น้ำตาล'), thai.get_word_id_by_spelling('เข้ม'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำตาล'), 191, 'Dark Brown', '#3d2314', 11, 'dark brown');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (192, thai.get_word_id_by_spelling('เหล็ก'), thai.get_word_id_by_spelling('ไหล'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำตาล'), 192, 'Meteorite Iron', '#29241b', 12, 'flowing iron');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (193, thai.get_word_id_by_spelling('น้ำ'), thai.get_word_id_by_spelling('รัก'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'น้ำตาล'), 193, 'Lacquer', '#2b1c18', 13, 'lacquer tree sap');

-- Grey (เทา) - 15 colors
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (194, thai.get_word_id_by_spelling('เงิน'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เทา'), 194, 'Silver', '#bcc4c1', 1, 'silver');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (195, thai.get_word_id_by_spelling('เหล็ก'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เทา'), 195, 'Steel', '#bcbdb8', 2, 'iron/steel');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (196, thai.get_word_id_by_spelling('ดอก'), thai.get_word_id_by_spelling('เลา'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เทา'), 196, 'Reed Flower', '#b3b3ba', 3, 'reed flower');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (197, thai.get_word_id_by_spelling('นวล'), thai.get_word_id_by_spelling('เทา'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เทา'), 197, 'Soft Grey', '#b4ae9b', 4, 'soft grey');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (198, thai.get_word_id_by_spelling('เหลือง'), thai.get_word_id_by_spelling('เทา'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เทา'), 198, 'Grey Yellow', '#b4a790', 5, 'greyish yellow');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (199, thai.get_word_id_by_spelling('เทา'), thai.get_word_id_by_spelling('เขียว'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เทา'), 199, 'Green Grey', '#93a9a4', 6, 'greenish grey');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (200, thai.get_word_id_by_spelling('ปูน'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เทา'), 200, 'Cement', '#9fa49f', 7, 'lime/cement');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (201, thai.get_word_id_by_spelling('ควัน'), thai.get_word_id_by_spelling('เพลิง'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เทา'), 201, 'Fire Smoke', '#9d9081', 8, 'fire smoke');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (202, thai.get_word_id_by_spelling('มอ'), thai.get_word_id_by_spelling('หมึก'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เทา'), 202, 'Faded Ink', '#778a83', 9, 'faded ink');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (203, thai.get_word_id_by_spelling('เทา'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เทา'), 203, 'Grey', '#7f8585', 10, 'grey');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (204, thai.get_word_id_by_spelling('สวาด'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เทา'), 204, 'Charcoal', '#6a6e78', 11, 'charcoal');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (205, thai.get_word_id_by_spelling('ลูก'), thai.get_word_id_by_spelling('ห่าน'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เทา'), 205, 'Gosling', '#6b6272', 12, 'gosling');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (206, thai.get_word_id_by_spelling('ดำ'), thai.get_word_id_by_spelling('หมึก'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เทา'), 206, 'Ink Black', '#5a5b60', 13, 'ink black');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (207, thai.get_word_id_by_spelling('สัมฤทธิ์'), thai.get_word_id_by_spelling('เดช'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เทา'), 207, 'Powerful Bronze', '#444740', 14, 'powerful bronze');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (208, thai.get_word_id_by_spelling('หมึก'), thai.get_word_id_by_spelling('จีน'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'เทา'), 208, 'Chinese Ink', '#363c41', 15, 'Chinese ink');

-- Black (ดำ) - 3 colors
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (209, thai.get_word_id_by_spelling('ดำ'), thai.get_word_id_by_spelling('มืด'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ดำ'), 209, 'Pitch Black', '#0a0a0a', 1, 'pitch black');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (210, thai.get_word_id_by_spelling('นิล'), thai.get_word_id_by_spelling('กาฬ'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ดำ'), 210, 'Black Sapphire', '#00080b', 2, 'black sapphire');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (211, thai.get_word_id_by_spelling('ดำ'), thai.get_word_id_by_spelling('เขม่า'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ดำ'), 211, 'Soot Black', '#000000', 3, 'soot black');

-- White (ขาว) - 9 colors
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (212, thai.get_word_id_by_spelling('งา'), thai.get_word_id_by_spelling('ช้าง'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ขาว'), 212, 'Ivory', '#fffde0', 1, 'elephant tusk');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (213, thai.get_word_id_by_spelling('เบจ'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ขาว'), 213, 'Beige', '#F5F5DC', 2, 'beige');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (214, thai.get_word_id_by_spelling('สังข์'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ขาว'), 214, 'Conch', '#fff1d0', 3, 'conch shell');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (215, thai.get_word_id_by_spelling('ขาว'), thai.get_word_id_by_spelling('ผ่อง'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ขาว'), 215, 'Bright White', '#f1ede2', 4, 'bright white');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (216, thai.get_word_id_by_spelling('ฟ้า'), thai.get_word_id_by_spelling('แลบ'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ขาว'), 216, 'Lightning', '#efdae0', 5, 'sky lightning');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (217, thai.get_word_id_by_spelling('ขาว'), thai.get_word_id_by_spelling('ปน'), thai.get_word_id_by_spelling('เพรช'), NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ขาว'), 217, 'Diamond White', '#e5dbc0', 6, 'diamond-mixed white');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (218, thai.get_word_id_by_spelling('เนื้อ'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ขาว'), 218, 'Flesh', '#f1d7b3', 7, 'flesh/skin');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (219, thai.get_word_id_by_spelling('ขาว'), thai.get_word_id_by_spelling('กะบัง'), NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ขาว'), 219, 'Shield White', '#d0cfcf', 8, 'shield white');
INSERT INTO thai."CompositeName" (id, word1, word2, word3, word4, word5) VALUES (220, thai.get_word_id_by_spelling('หมอก'), NULL, NULL, NULL, NULL);
INSERT INTO thai."Color" (hue_family_id, composite_name_id, english_label, hex, luminance_order, notes) VALUES ((SELECT id FROM thai."HueFamily" WHERE thai_name = 'ขาว'), 220, 'Mist', '#dbccb9', 9, 'mist/fog');

SELECT setval('thai."CompositeName_id_seq"', 220);

-- ============================================================================
-- PHASE 9: Cleanup helper function
-- ============================================================================
DROP FUNCTION thai.get_word_id_by_spelling(VARCHAR);

-- Total: 189 colors in 13 families
