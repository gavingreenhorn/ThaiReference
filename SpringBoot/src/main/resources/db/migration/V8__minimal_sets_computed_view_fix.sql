DROP MATERIALIZED VIEW IF EXISTS thai."MinimalSetComputedView";

CREATE MATERIALIZED VIEW thai."MinimalSetComputedView" AS
WITH words AS (
	SELECT
		w.id AS word_id,
		s.value AS spelling,
		t."IPA" AS ipa,
		t.paiboon AS paiboon,
		COALESCE(init.label, 'ʔ') AS label,
		regexp_replace(
			lower(COALESCE(t."IPA", '')),
			'[ːˑ\u02D0\u02D1\u0304\u0301\u0300\u0302\u030C\u1DC4\u1DC5\u1DC6\u1DC7\u0306\u0308]',
			'',
			'g') AS ipa_core,
		regexp_replace(COALESCE(t."IPA", ''), '[^ːˑ\u02D0\u02D1]', '', 'g') AS length_marker,
		regexp_replace(COALESCE(t."IPA", ''), '[^\u0304\u0301\u0300\u0302\u030C\u1DC4\u1DC5\u1DC6\u1DC7]', '', 'g') AS tone_marker
	FROM thai."Word" w
	JOIN thai."Spelling" s ON s.id = w.spelling_id
	JOIN thai."Transcription" t ON t.id = w.transcription
	LEFT JOIN LATERAL (
		SELECT snd.value AS label
		FROM thai."Consonant" c
		JOIN thai."Sound" snd ON snd.id = c.initial
		WHERE t."IPA" LIKE c.phoneme || '%'
		  AND snd.value IS NOT NULL
		  AND snd.value <> ''
		ORDER BY length(c.phoneme) DESC, c.id
		LIMIT 1
	) init ON true
	WHERE w.is_primary = true
	  AND t."IPA" IS NOT NULL
	  AND t."IPA" <> ''
),
sets AS (
	SELECT
		w.label,
		w.ipa_core,
		COUNT(*) AS word_count,
		COUNT(DISTINCT w.tone_marker) AS tone_count,
		COUNT(DISTINCT w.length_marker) AS length_count,
		row_number() OVER (ORDER BY w.label, w.ipa_core) AS set_id
	FROM words w
	WHERE w.label IS NOT NULL
	  AND w.label <> ''
	GROUP BY w.label, w.ipa_core
	HAVING COUNT(*) >= 2
	   AND (
			(COUNT(DISTINCT w.tone_marker) > 1 AND COUNT(DISTINCT w.length_marker) = 1)
		 OR (COUNT(DISTINCT w.length_marker) > 1 AND COUNT(DISTINCT w.tone_marker) = 1)
	   )
)
SELECT
	st.set_id,
	st.label,
	w.word_id,
	row_number() OVER (PARTITION BY st.set_id ORDER BY w.spelling, w.word_id) AS set_word_order
FROM sets st
JOIN words w ON w.label = st.label AND w.ipa_core = st.ipa_core
ORDER BY st.set_id, set_word_order
WITH NO DATA;

CREATE UNIQUE INDEX "MinimalSetComputedView_set_word_uq"
	ON thai."MinimalSetComputedView" (set_id, word_id);

CREATE INDEX "MinimalSetComputedView_label_set_idx"
	ON thai."MinimalSetComputedView" (label, set_id);

REFRESH MATERIALIZED VIEW thai."MinimalSetComputedView";
