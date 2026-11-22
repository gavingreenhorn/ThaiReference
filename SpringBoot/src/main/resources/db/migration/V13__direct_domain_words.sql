DO $$
DECLARE
    entry record;
    part record;
    spelling_id integer;
    transcription_id integer;
    lexeme_id integer;
    component_id integer;
    whole_id integer;
    candidate_count integer;
    component_ids integer[];
    existing_ids integer[];
    current_spelling text;
    original_names jsonb;
    original_lexemes jsonb;
BEGIN
    UPDATE thai."Spelling" s SET value = 'เพชร'
    FROM thai."SimpleWord" sw
    WHERE sw.id = 669 AND sw.spelling_id = s.id AND s.value = 'เพรช';

    FOR entry IN
        SELECT * FROM (VALUES
            ('เฒ่า', 'tâo', 'tʰâw', 'aged; very old', 'adjective', 18),
            ('ฮูก', 'hûuk', 'hûːk', 'owl', 'noun', 44),
            ('ไต่', 'dtài', 'tàj', 'to crawl; to climb', 'verb', 128),
            ('คู้', 'kúu', 'kʰúː', 'to bend; to curl', 'verb', 128),
            ('ไปยาล', 'bpai-yaan', 'pāj.jāːn', 'abbreviation symbol', 'noun', 129),
            ('ยมก', 'yá-mók', 'já.mók', 'pair; twin', 'noun', 130),
            ('พวก', 'pûuak', 'pʰûak', 'group', 'noun', 143),
            ('บาง', 'baang', 'bāːŋ', 'some; certain', 'determiner', 172),
            ('พระ', 'phrá', 'pʰráʔ', 'god; deity', 'noun', 244)
        ) AS missing(spelling, paiboon, ipa, gloss, pos, target_id)
    LOOP
        IF NOT EXISTS (SELECT 1 FROM thai."Word" WHERE id = entry.target_id) THEN
            RAISE NOTICE 'Target % is absent; component % is not needed', entry.target_id, entry.spelling;
            CONTINUE;
        END IF;
        INSERT INTO thai."Spelling"(value) VALUES (entry.spelling) ON CONFLICT (value) DO NOTHING;
        SELECT id INTO STRICT spelling_id FROM thai."Spelling" WHERE value = entry.spelling;
        SELECT count(*), min(sw.id) INTO candidate_count, component_id
        FROM thai."SimpleWord" sw JOIN thai."Spelling" s ON s.id = sw.spelling_id
        WHERE s.value = entry.spelling;
        IF candidate_count = 0 THEN
            SELECT count(*), min(id) INTO candidate_count, transcription_id
            FROM thai."Transcription"
            WHERE normalize(paiboon, NFC) = normalize(entry.paiboon, NFC)
              AND normalize("IPA", NFC) = normalize(entry.ipa, NFC);
            IF candidate_count = 0 THEN
                INSERT INTO thai."Transcription"(paiboon, "IPA") VALUES (entry.paiboon, entry.ipa)
                RETURNING id INTO transcription_id;
            ELSIF candidate_count <> 1 THEN
                RAISE EXCEPTION 'Ambiguous transcription for %', entry.spelling;
            END IF;
            INSERT INTO thai."Word"(kind, is_primary) VALUES ('simple', true) RETURNING id INTO component_id;
            INSERT INTO thai."SimpleWord"(id, spelling_id, transcription)
            VALUES (component_id, spelling_id, transcription_id);
        ELSIF candidate_count <> 1 THEN
            RAISE EXCEPTION 'Ambiguous component %', entry.spelling;
        END IF;
        IF NOT EXISTS (
            SELECT 1 FROM thai."WordLexeme" wl JOIN thai."Lexeme" l ON l.id = wl.lexeme
            WHERE wl.word = component_id AND l.value = entry.gloss
        ) THEN
            INSERT INTO thai."Lexeme"(value, part_of_speech) VALUES (entry.gloss, entry.pos)
            ON CONFLICT (value, part_of_speech) DO NOTHING;
            SELECT id INTO STRICT lexeme_id FROM thai."Lexeme"
            WHERE value = entry.gloss AND part_of_speech = entry.pos;
            INSERT INTO thai."WordLexeme"(word, lexeme) VALUES (component_id, lexeme_id);
        END IF;
    END LOOP;

    IF EXISTS (SELECT 1 FROM thai."SimpleWord" WHERE id = 187) THEN
        SELECT count(*), min(id) INTO candidate_count, transcription_id
        FROM thai."Transcription"
        WHERE normalize(paiboon, NFC) = normalize('phrʉ́-hàt-sà-bɔɔ-dii', NFC)
          AND normalize("IPA", NFC) = normalize('pʰrɯ́.hàt.sà.bɔ̄ː.dīː', NFC);
        IF candidate_count = 0 THEN
            INSERT INTO thai."Transcription"(paiboon, "IPA")
            VALUES ('phrʉ́-hàt-sà-bɔɔ-dii', 'pʰrɯ́.hàt.sà.bɔ̄ː.dīː')
            RETURNING id INTO transcription_id;
        ELSIF candidate_count <> 1 THEN
            RAISE EXCEPTION 'Ambiguous full pronunciation for พฤหัสบดี';
        END IF;
        UPDATE thai."SimpleWord" SET transcription = transcription_id WHERE id = 187;
    END IF;

    FOR entry IN
        SELECT * FROM (VALUES
            (594, 'mix', 'verb'), (669, 'diamond', 'noun'),
            (581, 'bird', 'noun'), (183, 'sun', 'noun'), (185, 'Mars', 'noun'),
            (186, 'Mercury', 'noun'), (187, 'Jupiter', 'noun'),
            (188, 'Venus', 'noun'), (189, 'Saturn', 'noun'),
            (520, 'north', 'noun'), (521, 'south', 'noun'),
            (524, 'oblique; diagonal', 'adjective'), (648, 'to go out', 'verb'),
            (541, 'agarwood', 'noun'), (546, 'parakeet', 'noun'),
            (547, 'crow', 'noun'), (549, 'karawik (mythical bird)', 'noun'),
            (566, 'Chinese', 'adjective'), (596, 'wing', 'noun'),
            (624, 'lacquer tree', 'noun'), (639, 'ink', 'noun'),
            (640, 'pig', 'noun'), (668, 'albino', 'adjective'), (668, 'taro', 'noun'),
            (675, 'blood', 'noun'), (683, 'Bermuda grass', 'noun'),
            (684, 'cat', 'noun'), (502, 'tusk; ivory', 'noun')
        ) AS senses(word_id, gloss, pos)
    LOOP
        IF EXISTS (SELECT 1 FROM thai."Word" WHERE id = entry.word_id)
           AND NOT EXISTS (
               SELECT 1 FROM thai."WordLexeme" wl JOIN thai."Lexeme" l ON l.id = wl.lexeme
               WHERE wl.word = entry.word_id AND lower(l.value) = lower(entry.gloss)
           ) THEN
            INSERT INTO thai."Lexeme"(value, part_of_speech) VALUES (entry.gloss, entry.pos)
            ON CONFLICT (value, part_of_speech) DO NOTHING;
            SELECT id INTO STRICT lexeme_id FROM thai."Lexeme"
            WHERE value = entry.gloss AND part_of_speech = entry.pos;
            INSERT INTO thai."WordLexeme"(word, lexeme) VALUES (entry.word_id, lexeme_id);
        END IF;
    END LOOP;

    REFRESH MATERIALIZED VIEW thai."WordView";
    SELECT jsonb_agg(jsonb_build_object('id', cn.id, 'value',
        concat_ws('', w1.spelling, w2.spelling, w3.spelling, w4.spelling, w5.spelling)) ORDER BY cn.id)
    INTO original_names
    FROM thai."CompositeName" cn
    LEFT JOIN thai."WordView" w1 ON w1.id = cn.word1
    LEFT JOIN thai."WordView" w2 ON w2.id = cn.word2
    LEFT JOIN thai."WordView" w3 ON w3.id = cn.word3
    LEFT JOIN thai."WordView" w4 ON w4.id = cn.word4
    LEFT JOIN thai."WordView" w5 ON w5.id = cn.word5;
    SELECT jsonb_agg(to_jsonb(wl) ORDER BY wl.id) INTO original_lexemes
    FROM thai."WordLexeme" wl WHERE word IN (18,44,128,129,130,143,172,244,245,246,247,248,249,250,406);

    FOR entry IN
        SELECT * FROM (VALUES
            (18, NULL, 'ผู้เฒ่า', ARRAY['ผู้','เฒ่า'], NULL, 0),
            (44, NULL, 'นกฮูก', ARRAY['นก','ฮูก'], NULL, 0),
            (128, NULL, 'ไม้ไต่คู้', ARRAY['ไม้','ไต่','คู้'], NULL, 0),
            (129, NULL, 'ไปยาลน้อย', ARRAY['ไปยาล','น้อย'], NULL, 0),
            (130, NULL, 'ไม้ยมก', ARRAY['ไม้','ยมก'], NULL, 0),
            (143, NULL, 'พวกเขา', ARRAY['พวก','เขา'], NULL, 0),
            (172, NULL, 'บางคน', ARRAY['บาง','คน'], NULL, 0),
            (244, NULL, 'พระอาทิตย์', ARRAY['พระ','อาทิตย์'], NULL, 0),
            (245, NULL, 'พระจันทร์', ARRAY['พระ','จันทร์'], NULL, 0),
            (246, NULL, 'พระอังคาร', ARRAY['พระ','อังคาร'], NULL, 0),
            (247, NULL, 'พระพุธ', ARRAY['พระ','พุธ'], NULL, 0),
            (248, NULL, 'พระพฤหัสบดี', ARRAY['พระ','พฤหัสบดี'], NULL, 0),
            (249, NULL, 'พระศุกร์', ARRAY['พระ','ศุกร์'], NULL, 0),
            (250, NULL, 'พระเสาร์', ARRAY['พระ','เสาร์'], NULL, 0),
            (406, NULL, 'เฉพาะทาง', ARRAY['เฉพาะ','ทาง'], NULL, 0),
            (NULL, 1, 'ไม้เอก', ARRAY['ไม้','เอก'], 'first Thai tone mark', 0),
            (NULL, 2, 'ไม้โท', ARRAY['ไม้','โท'], 'second Thai tone mark', 0),
            (NULL, 3, 'ไม้ตรี', ARRAY['ไม้','ตรี'], 'third Thai tone mark', 0),
            (NULL, 4, 'ไม้จัตวา', ARRAY['ไม้','จัตวา'], 'fourth Thai tone mark', 0),
            (NULL, 5, 'เสียงสามัญ', ARRAY['เสียง','สามัญ'], 'mid tone', 0),
            (NULL, 6, 'เสียงเอก', ARRAY['เสียง','เอก'], 'low tone', 0),
            (NULL, 7, 'เสียงโท', ARRAY['เสียง','โท'], 'falling tone', 0),
            (NULL, 8, 'เสียงตรี', ARRAY['เสียง','ตรี'], 'high tone', 0),
            (NULL, 9, 'เสียงจัตวา', ARRAY['เสียง','จัตวา'], 'rising tone', 0),
            (NULL, 11, 'ตะวันออกเฉียงเหนือ', ARRAY['ตะวัน','ออก','เฉียง','เหนือ'], 'northeast', 0),
            (NULL, 13, 'ตะวันออกเฉียงใต้', ARRAY['ตะวัน','ออก','เฉียง','ใต้'], 'southeast', 0),
            (NULL, 15, 'ตะวันตกเฉียงใต้', ARRAY['ตะวัน','ตก','เฉียง','ใต้'], 'southwest', 0),
            (NULL, 17, 'ตะวันตกเฉียงเหนือ', ARRAY['ตะวัน','ตก','เฉียง','เหนือ'], 'northwest', 0),
            (NULL, 221, 'อักษรต่ำ', ARRAY['อักษร','ต่ำ'], 'low-class consonants', 0),
            (NULL, 222, 'อักษรกลาง', ARRAY['อักษร','กลาง'], 'mid-class consonants', 0),
            (NULL, 223, 'อักษรสูง', ARRAY['อักษร','สูง'], 'high-class consonants', 0),
            (NULL, 224, 'ฤดูหนาว', ARRAY['ฤดู','หนาว'], 'cold season', 0),
            (NULL, 225, 'ฤดูร้อน', ARRAY['ฤดู','ร้อน'], 'hot season', 0),
            (NULL, 226, 'ฤดูฝน', ARRAY['ฤดู','ฝน'], 'rainy season', 0),
            (NULL, 42, 'เลือดหมู', ARRAY['เลือด','หมู'], 'maroon', 0),
            (NULL, 60, 'น้ำหมาก', ARRAY['น้ำ','หมาก'], 'betel juice', 0),
            (NULL, 70, 'ไม้กฤษณา', ARRAY['ไม้','กฤษณา'], 'agarwood', 0),
            (NULL, 103, 'ปีกกา', ARRAY['ปีก','กา'], 'crow wing', 0),
            (NULL, 112, 'ตาแมว', ARRAY['ตา','แมว'], 'cat''s-eye gemstone', 0),
            (NULL, 161, 'ควายเผือก', ARRAY['ควาย','เผือก'], 'albino buffalo', 0),
            (NULL, 192, 'เหล็กไหล', ARRAY['เหล็ก','ไหล'], 'legendary metal', 0),
            (NULL, 193, 'น้ำรัก', ARRAY['น้ำ','รัก'], 'lacquer resin', 0),
            (NULL, 208, 'หมึกจีน', ARRAY['หมึก','จีน'], 'Chinese ink', 0),
            (NULL, 212, 'งาช้าง', ARRAY['งา','ช้าง'], 'ivory', 0),
            (NULL, 64, 'หญ้าแพรก', ARRAY['หญ้า','แพรก'], 'Bermuda grass', 1),
            (NULL, 88, 'นกกะลิง', ARRAY['นก','กะลิง'], 'parakeet', 2),
            (NULL, 107, 'นกการเวก', ARRAY['นก','การเวก'], 'karawik (mythical bird)', 3)
        ) AS mapping(word_id, name_id, spelling, parts, gloss, embedded_position)
    LOOP
        whole_id := entry.word_id;
        IF (whole_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM thai."Word" WHERE id = whole_id))
           OR (entry.name_id IS NOT NULL AND NOT EXISTS (
               SELECT 1 FROM thai."CompositeName" WHERE id = entry.name_id
           )) THEN
            RAISE NOTICE 'Approved target % / % is absent', whole_id, entry.name_id;
            CONTINUE;
        END IF;
        IF array_to_string(entry.parts, '') <> entry.spelling THEN
            RAISE EXCEPTION 'Component spellings do not form %', entry.spelling;
        END IF;
        component_ids := ARRAY[]::integer[];
        FOR part IN SELECT value FROM unnest(entry.parts) AS p(value)
        LOOP
            SELECT count(*), min(sw.id) INTO candidate_count, component_id
            FROM thai."SimpleWord" sw JOIN thai."Spelling" s ON s.id = sw.spelling_id
            WHERE s.value = part.value;
            IF candidate_count <> 1 THEN
                RAISE EXCEPTION 'Expected one SimpleWord for %, found %', part.value, candidate_count;
            END IF;
            component_ids := array_append(component_ids, component_id);
        END LOOP;
        IF whole_id IS NULL THEN
            SELECT count(*), min(id) INTO candidate_count, whole_id
            FROM thai."WordView" WHERE spelling = entry.spelling;
            IF candidate_count > 1 THEN
                RAISE EXCEPTION 'Ambiguous whole word %', entry.spelling;
            ELSIF candidate_count = 0 THEN
                INSERT INTO thai."Word"(kind, is_primary) VALUES ('compound', true) RETURNING id INTO whole_id;
            END IF;
        ELSE
            SELECT spelling INTO STRICT current_spelling FROM thai."WordView" WHERE id = whole_id;
            IF current_spelling <> entry.spelling THEN
                RAISE EXCEPTION 'Word % does not spell %', whole_id, entry.spelling;
            END IF;
        END IF;
        IF EXISTS (SELECT 1 FROM thai."CompoundWord" WHERE id = whole_id) THEN
            SELECT array_agg(word_id ORDER BY position) INTO existing_ids
            FROM thai."WordComponent" WHERE compound_id = whole_id;
            IF existing_ids IS DISTINCT FROM component_ids THEN
                RAISE EXCEPTION 'Existing compound % has different components', whole_id;
            END IF;
        ELSE
            DELETE FROM thai."SimpleWord" WHERE id = whole_id;
            UPDATE thai."Word" SET kind = 'compound' WHERE id = whole_id;
            INSERT INTO thai."CompoundWord"(id) VALUES (whole_id);
            INSERT INTO thai."WordComponent"(compound_id, word_id, position)
            SELECT whole_id, p.id, p.position FROM unnest(component_ids) WITH ORDINALITY AS p(id, position);
        END IF;
        IF entry.gloss IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM thai."WordLexeme" wl JOIN thai."Lexeme" l ON l.id = wl.lexeme
            WHERE wl.word = whole_id AND l.value = entry.gloss
        ) THEN
            INSERT INTO thai."Lexeme"(value, part_of_speech) VALUES (entry.gloss, 'noun')
            ON CONFLICT (value, part_of_speech) DO NOTHING;
            SELECT id INTO STRICT lexeme_id FROM thai."Lexeme"
            WHERE value = entry.gloss AND part_of_speech = 'noun';
            INSERT INTO thai."WordLexeme"(word, lexeme) VALUES (whole_id, lexeme_id);
        END IF;
        IF entry.name_id IS NOT NULL THEN
            CASE entry.embedded_position
                WHEN 0 THEN UPDATE thai."CompositeName" SET
                    word1 = whole_id, word2 = NULL, word3 = NULL, word4 = NULL, word5 = NULL
                    WHERE id = entry.name_id;
                WHEN 1 THEN UPDATE thai."CompositeName" SET
                    word1 = whole_id, word2 = word3, word3 = NULL, word4 = NULL, word5 = NULL
                    WHERE id = entry.name_id;
                WHEN 2 THEN UPDATE thai."CompositeName" SET
                    word2 = whole_id, word3 = NULL, word4 = NULL, word5 = NULL WHERE id = entry.name_id;
                WHEN 3 THEN UPDATE thai."CompositeName" SET
                    word3 = whole_id, word4 = NULL, word5 = NULL WHERE id = entry.name_id;
            END CASE;
        END IF;
    END LOOP;
    IF EXISTS (SELECT 1 FROM thai."CompositeName" WHERE id = 26) THEN
        IF NOT EXISTS (SELECT 1 FROM thai."SimpleWord" WHERE id = 532) THEN
            RAISE EXCEPTION 'ท่ามกลาง must reuse SimpleWord 532';
        END IF;
        UPDATE thai."CompositeName" SET word1 = 532, word2 = NULL, word3 = NULL, word4 = NULL, word5 = NULL
        WHERE id = 26;
    END IF;
    SET CONSTRAINTS ALL IMMEDIATE;
    REFRESH MATERIALIZED VIEW thai."WordView";
    IF original_names IS DISTINCT FROM (
        SELECT jsonb_agg(jsonb_build_object('id', cn.id, 'value',
            concat_ws('', w1.spelling, w2.spelling, w3.spelling, w4.spelling, w5.spelling)) ORDER BY cn.id)
        FROM thai."CompositeName" cn
        LEFT JOIN thai."WordView" w1 ON w1.id = cn.word1
        LEFT JOIN thai."WordView" w2 ON w2.id = cn.word2
        LEFT JOIN thai."WordView" w3 ON w3.id = cn.word3
        LEFT JOIN thai."WordView" w4 ON w4.id = cn.word4
        LEFT JOIN thai."WordView" w5 ON w5.id = cn.word5
    ) THEN
        RAISE EXCEPTION 'Word migration changed a composite name spelling';
    END IF;
    IF original_lexemes IS DISTINCT FROM (
        SELECT jsonb_agg(to_jsonb(wl) ORDER BY wl.id) FROM thai."WordLexeme" wl
        WHERE word IN (18,44,128,129,130,143,172,244,245,246,247,248,249,250,406)
    ) THEN
        RAISE EXCEPTION 'Word migration changed existing whole-word lexeme links';
    END IF;
