CREATE SCHEMA IF NOT EXISTS thai;

CREATE TABLE thai."CelestialBody" (
    id integer NOT NULL,
    ordinal integer NOT NULL,
    glyph character varying(32) NOT NULL,
    color_hex character varying(16) NOT NULL,
    word_id integer NOT NULL
);

CREATE SEQUENCE thai."CelestialBody_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."CelestialBody_id_seq" OWNED BY thai."CelestialBody".id;

CREATE TABLE thai."Class" (
    id integer NOT NULL,
    name character varying(15) NOT NULL
);

CREATE SEQUENCE thai."Class_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Class_id_seq" OWNED BY thai."Class".id;

CREATE TABLE thai."Cluster" (
    id integer NOT NULL,
    type character varying(64) NOT NULL,
    subtype character varying(64),
    value character varying(64) NOT NULL
);

CREATE TABLE thai."ClusterWord" (
    id integer NOT NULL,
    cluster_id integer NOT NULL,
    word_id integer NOT NULL
);

CREATE SEQUENCE thai."ClusterWord_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."ClusterWord_id_seq" OWNED BY thai."ClusterWord".id;

CREATE SEQUENCE thai."Cluster_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Cluster_id_seq" OWNED BY thai."Cluster".id;

CREATE TABLE thai."CompositeName" (
    id integer NOT NULL,
    word1 integer,
    word2 integer,
    word3 integer,
    word4 integer,
    word5 integer
);

CREATE SEQUENCE thai."CompositeName_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."CompositeName_id_seq" OWNED BY thai."CompositeName".id;

CREATE TABLE thai."Consonant" (
    id integer NOT NULL,
    grapheme character varying(1) NOT NULL,
    phoneme character varying(2) NOT NULL,
    soundtype character varying(10) NOT NULL,
    class integer NOT NULL,
    ending integer NOT NULL,
    transcription integer,
    acrophonic integer,
    mediapath text,
    initial integer,
    final integer
);

CREATE TABLE thai."ConsonantSample" (
    id integer NOT NULL,
    consonant integer,
    word integer
);

CREATE SEQUENCE thai."ConsonantSample_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."ConsonantSample_id_seq" OWNED BY thai."ConsonantSample".id;

CREATE SEQUENCE thai."Consonant_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Consonant_id_seq" OWNED BY thai."Consonant".id;

CREATE TABLE thai."DayOfWeek" (
    id integer NOT NULL,
    ordinal integer NOT NULL,
    word_id integer NOT NULL,
    deity text,
    image_url character varying(2048)
);

CREATE SEQUENCE thai."DayOfWeek_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."DayOfWeek_id_seq" OWNED BY thai."DayOfWeek".id;

CREATE TABLE thai."Ending" (
    id integer NOT NULL,
    name character varying(15) NOT NULL
);

CREATE SEQUENCE thai."Ending_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Ending_id_seq" OWNED BY thai."Ending".id;

CREATE TABLE thai."Formality" (
    id integer NOT NULL,
    name text
);

CREATE SEQUENCE thai."Formality_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Formality_id_seq" OWNED BY thai."Formality".id;

CREATE TABLE thai."Gender" (
    id integer NOT NULL,
    name text
);

CREATE SEQUENCE thai."Gender_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Gender_id_seq" OWNED BY thai."Gender".id;

CREATE TABLE thai."Hour" (
    id integer NOT NULL,
    ordinal integer NOT NULL,
    word_id integer NOT NULL
);

CREATE SEQUENCE thai."Hour_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Hour_id_seq" OWNED BY thai."Hour".id;

CREATE TABLE thai."Lexeme" (
    id integer NOT NULL,
    value text,
    part_of_speech character varying(50),
    tags character varying(64)
);

CREATE SEQUENCE thai."Lexeme_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Lexeme_id_seq" OWNED BY thai."Lexeme".id;

CREATE TABLE thai."MinimalSet" (
    id integer NOT NULL,
    label text
);

CREATE TABLE thai."MinimalSetWord" (
    id integer NOT NULL,
    set_id integer NOT NULL,
    word_id integer NOT NULL
);

