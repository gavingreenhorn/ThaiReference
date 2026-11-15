-- Run separately after V11. Tatoeba sentences are CC BY 2.0 FR; their
-- contributors and source URLs are retained in MoodSentence.
-- Pronunciations for new words follow Wiktionary's Thai entries, with IPA
-- converted to the tone-mark format used by this database.
BEGIN;

LOCK TABLE thai."Word", thai."Spelling", thai."Transcription",
    thai."Lexeme", thai."WordLexeme", thai."Sentence",
    thai."SentenceWord", thai."Mood", thai."MoodSentence"
    IN SHARE ROW EXCLUSIVE MODE;

CREATE TEMP TABLE mood_seed_word (
    spelling text PRIMARY KEY,
    paiboon text,
    ipa text,
    gloss text,
    part_of_speech text,
    CHECK ((paiboon IS NULL) = (ipa IS NULL))
) ON COMMIT DROP;

INSERT INTO mood_seed_word VALUES
    ('ฉัน', NULL, NULL, NULL, NULL),
    ('ที่', NULL, NULL, 'that; which', 'relative_pronoun'),
    ('ทำ', NULL, NULL, NULL, NULL),
    ('อยู่', NULL, NULL, 'continuing action', 'aspect_particle'),
    ('วัน', NULL, NULL, NULL, NULL),
    ('มี', NULL, NULL, NULL, NULL),
    ('สุข', NULL, NULL, NULL, NULL),
    ('มาก', NULL, NULL, NULL, NULL),
    ('ดู', NULL, NULL, 'to look; to appear', 'verb'),
    ('ไม่', NULL, NULL, NULL, NULL),
    ('คุณ', NULL, NULL, NULL, NULL),
    ('นี้', NULL, NULL, NULL, NULL),
    ('ถึง', NULL, NULL, 'although; even though', 'conjunction'),
    ('เขา', NULL, NULL, NULL, NULL),
    ('ยัง', NULL, NULL, NULL, NULL),
    ('เหมือน', NULL, NULL, NULL, NULL),
    ('เธอ', NULL, NULL, NULL, NULL),
    ('เดิน', NULL, NULL, NULL, NULL),
    ('และ', NULL, NULL, NULL, NULL),
    ('อยาก', NULL, NULL, NULL, NULL),
    ('ชอบ', 'chɔ̂ɔp', 'tɕʰɔ̂ːp', 'to like', 'verb'),
    ('งาน', 'ngaan', 'ŋāːn', 'work; job', 'noun'),
    ('ทุก', 'túk', 'tʰúk', 'every', 'determiner'),
    ('ก็', 'gɔ̂', 'kɔ̂ʔ', 'then; also', 'particle'),
    ('เลย', 'ləəi', 'lɤ̄ːj', 'therefore; emphatic particle', 'particle'),
    ('ความสุข', 'kwaam-sùk', 'kʰwāːm.sùk', 'happiness', 'noun'),
    ('ทอม', 'tɔm', 'tʰɔ̄m', 'Tom', 'proper_noun'),
    ('ทำไม', 'tam-mai', 'tʰām.māj', 'why', 'interrogative'),
    ('รู้สึก', 'rúu-sʉ̀k', 'rúː.sɯ̀k', 'to feel', 'verb'),
    ('เศร้า', 'sâo', 'sâw', 'sad', 'adjective'),
    ('จะ', 'jà', 'tɕàʔ', 'will; to', 'auxiliary'),
    ('ร้องไห้', 'rɔ́ɔng-hâai', 'rɔ́ːŋ.hâːj', 'to cry', 'verb'),
    ('เวลา', 'wee-laa', 'wēː.lāː', 'time', 'noun'),
    ('เป็น', 'bpen', 'pēn', 'to be', 'verb'),
    ('ไร', 'rai', 'rāj', 'what; anything', 'pronoun'),
    ('หรอก', 'rɔ̀k', 'rɔ̀k', 'reassuring or emphatic particle', 'particle'),
    ('ขอโทษ', 'kɔ̌ɔ-tôot', 'kʰɔ̌ː.tʰôːt', 'to apologize', 'verb'),
    ('โกรธ', 'gròot', 'kròːt', 'angry', 'adjective'),
    ('กลัว', 'gluua', 'klūa', 'afraid; to fear', 'verb'),
    ('สัตว์ป่า', 'sàt-bpàa', 'sàt.pàː', 'wild animal', 'noun'),
    ('เดินทาง', 'dəən-taang', 'dɤ̄ːn.tʰāːŋ', 'to travel', 'verb'),
    ('คนเดียว', 'kon-diao', 'kʰōn.dīaw', 'alone', 'adverb'),
    ('ว่ายน้ำ', 'wâai-náam', 'wâːj.náːm', 'to swim', 'verb'),
    ('เหนื่อย', 'nʉ̀ai', 'nɯ̀aj', 'tired', 'adjective'),
    ('จน', 'jon', 'tɕōn', 'until; to the point that', 'conjunction'),
    ('ไหว', 'wǎi', 'wǎj', 'to be able to manage', 'verb'),
    ('ไป', 'bpai', 'pāj', 'to go', 'verb'),
    ('นอน', 'nɔɔn', 'nɔ̄ːn', 'to sleep', 'verb'),
    ('ตื่นเต้น', 'dtʉ̀ʉn-dtên', 'tɯ̀ːn.tên', 'excited', 'adjective'),
    ('สุด', 'sùt', 'sùt', 'most; extremely', 'adverb');

