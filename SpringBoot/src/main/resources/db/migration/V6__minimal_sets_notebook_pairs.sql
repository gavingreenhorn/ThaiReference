DO $$
DECLARE
	r record;
	sid integer;
	tid integer;
	wid integer;
BEGIN
	FOR r IN
		SELECT * FROM (VALUES
			('หิน',  'hǐn',  'hǐn'),
			('เห็น', 'hěn',  'hěn'),
			('ใหม่', 'mài',  'màj'),
			('ไม่',  'mâi',  'mâj'),
			('นั่ง', 'nâng', 'nâŋ'),
			('หนัง', 'nǎng', 'nǎŋ'),
			('ขิง',  'kǐng', 'kʰǐŋ'),
			('วาน',  'waan', 'waːn'),
			('วัน',  'wan',  'wan'),
			('หลง', 'lǒng', 'lǒŋ')
		) AS t(token, paiboon, ipa)
	LOOP
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
		END IF;
	END LOOP;
END $$;

WITH pairs(code, label, w1, w2) AS (
	VALUES
		('hin_hen',      'h', 'หิน',  'เห็น'),
		('suk_sukh',     's', 'สุก',  'สุข'),
		('mai_mai_not',  'm', 'ใหม่', 'ไม่'),
		('nang_nang',    'n', 'นั่ง', 'หนัง'),
		('khing_ying',   'k', 'ขิง',  'หญิง'),
		('wan_waan',     'w', 'วัน',  'วาน'),
		('long_laang',   'l', 'หลง', 'ล้าง')
),
missing_pairs AS (
	SELECT p.*
	FROM pairs p
	WHERE NOT EXISTS (
		SELECT 1
		FROM thai."MinimalSet" ms
		JOIN thai."MinimalSetWord" msw ON msw.set_id = ms.id
		JOIN thai."Word" w ON w.id = msw.word_id
		JOIN thai."Spelling" s ON s.id = w.spelling_id
		WHERE s.value IN (p.w1, p.w2)
		GROUP BY ms.id
		HAVING COUNT(DISTINCT s.value) = 2
	)
),
ordered_missing AS (
	SELECT
		code,
		label,
		w1,
		w2,
		row_number() OVER (ORDER BY code) AS rn
	FROM missing_pairs
),
set_base AS (
	SELECT COALESCE(MAX(id), 0) AS max_id FROM thai."MinimalSet"
),
inserted_sets AS (
	INSERT INTO thai."MinimalSet"(id, label)
	SELECT sb.max_id + om.rn, om.label
	FROM ordered_missing om
	CROSS JOIN set_base sb
	RETURNING id, label
),
set_map AS (
	SELECT
		iset.id AS set_id,
		om.w1,
		om.w2,
		om.rn
	FROM inserted_sets iset
	JOIN ordered_missing om ON om.label = iset.label
),
word_ids AS (
	SELECT s.value AS token, MIN(w.id) AS word_id
	FROM thai."Word" w
	JOIN thai."Spelling" s ON s.id = w.spelling_id
	WHERE s.value IN (
		'หิน','เห็น','สุก','สุข','ใหม่','ไม่','นั่ง','หนัง','ขิง','หญิง','วัน','วาน','หลง','ล้าง'
	)
	GROUP BY s.value
),
rows_to_insert AS (
	SELECT sm.set_id, 1 AS ord, w1.word_id
	FROM set_map sm
	JOIN word_ids w1 ON w1.token = sm.w1
	UNION ALL
	SELECT sm.set_id, 2 AS ord, w2.word_id
	FROM set_map sm
	JOIN word_ids w2 ON w2.token = sm.w2
),
word_base AS (
	SELECT COALESCE(MAX(id), 0) AS max_id FROM thai."MinimalSetWord"
)
INSERT INTO thai."MinimalSetWord"(id, set_id, word_id)
SELECT wb.max_id + row_number() OVER (ORDER BY rti.set_id, rti.ord), rti.set_id, rti.word_id
FROM rows_to_insert rti
CROSS JOIN word_base wb;

REFRESH MATERIALIZED VIEW thai."WordView";