CREATE TABLE thai."Month" (
    id integer NOT NULL,
    ordinal integer NOT NULL,
    word_id integer NOT NULL,
    short_name character varying(16) NOT NULL,
    english_name character varying(32) NOT NULL,
    zodiac_id integer NOT NULL
);

CREATE SEQUENCE thai."Month_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Month_id_seq" OWNED BY thai."Month".id;

CREATE TABLE thai."Numeral" (
    id integer NOT NULL,
    word integer,
    grapheme text
);

CREATE SEQUENCE thai."Numeral_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Numeral_id_seq" OWNED BY thai."Numeral".id;

CREATE TABLE thai."Plurality" (
    id integer NOT NULL,
    name text
);

CREATE SEQUENCE thai."Plurality_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Plurality_id_seq" OWNED BY thai."Plurality".id;

CREATE TABLE thai."Position" (
    id integer NOT NULL,
    row_index integer NOT NULL,
    col_index integer NOT NULL,
    name text,
    tile_style character varying(32)
);

CREATE TABLE thai."PositionWord" (
    id integer NOT NULL,
    position_id integer NOT NULL,
    word_id integer NOT NULL,
    word_order integer NOT NULL
);

CREATE SEQUENCE thai."PositionWord_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."PositionWord_id_seq" OWNED BY thai."PositionWord".id;

CREATE SEQUENCE thai."Position_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Position_id_seq" OWNED BY thai."Position".id;

CREATE TABLE thai."Pronoun" (
    id integer NOT NULL,
    word integer,
    gender integer,
    formality integer,
    plurality integer,
    grammatical_person integer
);

CREATE TABLE thai."PronounType" (
    id integer NOT NULL,
    name text
);

CREATE TABLE thai."PronounTypeBreakout" (
    id integer NOT NULL,
    pronoun_id integer,
    type_id integer
);

CREATE SEQUENCE thai."PronounTypeBreakout_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."PronounTypeBreakout_id_seq" OWNED BY thai."PronounTypeBreakout".id;

CREATE SEQUENCE thai."PronounType_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."PronounType_id_seq" OWNED BY thai."PronounType".id;

CREATE SEQUENCE thai."Pronoun_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Pronoun_id_seq" OWNED BY thai."Pronoun".id;

CREATE TABLE thai."Sound" (
    id integer NOT NULL,
    mediapath text,
    initial_only boolean NOT NULL,
    consonant_sound integer,
    sound_type integer,
    value text NOT NULL
);

CREATE TABLE thai."SoundType" (
    id integer NOT NULL,
    name character varying(15) NOT NULL
);

CREATE SEQUENCE thai."SoundType_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."SoundType_id_seq" OWNED BY thai."SoundType".id;

CREATE SEQUENCE thai."Sound_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Sound_id_seq" OWNED BY thai."Sound".id;

CREATE TABLE thai."SpecialSymbol" (
    id integer NOT NULL,
    grapheme text,
    word_id integer
);

CREATE SEQUENCE thai."SpecialSymbol_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."SpecialSymbol_id_seq" OWNED BY thai."SpecialSymbol".id;

CREATE TABLE thai."Spelling" (
    id integer NOT NULL,
    value character varying(25) NOT NULL
);

CREATE SEQUENCE thai."Spelling_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Spelling_id_seq" OWNED BY thai."Spelling".id;

CREATE TABLE thai."TimeOfDay" (
    id integer NOT NULL,
    ordinal integer NOT NULL,
    word_id integer NOT NULL
);

CREATE SEQUENCE thai."TimeOfDay_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."TimeOfDay_id_seq" OWNED BY thai."TimeOfDay".id;

CREATE TABLE thai."ToneMark" (
    id integer NOT NULL,
    mark character varying(2) NOT NULL,
    name integer,
    mediapath text,
    english_label text
);

CREATE SEQUENCE thai."ToneMark_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."ToneMark_id_seq" OWNED BY thai."ToneMark".id;

CREATE TABLE thai."Transcription" (
    id integer NOT NULL,
    phonemic character varying(25),
    paiboon character varying(25),
    "IPA" text
);