CREATE TEMP TABLE mood_seed_sense ON COMMIT DROP AS
SELECT spelling, gloss, part_of_speech FROM mood_seed_word WHERE gloss IS NOT NULL
UNION ALL
SELECT * FROM (VALUES
    ('ที่', 'subordinating marker', 'particle'),
    ('ถึง', 'question emphasis', 'particle'),
    ('เวลา', 'when', 'conjunction')
) AS additional(spelling, gloss, part_of_speech);

CREATE TEMP TABLE mood_seed_sentence (
    source_id integer PRIMARY KEY,
    mood text NOT NULL,
    position integer NOT NULL,
    thai_text text NOT NULL,
    translation text NOT NULL,
    source_author text NOT NULL,
    tokens text[] NOT NULL,
    UNIQUE (mood, position)
) ON COMMIT DROP;

-- Each token array is the word-by-word breakdown. Compound lexical words are
-- retained as units; วันนี้ is วัน + นี้ and ไม่เป็นไร is ไม่ + เป็น + ไร.
INSERT INTO mood_seed_sentence VALUES
    (12858373, 'สุข', 1, 'ฉันชอบงานที่ฉันทำอยู่ ทุกวันก็เลยมีความสุขมาก',
     'I like my work, so I am very happy every day.', 'superteenkai',
     ARRAY['ฉัน','ชอบ','งาน','ที่','ฉัน','ทำ','อยู่','ทุก','วัน','ก็','เลย','มี','ความสุข','มาก']),
    (12589685, 'สุข', 2, 'ทอมดูไม่มีความสุข',
     'Tom looks unhappy.', 'superteenkai', ARRAY['ทอม','ดู','ไม่','มี','ความสุข']),
    (8675869, 'สุข', 3, 'ทำไมคุณมีความสุขมาก?',
     'Why are you so happy?', 'pastelite', ARRAY['ทำไม','คุณ','มี','ความสุข','มาก']),
    (11471502, 'เศร้า', 1, 'วันนี้ฉันรู้สึกเศร้า',
     'I feel sad today.', 'superteenkai', ARRAY['วัน','นี้','ฉัน','รู้สึก','เศร้า']),
    (12708327, 'เศร้า', 2, 'จะร้องไห้เวลาเศร้าก็ไม่เป็นไรหรอก',
     'It is okay to cry when you are sad.', 'superteenkai',
     ARRAY['จะ','ร้องไห้','เวลา','เศร้า','ก็','ไม่','เป็น','ไร','หรอก']),
    (2847987, 'โกรธ', 1, 'ถึงเขาจะขอโทษ ฉันก็ยังโกรธ',
     'Even though he apologized, I am still angry.', 'ojiisan',
     ARRAY['ถึง','เขา','จะ','ขอโทษ','ฉัน','ก็','ยัง','โกรธ']),
    (10189682, 'โกรธ', 2, 'คุณดูเหมือนโกรธมาก',
     'You look very angry.', 'Somsak', ARRAY['คุณ','ดู','เหมือน','โกรธ','มาก']),
    (8719784, 'โกรธ', 3, 'ทำไมคุณถึงโกรธ?',
     'Why are you angry?', 'pastelite', ARRAY['ทำไม','คุณ','ถึง','โกรธ']),
    (12529528, 'กลัว', 1, 'ฉันกลัวสัตว์ป่า',
     'I am afraid of wild animals.', 'superteenkai', ARRAY['ฉัน','กลัว','สัตว์ป่า']),
    (8669449, 'กลัว', 2, 'เธอกลัวที่จะเดินทางคนเดียว',
     'She was afraid to travel alone.', 'pastelite', ARRAY['เธอ','กลัว','ที่','จะ','เดินทาง','คนเดียว']),
    (8675801, 'กลัว', 3, 'เขากลัวที่จะว่ายน้ำ',
     'He is afraid to swim.', 'pastelite', ARRAY['เขา','กลัว','ที่','จะ','ว่ายน้ำ']),
    (12729650, 'เหนื่อย', 1, 'ฉันเหนื่อยจนเดินไม่ไหว',
     'I am too tired to walk.', 'superteenkai', ARRAY['ฉัน','เหนื่อย','จน','เดิน','ไม่','ไหว']),
    (8692086, 'เหนื่อย', 2, 'คุณดูเหนื่อยมาก',
     'You look very tired.', 'pastelite', ARRAY['คุณ','ดู','เหนื่อย','มาก']),
    (8728779, 'เหนื่อย', 3, 'ฉันเหนื่อยและอยากไปนอน',
     'I am tired and want to go to bed.', 'pastelite', ARRAY['ฉัน','เหนื่อย','และ','อยาก','ไป','นอน']),
    (11127363, 'ตื่นเต้น', 1, 'ฉันรู้สึกตื่นเต้นมาก',
     'I feel very excited.', 'Somsak', ARRAY['ฉัน','รู้สึก','ตื่นเต้น','มาก']),
    (2847975, 'ตื่นเต้น', 2, 'เธอดูตื่นเต้นสุดๆเลย',
     'She looks extremely excited.', 'ojiisan', ARRAY['เธอ','ดู','ตื่นเต้น','สุด','เลย']);

