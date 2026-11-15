-- Run after V11 and mood_sample_data.sql.
-- Pronunciations follow the corresponding Wiktionary Thai entries.
-- Published example sources (CC BY 2.0 FR):
-- https://tatoeba.org/en/sentences/show/12858401 by superteenkai
-- https://tatoeba.org/en/sentences/show/10174096 by Somsak
-- https://tatoeba.org/en/sentences/show/10624653 by Somsak
BEGIN;

LOCK TABLE thai."Word", thai."Spelling", thai."Transcription",
    thai."Lexeme", thai."WordLexeme", thai."Sentence", thai."SentenceWord",
    thai."Mood", thai."MoodSentence" IN SHARE ROW EXCLUSIVE MODE;

CREATE TEMP TABLE mood_extra_word (
    spelling text PRIMARY KEY, paiboon text NOT NULL, ipa text NOT NULL,
    gloss text NOT NULL, part_of_speech text NOT NULL
) ON COMMIT DROP;

INSERT INTO mood_extra_word VALUES
    ('ดีใจ', 'dii-jai', 'dīː.tɕāj', 'glad; happy', 'adjective'),
    ('เสียใจ', 'sǐia-jai', 'sǐa.tɕāj', 'sorry; to regret', 'verb'),
    ('ได้', 'dâai', 'dâːj', 'to get; to be able to', 'verb'),
    ('เจอ', 'jəə', 'tɕɤ̄ː', 'to meet; to find', 'verb'),
    ('บ้าน', 'bâan', 'bâːn', 'home; house', 'noun'),
    ('ด้วย', 'dûai', 'dûaj', 'with; also', 'adverb'),
    ('ไหม', 'mǎi', 'mǎj', 'question particle', 'particle'),
    ('อย่างนั้น', 'yàang-nán', 'jàːŋ.nán', 'like that', 'adverb');

CREATE TEMP TABLE mood_extra_sentence (
    seed_id integer PRIMARY KEY, mood text NOT NULL, position integer NOT NULL,
    thai_text text NOT NULL, translation text NOT NULL, tokens text[] NOT NULL
) ON COMMIT DROP;

INSERT INTO mood_extra_sentence VALUES
    (1, 'ดีใจ', 1, 'ดีใจที่ได้เจอ', 'Nice to see you.',
     ARRAY['ดีใจ','ที่','ได้','เจอ']),
    (2, 'ดีใจ', 2, 'ผมดีใจที่คุณกลับบ้าน', 'I am glad you are home.',
     ARRAY['ผม','ดีใจ','ที่','คุณ','กลับ','บ้าน']),
    (3, 'เสียใจ', 1, 'เสียใจด้วยนะ', 'I am sorry for your loss.',
     ARRAY['เสียใจ','ด้วย','นะ']),
    (4, 'เสียใจ', 2, 'คุณเสียใจไหมที่ทำอย่างนั้น', 'Do you regret doing that?',
     ARRAY['คุณ','เสียใจ','ไหม','ที่','ทำ','อย่างนั้น']);