CREATE SEQUENCE thai."Transcription_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Transcription_id_seq" OWNED BY thai."Transcription".id;

CREATE TABLE thai."Vowel" (
    id integer NOT NULL,
    sound_id integer,
    is_diphtong boolean,
    mediapath text
);

CREATE TABLE thai."VowelGrapheme" (
    id integer NOT NULL,
    vowel_id integer,
    value text,
    is_closed boolean
);

CREATE SEQUENCE thai."VowelGrapheme_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."VowelGrapheme_id_seq" OWNED BY thai."VowelGrapheme".id;

CREATE TABLE thai."VowelSample" (
    id integer NOT NULL,
    vowel integer,
    word integer
);

CREATE SEQUENCE thai."VowelSample_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."VowelSample_id_seq" OWNED BY thai."VowelSample".id;

CREATE SEQUENCE thai."Vowel_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Vowel_id_seq" OWNED BY thai."Vowel".id;

CREATE TABLE thai."Word" (
    id integer NOT NULL,
    transcription integer NOT NULL,
    spelling_id integer NOT NULL,
    is_primary boolean DEFAULT true NOT NULL
);

CREATE TABLE thai."WordLexeme" (
    id integer NOT NULL,
    word integer,
    lexeme integer
);

CREATE SEQUENCE thai."WordLexeme_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."WordLexeme_id_seq" OWNED BY thai."WordLexeme".id;

CREATE MATERIALIZED VIEW thai."WordView" AS
 SELECT w.id,
    w.spelling_id,
    w.transcription AS transcription_id,
    sp.value AS spelling,
    tr.phonemic,
    tr.paiboon,
    tr."IPA" AS ipa,
    COALESCE(array_agg(l.value ORDER BY l.value) FILTER (WHERE (l.value IS NOT NULL)), ARRAY[]::text[]) AS lexemes
   FROM ((((thai."Word" w
     JOIN thai."Spelling" sp ON ((sp.id = w.spelling_id)))
     LEFT JOIN thai."Transcription" tr ON ((tr.id = w.transcription)))
     LEFT JOIN thai."WordLexeme" wl ON ((wl.word = w.id)))
     LEFT JOIN thai."Lexeme" l ON ((l.id = wl.lexeme)))
  GROUP BY w.id, w.spelling_id, w.transcription, sp.value, tr.phonemic, tr.paiboon, tr."IPA"
  WITH NO DATA;

CREATE SEQUENCE thai."Word_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Word_id_seq" OWNED BY thai."Word".id;

CREATE TABLE thai."Zodiac" (
    id integer NOT NULL,
    ordinal integer NOT NULL,
    english_name character varying(32) NOT NULL,
    sanskrit_name character varying(32) NOT NULL,
    sanskrit_lexeme character varying(64) NOT NULL,
    element character varying(16) NOT NULL,
    image_url text
);

CREATE SEQUENCE thai."Zodiac_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE thai."Zodiac_id_seq" OWNED BY thai."Zodiac".id;

ALTER TABLE ONLY thai."CelestialBody" ALTER COLUMN id SET DEFAULT nextval('thai."CelestialBody_id_seq"'::regclass);

ALTER TABLE ONLY thai."Class" ALTER COLUMN id SET DEFAULT nextval('thai."Class_id_seq"'::regclass);

ALTER TABLE ONLY thai."Cluster" ALTER COLUMN id SET DEFAULT nextval('thai."Cluster_id_seq"'::regclass);

ALTER TABLE ONLY thai."ClusterWord" ALTER COLUMN id SET DEFAULT nextval('thai."ClusterWord_id_seq"'::regclass);

ALTER TABLE ONLY thai."CompositeName" ALTER COLUMN id SET DEFAULT nextval('thai."CompositeName_id_seq"'::regclass);

ALTER TABLE ONLY thai."Consonant" ALTER COLUMN id SET DEFAULT nextval('thai."Consonant_id_seq"'::regclass);

ALTER TABLE ONLY thai."ConsonantSample" ALTER COLUMN id SET DEFAULT nextval('thai."ConsonantSample_id_seq"'::regclass);