CREATE TEMP TABLE mood_seed_token ON COMMIT DROP AS
SELECT s.source_id, t.ordinality::integer AS word_order, t.spelling,
       CASE
           WHEN s.source_id = 12858373 AND t.ordinality = 7 THEN ' '
           WHEN s.source_id = 2847987 AND t.ordinality = 4 THEN ' '
           WHEN s.source_id IN (8675869, 8719784)
                AND t.ordinality = cardinality(s.tokens) THEN '?'
           WHEN s.source_id = 2847975 AND t.ordinality = 4 THEN 'ๆ'
           ELSE ''
       END AS following_text
FROM mood_seed_sentence s
CROSS JOIN LATERAL unnest(s.tokens) WITH ORDINALITY AS t(spelling, ordinality);

DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM mood_seed_sentence s
        JOIN mood_seed_token t ON t.source_id = s.source_id
        GROUP BY s.source_id, s.thai_text
        HAVING string_agg(t.spelling || t.following_text, '' ORDER BY t.word_order) <> s.thai_text
    ) THEN
        RAISE EXCEPTION 'A sentence breakdown does not reproduce its source text';
    END IF;
    IF EXISTS (
        SELECT 1 FROM mood_seed_token t
        LEFT JOIN mood_seed_word w ON w.spelling = t.spelling
        WHERE w.spelling IS NULL
    ) OR EXISTS (
        SELECT 1 FROM mood_seed_sentence s
        LEFT JOIN mood_seed_word w ON w.spelling = s.mood
        WHERE w.spelling IS NULL
    ) THEN
        RAISE EXCEPTION 'A sentence or mood references an unstaged word';
    END IF;
    IF EXISTS (
        SELECT 1 FROM mood_seed_word v
        JOIN thai."Spelling" s ON s.value = v.spelling
        JOIN thai."Word" w ON w.spelling_id = s.id
        GROUP BY v.spelling HAVING count(*) > 1
    ) THEN
        RAISE EXCEPTION 'A spelling has multiple existing words; choose its pronunciation explicitly';
    END IF;
    IF EXISTS (
        SELECT 1 FROM mood_seed_word v
        LEFT JOIN thai."Spelling" s ON s.value = v.spelling
        LEFT JOIN thai."Word" w ON w.spelling_id = s.id
        LEFT JOIN thai."Transcription" t ON t.id = w.transcription
        WHERE (v.paiboon IS NULL AND w.id IS NULL)
           OR (v.paiboon IS NOT NULL AND w.id IS NOT NULL
               AND (normalize(t.paiboon, NFC) IS DISTINCT FROM normalize(v.paiboon, NFC)
                 OR normalize(t."IPA", NFC) IS DISTINCT FROM normalize(v.ipa, NFC)))
    ) THEN
        RAISE EXCEPTION 'A required existing word is missing or has a different pronunciation';
    END IF;
    IF EXISTS (
        SELECT 1 FROM mood_seed_word v
        JOIN thai."Transcription" t
          ON normalize(t.paiboon, NFC) = normalize(v.paiboon, NFC)
          OR normalize(t."IPA", NFC) = normalize(v.ipa, NFC)
        WHERE normalize(t.paiboon, NFC) IS DISTINCT FROM normalize(v.paiboon, NFC)
           OR normalize(t."IPA", NFC) IS DISTINCT FROM normalize(v.ipa, NFC)
    ) THEN
        RAISE EXCEPTION 'IPA or Paiboon collides with a different existing transcription';
    END IF;
    IF EXISTS (
        SELECT 1 FROM mood_seed_word WHERE paiboon IS NOT NULL
        GROUP BY normalize(paiboon, NFC) HAVING count(*) > 1
    ) OR EXISTS (
        SELECT 1 FROM mood_seed_word WHERE ipa IS NOT NULL
        GROUP BY normalize(ipa, NFC) HAVING count(*) > 1
    ) THEN
        RAISE EXCEPTION 'The seed contains duplicate IPA or Paiboon values';
    END IF;
    IF EXISTS (
        SELECT 1 FROM thai."MoodSentence" ms
        JOIN mood_seed_sentence s
          ON ms.source_url = 'https://tatoeba.org/en/sentences/show/' || s.source_id
    ) THEN
        RAISE EXCEPTION 'At least one sample sentence has already been inserted';
    END IF;
