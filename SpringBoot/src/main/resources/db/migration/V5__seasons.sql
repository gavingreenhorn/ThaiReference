CREATE TABLE thai."Season" (
	id integer NOT NULL,
	ordinal integer NOT NULL,
	name integer NOT NULL,
	english_label text NOT NULL,
	CONSTRAINT "Season_pkey" PRIMARY KEY (id),
	CONSTRAINT "Season_name_fkey" FOREIGN KEY (name) REFERENCES thai."CompositeName"(id)
);

CREATE UNIQUE INDEX uq_thai_season_ordinal ON thai."Season"(ordinal);
CREATE UNIQUE INDEX uq_thai_season_name ON thai."Season"(name);

ALTER TABLE thai."Month"
	ADD COLUMN season_id integer;

CREATE TABLE thai._season_seed_meta (
	kind text NOT NULL,
	token text,
	word_id integer,
	spelling_id integer,
	transcription_id integer,
	lexeme_id integer,
	composite_name_id integer,
	season_id integer
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
			('ฤดู',  'rue-duu', 'rʉ́duu', 'season', 'noun'),
			('หนาว', 'nǎao',    'naːw',   'cold',   'adjective'),
			('ร้อน', 'rɔ́ɔn',   'rɔ́ːn',  'hot',    'adjective'),
			('ฝน',   'fǒn',     'fon',    'rain',   'noun')
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

			INSERT INTO thai._season_seed_meta(kind, token, word_id, spelling_id, transcription_id)
			VALUES ('word', r.token, wid, sid, tid);
		ELSE
			INSERT INTO thai._season_seed_meta(kind, token, word_id, spelling_id, transcription_id)
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

			INSERT INTO thai._season_seed_meta(kind, token, lexeme_id)
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
	WHERE s.value IN ('ฤดู', 'หนาว', 'ร้อน', 'ฝน')
	GROUP BY s.value
),
seasons(id, ordinal, english_label, t1, t2) AS (
	VALUES
		(1, 1, 'Winter', 'ฤดู', 'หนาว'),
		(2, 2, 'Summer', 'ฤดู', 'ร้อน'),
		(3, 3, 'Rainy',  'ฤดู', 'ฝน')
),
prepared AS (
	SELECT
		s.id,
		s.ordinal,
		s.english_label,
		w1.word_id AS word1,
		w2.word_id AS word2
	FROM seasons s
	JOIN w w1 ON w1.token = s.t1
	JOIN w w2 ON w2.token = s.t2
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
INSERT INTO thai."Season"(id, ordinal, name, english_label)
SELECT p.id, p.ordinal, c.composite_name_id, p.english_label
FROM prepared p
JOIN cn_ordered c ON c.rn = p.id;

INSERT INTO thai._season_seed_meta(kind, composite_name_id, season_id)
SELECT 'season', name, id FROM thai."Season";

UPDATE thai."Month" m
SET season_id = CASE
	WHEN m.ordinal IN (11, 12, 1, 2) THEN 1
	WHEN m.ordinal IN (3, 4, 5, 6)   THEN 2
	WHEN m.ordinal IN (7, 8, 9, 10)  THEN 3
END;

ALTER TABLE thai."Month"
	ALTER COLUMN season_id SET NOT NULL;

ALTER TABLE thai."Month"
	ADD CONSTRAINT "Month_season_id_fkey"
		FOREIGN KEY (season_id) REFERENCES thai."Season"(id);

CREATE INDEX idx_thai_month_season_id ON thai."Month"(season_id);

REFRESH MATERIALIZED VIEW thai."WordView";