ALTER TABLE ONLY thai."DayOfWeek" ALTER COLUMN id SET DEFAULT nextval('thai."DayOfWeek_id_seq"'::regclass);

ALTER TABLE ONLY thai."Ending" ALTER COLUMN id SET DEFAULT nextval('thai."Ending_id_seq"'::regclass);

ALTER TABLE ONLY thai."Formality" ALTER COLUMN id SET DEFAULT nextval('thai."Formality_id_seq"'::regclass);

ALTER TABLE ONLY thai."Gender" ALTER COLUMN id SET DEFAULT nextval('thai."Gender_id_seq"'::regclass);

ALTER TABLE ONLY thai."Hour" ALTER COLUMN id SET DEFAULT nextval('thai."Hour_id_seq"'::regclass);

ALTER TABLE ONLY thai."Lexeme" ALTER COLUMN id SET DEFAULT nextval('thai."Lexeme_id_seq"'::regclass);

ALTER TABLE ONLY thai."Month" ALTER COLUMN id SET DEFAULT nextval('thai."Month_id_seq"'::regclass);

ALTER TABLE ONLY thai."Numeral" ALTER COLUMN id SET DEFAULT nextval('thai."Numeral_id_seq"'::regclass);

ALTER TABLE ONLY thai."Plurality" ALTER COLUMN id SET DEFAULT nextval('thai."Plurality_id_seq"'::regclass);

ALTER TABLE ONLY thai."Position" ALTER COLUMN id SET DEFAULT nextval('thai."Position_id_seq"'::regclass);

ALTER TABLE ONLY thai."PositionWord" ALTER COLUMN id SET DEFAULT nextval('thai."PositionWord_id_seq"'::regclass);

ALTER TABLE ONLY thai."Pronoun" ALTER COLUMN id SET DEFAULT nextval('thai."Pronoun_id_seq"'::regclass);

ALTER TABLE ONLY thai."PronounType" ALTER COLUMN id SET DEFAULT nextval('thai."PronounType_id_seq"'::regclass);

ALTER TABLE ONLY thai."PronounTypeBreakout" ALTER COLUMN id SET DEFAULT nextval('thai."PronounTypeBreakout_id_seq"'::regclass);

ALTER TABLE ONLY thai."Sound" ALTER COLUMN id SET DEFAULT nextval('thai."Sound_id_seq"'::regclass);

ALTER TABLE ONLY thai."SoundType" ALTER COLUMN id SET DEFAULT nextval('thai."SoundType_id_seq"'::regclass);

ALTER TABLE ONLY thai."SpecialSymbol" ALTER COLUMN id SET DEFAULT nextval('thai."SpecialSymbol_id_seq"'::regclass);

ALTER TABLE ONLY thai."Spelling" ALTER COLUMN id SET DEFAULT nextval('thai."Spelling_id_seq"'::regclass);

ALTER TABLE ONLY thai."TimeOfDay" ALTER COLUMN id SET DEFAULT nextval('thai."TimeOfDay_id_seq"'::regclass);

ALTER TABLE ONLY thai."ToneMark" ALTER COLUMN id SET DEFAULT nextval('thai."ToneMark_id_seq"'::regclass);

ALTER TABLE ONLY thai."Transcription" ALTER COLUMN id SET DEFAULT nextval('thai."Transcription_id_seq"'::regclass);

ALTER TABLE ONLY thai."Vowel" ALTER COLUMN id SET DEFAULT nextval('thai."Vowel_id_seq"'::regclass);

ALTER TABLE ONLY thai."VowelGrapheme" ALTER COLUMN id SET DEFAULT nextval('thai."VowelGrapheme_id_seq"'::regclass);

ALTER TABLE ONLY thai."VowelSample" ALTER COLUMN id SET DEFAULT nextval('thai."VowelSample_id_seq"'::regclass);

ALTER TABLE ONLY thai."Word" ALTER COLUMN id SET DEFAULT nextval('thai."Word_id_seq"'::regclass);

ALTER TABLE ONLY thai."WordLexeme" ALTER COLUMN id SET DEFAULT nextval('thai."WordLexeme_id_seq"'::regclass);

