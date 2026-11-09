-- Meanings and tones verified in SEAlang's Thai Dictionary Project entries:
-- https://sealang.net/thai/search.pl?dict=thai&orth=<Thai spelling>
-- Source provenance: https://sealang.net/thai/dictionary-bot1.htm
DO $cleanup$
DECLARE
	r record;
	fk record;
	tid integer;
	lid integer;
	missing bigint;
BEGIN
	FOR r IN SELECT * FROM (VALUES
		(466,157,'ข้า'), (473,5,'คน'), (478,162,'นี่'), (468,29,'ฝา'),
		(440,55,'มา'), (442,33,'ม้า'), (471,177,'ล่าง'), (443,265,'หมา'),
		(480,376,'หลัง'), (454,205,'เที่ยง')
	) AS mapping(old_id, kept_id, spelling)
	LOOP
		IF NOT EXISTS (
			SELECT 1 FROM thai."Word" w JOIN thai."Spelling" s ON s.id=w.spelling_id
			WHERE w.id=r.kept_id AND s.value=r.spelling
		) OR EXISTS (
			SELECT 1 FROM thai."Word" w JOIN thai."Spelling" s ON s.id=w.spelling_id
			WHERE w.id=r.old_id AND s.value<>r.spelling
		) THEN
			RAISE EXCEPTION 'Unexpected word identity for %', r.spelling;
		END IF;
		SELECT transcription INTO tid FROM thai."Word" WHERE id=r.old_id;
		FOR fk IN
			SELECT c.conrelid, a.attname AS col
			FROM pg_constraint c
			JOIN pg_attribute a ON a.attrelid=c.conrelid AND a.attnum=c.conkey[1]
			WHERE c.contype='f' AND c.confrelid='thai."Word"'::regclass
		LOOP
			EXECUTE format('UPDATE %s SET %I=$1 WHERE %I=$2',
				fk.conrelid::regclass, fk.col, fk.col) USING r.kept_id,r.old_id;
		END LOOP;
		DELETE FROM thai."Word" WHERE id=r.old_id;
		DELETE FROM thai."Transcription" t WHERE t.id=tid
		  AND NOT EXISTS (SELECT 1 FROM thai."Word" w WHERE w.transcription=t.id)
		  AND NOT EXISTS (SELECT 1 FROM thai."Consonant" c WHERE c.transcription=t.id);
	END LOOP;

	-- Merge only individually verified synonymous senses, not arbitrary text matches.
	FOR r IN SELECT * FROM (VALUES
		(618,522,'I, me','I; Me'), (628,5,'person, people','person'),
		(621,29,'lid, cover','lid'), (586,83,'to come','come'),
		(588,33,'horse','horse'), (589,368,'dog','dog'),
		(626,216,'lower, below','bottom'), (603,244,'noon, midday','noon; 12:00')
	) AS mapping(old_id, kept_id, old_value, kept_value)
	LOOP
		IF NOT EXISTS (SELECT 1 FROM thai."Lexeme" WHERE id=r.kept_id AND value=r.kept_value)
		   OR EXISTS (SELECT 1 FROM thai."Lexeme" WHERE id=r.old_id AND value<>r.old_value) THEN
			RAISE EXCEPTION 'Unexpected lexeme identity % -> %',r.old_id,r.kept_id;
		END IF;
		UPDATE thai."WordLexeme" SET lexeme=r.kept_id WHERE lexeme=r.old_id;
		DELETE FROM thai."Lexeme" WHERE id=r.old_id;
	END LOOP;

	-- Distinct senses remain separate: this/here, lid/wall, back/behind, noon/accurate.
	UPDATE thai."Lexeme" SET value='here',part_of_speech='pronoun'
	WHERE id=633 AND value='here, this (proximal)';
	UPDATE thai."Lexeme" SET value='accurate, precise, steady, stable',part_of_speech='adjective'
	WHERE id=604 AND value='straight, exact, on time';

	DELETE FROM thai."WordLexeme" a USING thai."WordLexeme" b
	WHERE a.word=b.word AND a.lexeme=b.lexeme AND a.id>b.id;
	DELETE FROM thai."MinimalSetWord" a USING thai."MinimalSetWord" b
	WHERE a.set_id=b.set_id AND a.word_id=b.word_id AND a.id>b.id;

	-- Correct the retained pronunciations only when they match the audited old values.
	FOR r IN SELECT * FROM (VALUES
		(29,'ฝา',U&'fa\02D0',U&'fa\030C\02D0','fǎa'),
		(33,'ม้า',U&'ma\02D0',U&'ma\0301\02D0','máa'),
		(177,'ล่าง',U&'la\02D0\014B',U&'la\0302\02D0\014B','lâang'),
		(376,'หลัง',U&'la\014B',U&'la\030C\014B','lǎng')
	) AS correction(word_id,spelling,old_ipa,new_ipa,paiboon)
	LOOP
		UPDATE thai."Transcription" t SET "IPA"=r.new_ipa,paiboon=r.paiboon
		FROM thai."Word" w WHERE w.id=r.word_id AND w.transcription=t.id
		  AND normalize(t."IPA",NFD)=r.old_ipa;
		IF NOT EXISTS (
			SELECT 1 FROM thai."Word" w JOIN thai."Transcription" t ON t.id=w.transcription
			WHERE w.id=r.word_id AND normalize(t."IPA",NFD)=r.new_ipa
		) THEN RAISE EXCEPTION 'Unexpected retained pronunciation for %',r.spelling; END IF;
	END LOOP;

	-- Each row below is one sense; comma-separated synonyms are one lexeme.
	FOR r IN SELECT * FROM (VALUES
		(556,'ขน','fur, hair','noun'), (556,'ขน','feathers, bristles, quills','noun'),
		(556,'ขน','carry, transport, transfer','verb'),
		(564,'คำ','word, speech, utterance','noun'), (564,'คำ','gold','noun'),
		(564,'คำ','mouthful, bite','classifier'),
		(584,'นาก','otter','noun'), (584,'นาก','alloy of gold and copper','noun'),
		(585,'นาง','woman, lady','noun'), (585,'นาง','Mrs.','title'),
		(585,'นาง','she, her','pronoun'),
		(610,'ภู','hill, mountain','noun'), (631,'สังข์','conch shell','noun'),
		(634,'สุก','ripe','adjective'), (634,'สุก','cooked, done','adjective'),
		(634,'สุก','radiant, shiny','adjective'),
		(635,'สุข','happy, at ease, content','adjective'), (635,'สุข','healthy','adjective'),
		(637,'หมอก','fog, mist','noun'),
		(638,'หมาก','areca nut','noun'), (638,'หมาก','areca palm','noun'),
		(638,'หมาก','betel chew','noun'), (638,'หมาก','game piece, marker','noun'),
		(664,'เนื้อ','meat, flesh','noun'), (664,'เนื้อ','gist, substance','noun'),
		(664,'เนื้อ','tissue','noun'), (664,'เนื้อ','deer','noun'),
		(688,'ใบ','leaf','noun'), (688,'ใบ','blade (of a fan)','noun'),
		(688,'ใบ','sail','noun'),
		(692,'ไหม้','burn','verb'), (692,'ไหม้','burned, charred','adjective'),
		(693,'ไหล','flow, run (of liquid)','verb'), (693,'ไหล','fresh-water eel','noun'),
		(711,'ใหม่','new, fresh','adjective'), (711,'ใหม่','anew, again','adverb'),
		(712,'ไม่','not','adverb'), (713,'นั่ง','sit','verb'),
		(713,'นั่ง','ride (sitting in a vehicle)','verb'),
		(714,'หนัง','skin, hide, leather','noun'), (714,'หนัง','movie, film','noun'),
		(716,'วาน','ask someone to do a favor','verb'),
		(717,'วัน','day','noun')
	) AS meaning(word_id,spelling,value,pos)
	LOOP
		IF NOT EXISTS (
			SELECT 1 FROM thai."Word" w JOIN thai."Spelling" s ON s.id=w.spelling_id
			WHERE w.id=r.word_id AND s.value=r.spelling
		) THEN RAISE EXCEPTION 'Unexpected target word % (%)',r.word_id,r.spelling; END IF;
		SELECT id INTO lid FROM thai."Lexeme"
		WHERE value=r.value AND (part_of_speech=r.pos OR part_of_speech IS NULL)
		ORDER BY id LIMIT 1;
		IF lid IS NULL THEN
			INSERT INTO thai."Lexeme"(value,part_of_speech)
			VALUES (r.value,r.pos) RETURNING id INTO lid;
		END IF;
		INSERT INTO thai."WordLexeme"(word,lexeme)
		SELECT r.word_id,lid WHERE NOT EXISTS (
			SELECT 1 FROM thai."WordLexeme" WHERE word=r.word_id AND lexeme=lid
		);
	END LOOP;

	FOR fk IN
		SELECT c.conrelid,c.confrelid,a.attname AS col,b.attname AS target_col
		FROM pg_constraint c
		JOIN pg_attribute a ON a.attrelid=c.conrelid AND a.attnum=c.conkey[1]
		JOIN pg_attribute b ON b.attrelid=c.confrelid AND b.attnum=c.confkey[1]
		WHERE c.contype='f' AND c.confrelid IN
			('thai."Word"'::regclass,'thai."Lexeme"'::regclass,'thai."Transcription"'::regclass)
	LOOP
		EXECUTE format(
			'SELECT count(*) FROM %s d WHERE d.%I IS NOT NULL AND NOT EXISTS (SELECT 1 FROM %s p WHERE p.%I=d.%I)',
			fk.conrelid::regclass,fk.col,fk.confrelid::regclass,fk.target_col,fk.col) INTO missing;
		IF missing<>0 THEN RAISE EXCEPTION 'Orphan references in %',fk.conrelid::regclass; END IF;
	END LOOP;
END $cleanup$;

REFRESH MATERIALIZED VIEW thai."WordView";
REFRESH MATERIALIZED VIEW thai."MinimalSetView";

DO $validate$
BEGIN
	IF EXISTS (
		SELECT m.set_id,s.value FROM thai."MinimalSetView" m
		JOIN thai."Word" w ON w.id=m.word_id JOIN thai."Spelling" s ON s.id=w.spelling_id
		GROUP BY m.set_id,s.value HAVING COUNT(*)>1
	) THEN RAISE EXCEPTION 'Repeated spellings remain in computed sets'; END IF;
	IF EXISTS (
		SELECT 1 FROM thai."MinimalSetView" m WHERE NOT EXISTS (
			SELECT 1 FROM thai."WordLexeme" wl JOIN thai."Lexeme" l ON l.id=wl.lexeme
			WHERE wl.word=m.word_id AND NULLIF(btrim(l.value),'') IS NOT NULL
		)
	) THEN RAISE EXCEPTION 'Computed-set word has no meaning'; END IF;
END $validate$;