END
$$;

ALTER TABLE thai."Direction" ADD COLUMN word_id integer REFERENCES thai."Word"(id);
UPDATE thai."Direction" d SET word_id = cn.word1 FROM thai."CompositeName" cn
WHERE cn.id = d.composite_name_id AND cn.word2 IS NULL;
ALTER TABLE thai."Direction" ALTER COLUMN word_id SET NOT NULL, DROP COLUMN composite_name_id;

ALTER TABLE thai."Class" ADD COLUMN word_id integer REFERENCES thai."Word"(id);
UPDATE thai."Class" c SET word_id = cn.word1 FROM thai."CompositeName" cn
WHERE cn.id = c.name AND cn.word2 IS NULL;
ALTER TABLE thai."Class" ALTER COLUMN word_id SET NOT NULL, DROP COLUMN name;

ALTER TABLE thai."Season" ADD COLUMN word_id integer REFERENCES thai."Word"(id);
UPDATE thai."Season" s SET word_id = cn.word1 FROM thai."CompositeName" cn
WHERE cn.id = s.name AND cn.word2 IS NULL;
ALTER TABLE thai."Season" ALTER COLUMN word_id SET NOT NULL, DROP COLUMN name;

ALTER TABLE thai."ToneMark" ADD COLUMN word_id integer REFERENCES thai."Word"(id);
UPDATE thai."ToneMark" tm SET word_id = cn.word1 FROM thai."CompositeName" cn
WHERE cn.id = tm.name AND cn.word2 IS NULL;
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM thai."ToneMark" WHERE name IS NOT NULL AND word_id IS NULL) THEN
        RAISE EXCEPTION 'A named tone mark could not be migrated to a word';
    END IF;
END
$$;
ALTER TABLE thai."ToneMark" DROP COLUMN name;

ALTER TABLE thai."Color" ADD COLUMN word_id integer REFERENCES thai."Word"(id);
UPDATE thai."Color" c SET word_id = cn.word1, composite_name_id = NULL
FROM thai."CompositeName" cn WHERE cn.id = c.composite_name_id AND cn.word2 IS NULL;
ALTER TABLE thai."Color" ADD CONSTRAINT "Color_label_reference_check"
    CHECK ((word_id IS NOT NULL) <> (composite_name_id IS NOT NULL));

REFRESH MATERIALIZED VIEW thai."MinimalSetView";