ALTER TABLE ONLY thai."Zodiac" ALTER COLUMN id SET DEFAULT nextval('thai."Zodiac_id_seq"'::regclass);

ALTER TABLE ONLY thai."CelestialBody"
    ADD CONSTRAINT "CelestialBody_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Class"
    ADD CONSTRAINT "Class_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."ClusterWord"
    ADD CONSTRAINT "ClusterWord_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Cluster"
    ADD CONSTRAINT "Cluster_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."CompositeName"
    ADD CONSTRAINT "CompositeName_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."ConsonantSample"
    ADD CONSTRAINT "ConsonantSample_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Consonant"
    ADD CONSTRAINT "Consonant_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."DayOfWeek"
    ADD CONSTRAINT "DayOfWeek_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Ending"
    ADD CONSTRAINT "Ending_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Formality"
    ADD CONSTRAINT "Formality_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Gender"
    ADD CONSTRAINT "Gender_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Hour"
    ADD CONSTRAINT "Hour_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Lexeme"
    ADD CONSTRAINT "Lexeme_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."MinimalSetWord"
    ADD CONSTRAINT "MinimalSetWord_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."MinimalSet"
    ADD CONSTRAINT "MinimalSet_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Month"
    ADD CONSTRAINT "Month_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Numeral"
    ADD CONSTRAINT "Numeral_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Plurality"
    ADD CONSTRAINT "Plurality_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."PositionWord"
    ADD CONSTRAINT "PositionWord_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Position"
    ADD CONSTRAINT "Position_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."PronounTypeBreakout"
    ADD CONSTRAINT "PronounTypeBreakout_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."PronounType"
    ADD CONSTRAINT "PronounType_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Pronoun"
    ADD CONSTRAINT "Pronoun_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."SoundType"
    ADD CONSTRAINT "SoundType_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Sound"
    ADD CONSTRAINT "Sound_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."SpecialSymbol"
    ADD CONSTRAINT "SpecialSymbol_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Spelling"
    ADD CONSTRAINT "Spelling_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."TimeOfDay"
    ADD CONSTRAINT "TimeOfDay_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."ToneMark"
    ADD CONSTRAINT "ToneMark_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Transcription"
    ADD CONSTRAINT "Transcription_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."VowelGrapheme"
    ADD CONSTRAINT "VowelGrapheme_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."VowelSample"
    ADD CONSTRAINT "VowelSample_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Vowel"
    ADD CONSTRAINT "Vowel_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."WordLexeme"
    ADD CONSTRAINT "WordLexeme_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Word"
    ADD CONSTRAINT "Word_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."Zodiac"
    ADD CONSTRAINT "Zodiac_pkey" PRIMARY KEY (id);

ALTER TABLE ONLY thai."CelestialBody"
    ADD CONSTRAINT uq_thai_celestial_body_ordinal UNIQUE (ordinal);

ALTER TABLE ONLY thai."Cluster"
    ADD CONSTRAINT uq_thai_cluster_type_value UNIQUE (type, value);

ALTER TABLE ONLY thai."ClusterWord"
    ADD CONSTRAINT uq_thai_cluster_word_pair UNIQUE (cluster_id, word_id);

ALTER TABLE ONLY thai."Hour"
    ADD CONSTRAINT uq_thai_hour_ordinal UNIQUE (ordinal);

ALTER TABLE ONLY thai."Lexeme"
    ADD CONSTRAINT uq_thai_lexeme_value_part_of_speech UNIQUE NULLS NOT DISTINCT (value, part_of_speech);

ALTER TABLE ONLY thai."Month"
    ADD CONSTRAINT uq_thai_month_ordinal UNIQUE (ordinal);

ALTER TABLE ONLY thai."Position"
    ADD CONSTRAINT uq_thai_position_row_col UNIQUE (row_index, col_index);

ALTER TABLE ONLY thai."PositionWord"
    ADD CONSTRAINT uq_thai_position_word_order UNIQUE (position_id, word_order);

ALTER TABLE ONLY thai."Spelling"
    ADD CONSTRAINT uq_thai_spelling_value UNIQUE (value);

ALTER TABLE ONLY thai."TimeOfDay"
    ADD CONSTRAINT uq_thai_time_of_day_ordinal UNIQUE (ordinal);

