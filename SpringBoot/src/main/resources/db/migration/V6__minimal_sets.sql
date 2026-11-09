DROP MATERIALIZED VIEW IF EXISTS thai."MinimalSetComputedView";
DROP MATERIALIZED VIEW IF EXISTS thai."MinimalSetView";

CREATE MATERIALIZED VIEW thai."MinimalSetView" AS
WITH normalized_words AS (
	SELECT
		w.id AS word_id,
		regexp_replace(
			normalize(t."IPA", NFD),
			U&'[\0304\031A\032F\0361[:space:].]',
			'',
			'g') AS comparison_ipa
	FROM thai."Word" w
	JOIN thai."Transcription" t ON t.id = w.transcription
	WHERE w.is_primary = true
	  AND t."IPA" IS NOT NULL
	  AND t."IPA" <> ''
),
words AS (
	SELECT
		w.word_id,
		w.comparison_ipa,
		regexp_replace(w.comparison_ipa,
			U&'[\02D0\02D1\0300\0301\0302\030C\1DC4\1DC5\1DC6\1DC7]',
			'', 'g') AS comparison_key,
		init.label
	FROM normalized_words w
	JOIN LATERAL (
		SELECT snd.value AS label
		FROM thai."Sound" snd
		WHERE snd.consonant_sound = 1
		  AND snd.value IS NOT NULL
		  AND snd.value <> ''
		  AND (
				starts_with(w.comparison_ipa, regexp_replace(
					normalize(snd.value, NFD), U&'[\0361]', '', 'g'))
				OR (snd.value = U&'\0294' AND w.comparison_ipa ~ U&'^[aeiou\025B\0264\0254\026F]')
			  )
		ORDER BY length(snd.value) DESC, snd.initial_only DESC, snd.id
		LIMIT 1
	) init ON true
	WHERE w.comparison_ipa <> ''
),
sets AS (
	SELECT
		label,
		comparison_key,
		row_number() OVER (ORDER BY label, comparison_key) AS set_id
	FROM words
	GROUP BY label, comparison_key
	HAVING COUNT(DISTINCT comparison_ipa) > 1
)
SELECT st.set_id, st.label, w.word_id
FROM sets st
JOIN words w
	ON w.label = st.label
	AND w.comparison_key = st.comparison_key
ORDER BY st.set_id, w.word_id
WITH NO DATA;

CREATE UNIQUE INDEX "MinimalSetView_set_word_uq"
	ON thai."MinimalSetView" (set_id, word_id);

CREATE INDEX "MinimalSetView_label_set_idx"
	ON thai."MinimalSetView" (label, set_id);

REFRESH MATERIALIZED VIEW thai."MinimalSetView";