END $$;

INSERT INTO thai."Spelling"(value)
SELECT spelling FROM mood_seed_word
ON CONFLICT (value) DO NOTHING;

INSERT INTO thai."Transcription"(paiboon, "IPA")
SELECT normalize(v.paiboon, NFC), normalize(v.ipa, NFC)
FROM mood_seed_word v
WHERE v.paiboon IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM thai."Transcription" t
      WHERE normalize(t.paiboon, NFC) = normalize(v.paiboon, NFC)
        AND normalize(t."IPA", NFC) = normalize(v.ipa, NFC)
  );

INSERT INTO thai."Word"(spelling_id, transcription, is_primary)
SELECT s.id, t.id, true
FROM mood_seed_word v
JOIN thai."Spelling" s ON s.value = v.spelling
JOIN thai."Transcription" t
  ON normalize(t.paiboon, NFC) = normalize(v.paiboon, NFC)
 AND normalize(t."IPA", NFC) = normalize(v.ipa, NFC)
WHERE NOT EXISTS (SELECT 1 FROM thai."Word" w WHERE w.spelling_id = s.id);

INSERT INTO thai."Lexeme"(value, part_of_speech)
SELECT DISTINCT gloss, part_of_speech FROM mood_seed_sense
ON CONFLICT (value, part_of_speech) DO NOTHING;