ALTER TABLE ONLY thai."Word"
    ADD CONSTRAINT uq_thai_word_spelling_transcription UNIQUE (spelling_id, transcription);

ALTER TABLE ONLY thai."Zodiac"
    ADD CONSTRAINT uq_thai_zodiac_ordinal UNIQUE (ordinal);

CREATE UNIQUE INDEX "WordView_id_idx" ON thai."WordView" USING btree (id);

CREATE INDEX "WordView_spelling_id_idx" ON thai."WordView" USING btree (spelling_id);

CREATE INDEX "WordView_transcription_id_idx" ON thai."WordView" USING btree (transcription_id);

ALTER TABLE ONLY thai."ClusterWord"
    ADD CONSTRAINT "ClusterWord_cluster_id_fkey" FOREIGN KEY (cluster_id) REFERENCES thai."Cluster"(id) ON DELETE CASCADE;

ALTER TABLE ONLY thai."ClusterWord"
    ADD CONSTRAINT "ClusterWord_word_id_fkey" FOREIGN KEY (word_id) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."CompositeName"
    ADD CONSTRAINT "CompositeName_word1_fkey" FOREIGN KEY (word1) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."CompositeName"
    ADD CONSTRAINT "CompositeName_word2_fkey" FOREIGN KEY (word2) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."CompositeName"
    ADD CONSTRAINT "CompositeName_word3_fkey" FOREIGN KEY (word3) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."CompositeName"
    ADD CONSTRAINT "CompositeName_word4_fkey" FOREIGN KEY (word4) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."CompositeName"
    ADD CONSTRAINT "CompositeName_word5_fkey" FOREIGN KEY (word5) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."ConsonantSample"
    ADD CONSTRAINT "ConsonantSample_consonant_fkey" FOREIGN KEY (consonant) REFERENCES thai."Consonant"(id);

ALTER TABLE ONLY thai."ConsonantSample"
    ADD CONSTRAINT "ConsonantSample_word_fkey" FOREIGN KEY (word) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."Consonant"
    ADD CONSTRAINT "Consonant_acrophonic_fkey" FOREIGN KEY (acrophonic) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."Consonant"
    ADD CONSTRAINT "Consonant_final_fkey" FOREIGN KEY (final) REFERENCES thai."Sound"(id);

ALTER TABLE ONLY thai."Consonant"
    ADD CONSTRAINT "Consonant_initial_fkey" FOREIGN KEY (initial) REFERENCES thai."Sound"(id);

ALTER TABLE ONLY thai."Consonant"
    ADD CONSTRAINT "Consonant_transcription_fkey" FOREIGN KEY (transcription) REFERENCES thai."Transcription"(id);

ALTER TABLE ONLY thai."DayOfWeek"
    ADD CONSTRAINT "DayOfWeek_word_id_fkey" FOREIGN KEY (word_id) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."Hour"
    ADD CONSTRAINT "Hour_word_id_fkey" FOREIGN KEY (word_id) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."MinimalSetWord"
    ADD CONSTRAINT "MinimalSetWord_set_id_fkey" FOREIGN KEY (set_id) REFERENCES thai."MinimalSet"(id);

ALTER TABLE ONLY thai."MinimalSetWord"
    ADD CONSTRAINT "MinimalSetWord_word_id_fkey" FOREIGN KEY (word_id) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."Month"
    ADD CONSTRAINT "Month_word_id_fkey" FOREIGN KEY (word_id) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."Month"
    ADD CONSTRAINT "Month_zodiac_id_fkey" FOREIGN KEY (zodiac_id) REFERENCES thai."Zodiac"(id);

ALTER TABLE ONLY thai."Numeral"
    ADD CONSTRAINT "Numeral_word_fkey" FOREIGN KEY (word) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."PositionWord"
    ADD CONSTRAINT "PositionWord_position_id_fkey" FOREIGN KEY (position_id) REFERENCES thai."Position"(id) ON DELETE CASCADE;

