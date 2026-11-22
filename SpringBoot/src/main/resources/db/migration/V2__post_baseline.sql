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

ALTER TABLE thai."Position"
    ADD COLUMN composite_name_id integer,
    ADD COLUMN lexeme character varying(32);

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

CREATE TABLE thai."HueFamily" (
    id SERIAL PRIMARY KEY,
    thai_name VARCHAR(50) NOT NULL UNIQUE,
    english_name VARCHAR(50) NOT NULL,
    hex_representative CHAR(7) NOT NULL,
    display_order SMALLINT NOT NULL UNIQUE
);

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