INSERT INTO thai."WordLexeme"(word, lexeme)
SELECT w.id, l.id
FROM mood_seed_sense v
JOIN thai."Spelling" s ON s.value = v.spelling
JOIN thai."Word" w ON w.spelling_id = s.id
JOIN thai."Lexeme" l ON l.value = v.gloss AND l.part_of_speech = v.part_of_speech
WHERE NOT EXISTS (
    SELECT 1 FROM thai."WordLexeme" wl WHERE wl.word = w.id AND wl.lexeme = l.id
);

CREATE TEMP TABLE mood_seed_sentence_id ON COMMIT DROP AS
SELECT source_id,
       (SELECT COALESCE(max(id), 0) FROM thai."Sentence")
       + row_number() OVER (ORDER BY source_id)::integer AS sentence_id
FROM mood_seed_sentence;

INSERT INTO thai."Sentence"(id, translation)
SELECT i.sentence_id, s.translation
FROM mood_seed_sentence s JOIN mood_seed_sentence_id i USING (source_id);

INSERT INTO thai."SentenceWord"(sentence_id, word_id, word_order, following_text)
SELECT i.sentence_id, w.id, t.word_order, t.following_text
FROM mood_seed_token t
JOIN mood_seed_sentence_id i USING (source_id)
JOIN thai."Spelling" s ON s.value = t.spelling
JOIN thai."Word" w ON w.spelling_id = s.id;

INSERT INTO thai."Mood"(word_id)
SELECT DISTINCT w.id
FROM mood_seed_sentence v
JOIN thai."Spelling" s ON s.value = v.mood
JOIN thai."Word" w ON w.spelling_id = s.id
ON CONFLICT (word_id) DO NOTHING;

INSERT INTO thai."MoodSentence"(mood_id, sentence_id, position, source_url, source_author, source_license)
SELECT m.id, i.sentence_id, v.position,
       'https://tatoeba.org/en/sentences/show/' || v.source_id,
       v.source_author, 'CC BY 2.0 FR'
FROM mood_seed_sentence v
JOIN mood_seed_sentence_id i USING (source_id)
JOIN thai."Spelling" s ON s.value = v.mood
JOIN thai."Word" w ON w.spelling_id = s.id
JOIN thai."Mood" m ON m.word_id = w.id;

DO $$
BEGIN
    IF (SELECT count(*) FROM thai."MoodSentence" ms
        JOIN mood_seed_sentence_id i ON i.sentence_id = ms.sentence_id) <> 16
       OR (SELECT count(*) FROM thai."SentenceWord" sw
           JOIN mood_seed_sentence_id i ON i.sentence_id = sw.sentence_id)
          <> (SELECT count(*) FROM mood_seed_token)
       OR EXISTS (
           SELECT 1 FROM mood_seed_sentence s
           JOIN mood_seed_sentence_id i USING (source_id)
           JOIN thai."SentenceWord" sw ON sw.sentence_id = i.sentence_id
           JOIN thai."Word" w ON w.id = sw.word_id
           JOIN thai."Spelling" sp ON sp.id = w.spelling_id
           GROUP BY s.source_id, s.thai_text
           HAVING string_agg(sp.value || sw.following_text, '' ORDER BY sw.word_order) <> s.thai_text
       ) THEN
        RAISE EXCEPTION 'Inserted sentence dependencies do not match the sample dataset';
    END IF;
END $$;

REFRESH MATERIALIZED VIEW thai."WordView";
COMMIT;