ALTER TABLE ONLY thai."PositionWord"
    ADD CONSTRAINT "PositionWord_word_id_fkey" FOREIGN KEY (word_id) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."PronounTypeBreakout"
    ADD CONSTRAINT "PronounTypeBreakout_pronoun_id_fkey" FOREIGN KEY (pronoun_id) REFERENCES thai."Pronoun"(id);

ALTER TABLE ONLY thai."PronounTypeBreakout"
    ADD CONSTRAINT "PronounTypeBreakout_type_id_fkey" FOREIGN KEY (type_id) REFERENCES thai."PronounType"(id);

ALTER TABLE ONLY thai."Pronoun"
    ADD CONSTRAINT "Pronoun_formality_fkey" FOREIGN KEY (formality) REFERENCES thai."Formality"(id);

ALTER TABLE ONLY thai."Pronoun"
    ADD CONSTRAINT "Pronoun_gender_fkey" FOREIGN KEY (gender) REFERENCES thai."Gender"(id);

ALTER TABLE ONLY thai."Pronoun"
    ADD CONSTRAINT "Pronoun_plurality_fkey" FOREIGN KEY (plurality) REFERENCES thai."Plurality"(id);

ALTER TABLE ONLY thai."Pronoun"
    ADD CONSTRAINT "Pronoun_word_fkey" FOREIGN KEY (word) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."Sound"
    ADD CONSTRAINT "Sound_sound_type_fkey" FOREIGN KEY (sound_type) REFERENCES thai."SoundType"(id);

ALTER TABLE ONLY thai."SpecialSymbol"
    ADD CONSTRAINT "SpecialSymbol_word_id_fkey" FOREIGN KEY (word_id) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."TimeOfDay"
    ADD CONSTRAINT "TimeOfDay_word_id_fkey" FOREIGN KEY (word_id) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."ToneMark"
    ADD CONSTRAINT "ToneMark_name_fkey" FOREIGN KEY (name) REFERENCES thai."CompositeName"(id);

ALTER TABLE ONLY thai."VowelGrapheme"
    ADD CONSTRAINT "VowelGrapheme_vowel_id_fkey" FOREIGN KEY (vowel_id) REFERENCES thai."Vowel"(id);

ALTER TABLE ONLY thai."VowelSample"
    ADD CONSTRAINT "VowelSample_vowel_fkey" FOREIGN KEY (vowel) REFERENCES thai."Vowel"(id);

ALTER TABLE ONLY thai."VowelSample"
    ADD CONSTRAINT "VowelSample_word_fkey" FOREIGN KEY (word) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."Vowel"
    ADD CONSTRAINT "Vowel_sound_id_fkey" FOREIGN KEY (sound_id) REFERENCES thai."Sound"(id);

ALTER TABLE ONLY thai."WordLexeme"
    ADD CONSTRAINT "WordLexeme_lexeme_fkey" FOREIGN KEY (lexeme) REFERENCES thai."Lexeme"(id);

ALTER TABLE ONLY thai."WordLexeme"
    ADD CONSTRAINT "WordLexeme_word_fkey" FOREIGN KEY (word) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."Word"
    ADD CONSTRAINT "Word_spelling_id_fkey" FOREIGN KEY (spelling_id) REFERENCES thai."Spelling"(id);

ALTER TABLE ONLY thai."Word"
    ADD CONSTRAINT "Word_transcription_fkey" FOREIGN KEY (transcription) REFERENCES thai."Transcription"(id);

ALTER TABLE ONLY thai."CelestialBody"
    ADD CONSTRAINT fk_celestial_body_word FOREIGN KEY (word_id) REFERENCES thai."Word"(id);

ALTER TABLE ONLY thai."Consonant"
    ADD CONSTRAINT fk_consonant_class FOREIGN KEY (class) REFERENCES thai."Class"(id);

ALTER TABLE ONLY thai."Consonant"
    ADD CONSTRAINT fk_consonant_ending FOREIGN KEY (ending) REFERENCES thai."Ending"(id);

CREATE TABLE thai.user_accounts (
    id       bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    username character varying(255) NOT NULL,
    password character varying(255) NOT NULL,
    role     character varying(32) NOT NULL,
    CONSTRAINT uq_user_accounts_username UNIQUE (username)
);