CREATE TEMP TABLE mood_art (spelling text PRIMARY KEY, svg text NOT NULL) ON COMMIT DROP;
INSERT INTO mood_art VALUES
('สุข', $svg$<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 160">
  <circle cx="80" cy="80" r="63" fill="#e9f8df" stroke="#528a43" stroke-width="4"/>
  <path d="M47 64q10 8 20 0M93 64q10 8 20 0" fill="none" stroke="#305d30" stroke-width="6" stroke-linecap="round"/>
  <path d="M54 91q26 25 52 0" fill="none" stroke="#305d30" stroke-width="6" stroke-linecap="round"/>
  <circle cx="42" cy="87" r="8" fill="#f3b3ad"/><circle cx="118" cy="87" r="8" fill="#f3b3ad"/>
</svg>$svg$),
('ดีใจ', $svg$<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 160">
  <circle cx="80" cy="80" r="61" fill="#fff3cd" stroke="#c58a16" stroke-width="4"/>
  <path d="M45 66q10-16 20 0M95 66q10-16 20 0" fill="none" stroke="#68460d" stroke-width="6" stroke-linecap="round"/>
  <path d="M54 91h52q-5 24-26 24T54 91Z" fill="#d65e43" stroke="#68460d" stroke-width="5" stroke-linejoin="round"/>
  <circle cx="42" cy="86" r="8" fill="#f3adad"/><circle cx="118" cy="86" r="8" fill="#f3adad"/>
  <path d="M20 30v14m-7-7h14M139 24v16m-8-8h16" stroke="#e9a629" stroke-width="5" stroke-linecap="round"/>
</svg>$svg$),
('เศร้า', $svg$<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 160">
  <circle cx="80" cy="80" r="63" fill="#e8f2fd" stroke="#427cb4" stroke-width="4"/>
  <path d="M48 54l17-5M95 49l17 5" fill="none" stroke="#244e79" stroke-width="6" stroke-linecap="round"/>
  <path d="M48 69q9 7 18 0M94 69q9 7 18 0" fill="none" stroke="#244e79" stroke-width="6" stroke-linecap="round"/>
  <path d="M58 105q22-18 44 0" fill="none" stroke="#244e79" stroke-width="6" stroke-linecap="round"/>
  <ellipse cx="46" cy="87" rx="8" ry="5" fill="#c8dff2"/><ellipse cx="114" cy="87" rx="8" ry="5" fill="#c8dff2"/>
</svg>$svg$),
('เสียใจ', $svg$<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 160">
  <circle cx="80" cy="80" r="63" fill="#e5e9fb" stroke="#5b67aa" stroke-width="4"/>
  <path d="M48 54l18-6M94 48l18 6" fill="none" stroke="#343e78" stroke-width="6" stroke-linecap="round"/>
  <circle cx="56" cy="69" r="6" fill="#343e78"/><circle cx="104" cy="69" r="6" fill="#343e78"/>
  <path d="M57 104q23-19 46 0" fill="none" stroke="#343e78" stroke-width="6" stroke-linecap="round"/>
  <path d="M48 79q-9 13-9 19a9 9 0 0 0 18 0q0-6-9-19Z" fill="#6ba6e3"/>
  <path d="M112 79q-7 11-7 16a7 7 0 0 0 14 0q0-5-7-16Z" fill="#6ba6e3"/>
</svg>$svg$),
('โกรธ', $svg$<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 160">
  <circle cx="80" cy="80" r="63" fill="#fff0ec" stroke="#bd4141" stroke-width="4"/>
  <path d="M42 49l25 12M118 49L93 61" fill="none" stroke="#742d2d" stroke-width="7" stroke-linecap="round"/>
  <circle cx="56" cy="75" r="7" fill="#742d2d"/><circle cx="104" cy="75" r="7" fill="#742d2d"/>
  <path d="M54 110q26-18 52 0" fill="none" stroke="#742d2d" stroke-width="7" stroke-linecap="round"/>
  <circle cx="41" cy="93" r="8" fill="#ed9992"/><circle cx="119" cy="93" r="8" fill="#ed9992"/>
  <path d="M129 24q9-8 17 0M135 37q8-7 15 0" fill="none" stroke="#e15b52" stroke-width="4" stroke-linecap="round"/>
</svg>$svg$),
('กลัว', $svg$<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 160">
  <circle cx="80" cy="80" r="63" fill="#f2edfc" stroke="#8363b8" stroke-width="4"/>
  <path d="M46 51q9-10 20-6M94 45q11-4 20 6" fill="none" stroke="#4d3974" stroke-width="5" stroke-linecap="round"/>
  <ellipse cx="56" cy="71" rx="11" ry="14" fill="#fff" stroke="#4d3974" stroke-width="4"/>
  <ellipse cx="104" cy="71" rx="11" ry="14" fill="#fff" stroke="#4d3974" stroke-width="4"/>
  <circle cx="56" cy="73" r="4" fill="#4d3974"/><circle cx="104" cy="73" r="4" fill="#4d3974"/>
  <ellipse cx="80" cy="108" rx="9" ry="12" fill="#4d3974"/>
  <path d="M132 28q-8 12-8 17a8 8 0 0 0 16 0q0-5-8-17Z" fill="#8bbbe8"/>
</svg>$svg$),
('เหนื่อย', $svg$<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 160">
  <circle cx="80" cy="80" r="63" fill="#edf1f3" stroke="#788b96" stroke-width="4"/>
  <path d="M44 67q11 6 22 0M94 67q11 6 22 0" fill="none" stroke="#405663" stroke-width="6" stroke-linecap="round"/>
  <path d="M67 104q13-8 26 0" fill="none" stroke="#405663" stroke-width="6" stroke-linecap="round"/>
  <path d="M117 102q17-7 24 2q-4 8-14 8" fill="none" stroke="#9eb2bc" stroke-width="4" stroke-linecap="round"/>
  <path d="M27 48l10 3M123 51l10-3" stroke="#9eb2bc" stroke-width="4" stroke-linecap="round"/>
</svg>$svg$),
('ตื่นเต้น', $svg$<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 160">
  <circle cx="80" cy="80" r="61" fill="#fff0f8" stroke="#cc6c9e" stroke-width="4"/>
  <ellipse cx="56" cy="68" rx="10" ry="13" fill="#723f66"/><ellipse cx="104" cy="68" rx="10" ry="13" fill="#723f66"/>
  <circle cx="53" cy="64" r="3" fill="#fff"/><circle cx="101" cy="64" r="3" fill="#fff"/>
  <path d="M62 93q18 5 36 0q-2 23-18 23T62 93Z" fill="#e9788e" stroke="#723f66" stroke-width="5" stroke-linejoin="round"/>
  <circle cx="40" cy="89" r="8" fill="#f3b2c4"/><circle cx="120" cy="89" r="8" fill="#f3b2c4"/>
  <path d="M20 25v14m-7-7h14M140 25v14m-7-7h14" stroke="#eb93bb" stroke-width="5" stroke-linecap="round"/>
</svg>$svg$);

DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM mood_extra_word v
        JOIN thai."Spelling" s ON s.value = v.spelling
        JOIN thai."Word" w ON w.spelling_id = s.id
        LEFT JOIN thai."Transcription" t ON t.id = w.transcription
        WHERE normalize(t.paiboon, NFC) IS DISTINCT FROM normalize(v.paiboon, NFC)
           OR normalize(t."IPA", NFC) IS DISTINCT FROM normalize(v.ipa, NFC)
    ) OR EXISTS (
        SELECT 1 FROM mood_extra_word v JOIN thai."Transcription" t
          ON normalize(t.paiboon, NFC) = normalize(v.paiboon, NFC)
          OR normalize(t."IPA", NFC) = normalize(v.ipa, NFC)
        WHERE normalize(t.paiboon, NFC) IS DISTINCT FROM normalize(v.paiboon, NFC)
           OR normalize(t."IPA", NFC) IS DISTINCT FROM normalize(v.ipa, NFC)
    ) THEN
        RAISE EXCEPTION 'A word or transcription conflicts with the sample data';
    END IF;
    IF EXISTS (
        SELECT 1 FROM thai."Mood" m JOIN thai."Word" w ON w.id = m.word_id
        JOIN thai."Spelling" s ON s.id = w.spelling_id
        WHERE s.value IN ('ดีใจ', 'เสียใจ') OR (s.value IN (SELECT spelling FROM mood_art) AND m.svg IS NOT NULL)
    ) THEN
        RAISE EXCEPTION 'A new mood or existing illustration is already present';
    END IF;
END $$;

INSERT INTO thai."Spelling"(value)
SELECT spelling FROM mood_extra_word ON CONFLICT (value) DO NOTHING;

INSERT INTO thai."Transcription"(paiboon, "IPA")
SELECT normalize(v.paiboon, NFC), normalize(v.ipa, NFC)
FROM mood_extra_word v
WHERE NOT EXISTS (
    SELECT 1 FROM thai."Transcription" t
    WHERE normalize(t.paiboon, NFC) = normalize(v.paiboon, NFC)
      AND normalize(t."IPA", NFC) = normalize(v.ipa, NFC)
);

INSERT INTO thai."Word"(spelling_id, transcription, is_primary)
SELECT s.id, t.id, true FROM mood_extra_word v
JOIN thai."Spelling" s ON s.value = v.spelling
JOIN thai."Transcription" t ON normalize(t.paiboon, NFC) = normalize(v.paiboon, NFC)
                              AND normalize(t."IPA", NFC) = normalize(v.ipa, NFC)
