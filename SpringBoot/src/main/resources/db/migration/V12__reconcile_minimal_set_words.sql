-- Existing corrections are retained; databases without them receive the same data.
DO $reconcile$
DECLARE
	r record;
	fk record;
	sid integer;
	tid integer;
	wid integer;
	lid integer;
	missing bigint;
BEGIN
	FOR r IN SELECT * FROM (VALUES
		(435, 113, 'ขาว'), (438, 142, 'เขา')
	) AS mapping(old_id, kept_id, spelling)
	LOOP
		IF NOT EXISTS (
			SELECT 1 FROM thai."Word" w JOIN thai."Spelling" s ON s.id = w.spelling_id
			WHERE w.id = r.kept_id AND s.value = r.spelling
		) THEN
			RAISE EXCEPTION 'Expected retained word % (%) is missing', r.kept_id, r.spelling;
		END IF;
		IF EXISTS (SELECT 1 FROM thai."Word" WHERE id = r.old_id) THEN
			IF NOT EXISTS (
				SELECT 1 FROM thai."Word" w JOIN thai."Spelling" s ON s.id = w.spelling_id
				WHERE w.id = r.old_id AND s.value = r.spelling
			) THEN
				RAISE EXCEPTION 'Unexpected word at duplicate ID %', r.old_id;
			END IF;
			FOR fk IN
				SELECT c.conrelid, a.attname AS col
				FROM pg_constraint c
				JOIN pg_attribute a ON a.attrelid = c.conrelid AND a.attnum = c.conkey[1]
				WHERE c.contype = 'f' AND c.confrelid = 'thai."Word"'::regclass
			LOOP
				EXECUTE format('UPDATE %s SET %I = $1 WHERE %I = $2',
					fk.conrelid::regclass, fk.col, fk.col) USING r.kept_id, r.old_id;
			END LOOP;
			DELETE FROM thai."Word" WHERE id = r.old_id;
		END IF;
	END LOOP;

	DELETE FROM thai."Transcription" t
	WHERE t.id IN (514, 517)
	  AND NOT EXISTS (SELECT 1 FROM thai."Word" w WHERE w.transcription = t.id)
	  AND NOT EXISTS (SELECT 1 FROM thai."Consonant" c WHERE c.transcription = t.id);

	SELECT w.id INTO wid
	FROM thai."Word" w JOIN thai."Spelling" s ON s.id = w.spelling_id
	WHERE s.value = 'คาว' ORDER BY w.id LIMIT 1;
	IF wid IS NULL THEN
		SELECT id INTO sid FROM thai."Spelling" WHERE value = 'คาว' ORDER BY id LIMIT 1;
		IF sid IS NULL THEN
			INSERT INTO thai."Spelling"(value) VALUES ('คาว') RETURNING id INTO sid;
		END IF;
		INSERT INTO thai."Transcription"(paiboon, "IPA")
		VALUES ('kaao', U&'k\02B0a\02D0w') RETURNING id INTO tid;
		INSERT INTO thai."Word"(spelling_id, transcription, is_primary)
		VALUES (sid, tid, true) RETURNING id INTO wid;
	END IF;

	FOR r IN SELECT * FROM (VALUES
		('fishy', 'adjective'), ('smell of fish', 'noun'), ('meat dish', 'noun')
	) AS meanings(value, pos)
	LOOP
		SELECT id INTO lid FROM thai."Lexeme"
		WHERE value = r.value AND part_of_speech = r.pos ORDER BY id LIMIT 1;
		IF lid IS NULL THEN
			INSERT INTO thai."Lexeme"(value, part_of_speech)
			VALUES (r.value, r.pos) RETURNING id INTO lid;
		END IF;
		INSERT INTO thai."WordLexeme"(word, lexeme)
		SELECT wid, lid WHERE NOT EXISTS (
			SELECT 1 FROM thai."WordLexeme" WHERE word = wid AND lexeme = lid
		);
	END LOOP;
	IF NOT EXISTS (SELECT 1 FROM thai."MinimalSet" WHERE id = 1 AND label = U&'k\02B0') THEN
		RAISE EXCEPTION 'Expected hand-maintained kʰ set is missing';
	END IF;
	INSERT INTO thai."MinimalSetWord"(id, set_id, word_id)
	SELECT COALESCE(MAX(id), 0) + 1, 1, wid FROM thai."MinimalSetWord"
	HAVING NOT EXISTS (SELECT 1 FROM thai."MinimalSetWord" WHERE set_id = 1 AND word_id = wid);

	UPDATE thai."Transcription" SET "IPA" = normalize(btrim("IPA"), NFD)
	WHERE "IPA" IS DISTINCT FROM normalize(btrim("IPA"), NFD);
	UPDATE thai."Transcription" SET "IPA" = U&'k\02B0a\030C\02D0w'
	WHERE id = 194 AND "IPA" = U&'k\02B0a\02D0w';
	UPDATE thai."Transcription" SET "IPA" = U&'p\02B0\0264\0302\014B', paiboon = U&'p\0259\0302ng'
	WHERE id = 500 AND "IPA" = normalize('pə̂əng', NFD) AND paiboon = 'pə̂əng';
	DELETE FROM thai."WordLexeme" wl USING thai."Lexeme" l
	WHERE wl.word = 113 AND wl.lexeme = l.id AND l.value = 'coop, sty';

	FOR fk IN
		SELECT c.conrelid, a.attname AS col
		FROM pg_constraint c
		JOIN pg_attribute a ON a.attrelid = c.conrelid AND a.attnum = c.conkey[1]
		WHERE c.contype = 'f' AND c.confrelid = 'thai."Word"'::regclass
	LOOP
		EXECUTE format(
			'SELECT count(*) FROM %s d WHERE d.%I IS NOT NULL AND NOT EXISTS (SELECT 1 FROM thai."Word" w WHERE w.id = d.%I)',
			fk.conrelid::regclass, fk.col, fk.col) INTO missing;
		IF missing <> 0 THEN
			RAISE EXCEPTION 'Orphan word references in %', fk.conrelid::regclass;
		END IF;
	END LOOP;
END $reconcile$;

REFRESH MATERIALIZED VIEW thai."WordView";
REFRESH MATERIALIZED VIEW thai."MinimalSetView";