WHERE NOT EXISTS (SELECT 1 FROM thai."Word" w WHERE w.spelling_id = s.id);

INSERT INTO thai."Lexeme"(value, part_of_speech)
SELECT gloss, part_of_speech FROM mood_extra_word ON CONFLICT (value, part_of_speech) DO NOTHING;

INSERT INTO thai."WordLexeme"(word, lexeme)
SELECT w.id, l.id FROM mood_extra_word v
JOIN thai."Spelling" s ON s.value = v.spelling
JOIN thai."Word" w ON w.spelling_id = s.id
JOIN thai."Lexeme" l ON l.value = v.gloss AND l.part_of_speech = v.part_of_speech
WHERE NOT EXISTS (SELECT 1 FROM thai."WordLexeme" wl WHERE wl.word = w.id AND wl.lexeme = l.id);

INSERT INTO thai."Mood"(word_id)
SELECT w.id FROM thai."Word" w JOIN thai."Spelling" s ON s.id = w.spelling_id
WHERE s.value IN ('ดีใจ', 'เสียใจ');

CREATE TEMP TABLE mood_extra_sentence_id ON COMMIT DROP AS
SELECT seed_id, (SELECT COALESCE(max(id), 0) FROM thai."Sentence")
       + row_number() OVER (ORDER BY seed_id)::integer AS sentence_id
FROM mood_extra_sentence;

INSERT INTO thai."Sentence"(id, translation)
SELECT i.sentence_id, s.translation FROM mood_extra_sentence s JOIN mood_extra_sentence_id i USING (seed_id);

INSERT INTO thai."SentenceWord"(sentence_id, word_id, word_order, following_text)
SELECT i.sentence_id, w.id, token.ordinality::integer, ''
FROM mood_extra_sentence v
JOIN mood_extra_sentence_id i USING (seed_id)
CROSS JOIN LATERAL unnest(v.tokens) WITH ORDINALITY AS token(spelling, ordinality)
JOIN thai."Spelling" s ON s.value = token.spelling
JOIN thai."Word" w ON w.spelling_id = s.id;

INSERT INTO thai."MoodSentence"(mood_id, sentence_id, position)
SELECT m.id, i.sentence_id, v.position FROM mood_extra_sentence v
JOIN mood_extra_sentence_id i USING (seed_id)
JOIN thai."Spelling" s ON s.value = v.mood
JOIN thai."Word" w ON w.spelling_id = s.id
JOIN thai."Mood" m ON m.word_id = w.id;

UPDATE thai."Mood" m SET svg = a.svg
FROM thai."Word" w JOIN thai."Spelling" s ON s.id = w.spelling_id
JOIN mood_art a ON a.spelling = s.value
WHERE m.word_id = w.id;

DO $$
BEGIN
    IF (SELECT count(*) FROM thai."Mood" m JOIN thai."Word" w ON w.id = m.word_id
        JOIN thai."Spelling" s ON s.id = w.spelling_id JOIN mood_art a ON a.spelling = s.value
        WHERE m.svg = a.svg) <> 8
       OR (SELECT count(*) FROM thai."MoodSentence" ms
           JOIN mood_extra_sentence_id i ON i.sentence_id = ms.sentence_id) <> 4
       OR EXISTS (
           SELECT 1 FROM mood_extra_sentence v JOIN mood_extra_sentence_id i USING (seed_id)
           LEFT JOIN thai."SentenceWord" sw ON sw.sentence_id = i.sentence_id
           LEFT JOIN thai."Word" w ON w.id = sw.word_id
           LEFT JOIN thai."Spelling" s ON s.id = w.spelling_id
           GROUP BY v.seed_id, v.thai_text
           HAVING string_agg(s.value || sw.following_text, '' ORDER BY sw.word_order) IS DISTINCT FROM v.thai_text
       ) THEN
        RAISE EXCEPTION 'Mood artwork or sentence dependencies are incomplete';
    END IF;
END $$;

REFRESH MATERIALIZED VIEW thai."WordView";
REFRESH MATERIALIZED VIEW thai."MinimalSetView";
COMMIT;
