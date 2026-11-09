-- Run after V11; fixtures are temporary and use the installed view definition.
DO $test$
DECLARE
	view_sql text;
	actual_sets jsonb;
	expected_sets jsonb;
BEGIN
	SET LOCAL search_path = pg_catalog;
	CREATE TEMP TABLE minimal_test_word (
		id integer, transcription integer, is_primary boolean
	) ON COMMIT DROP;
	CREATE TEMP TABLE minimal_test_transcription (
		id integer, "IPA" text
	) ON COMMIT DROP;
	CREATE TEMP TABLE minimal_test_sound (
		id integer, value text, consonant_sound integer, initial_only boolean
	) ON COMMIT DROP;

	INSERT INTO minimal_test_sound VALUES
		(1, U&'k\02B0', 1, true),
		(2, U&'t\0255', 1, true),
		(3, 'm', 1, true),
		(4, 'k', 1, true),
		(5, U&'\0294', 1, true);

	INSERT INTO minimal_test_transcription VALUES
		(1, U&'k\02B0a\030C\02D0w'),
		(2, U&'k\02B0a\0300\02D0w'),
		(3, U&'k\02B0a\0302\02D0w'),
		(4, U&'k\02B0\01CEw'),
		(5, U&'k\02B0\00E0w'),
		(6, U&'k\02B0\00E2w'),
		(7, U&'k\02B0a\02D0w'),
		(10, U&'t\0361\0255a\0304\02D0t\031A'),
		(11, U&'t\0255a\02D0t'),
		(12, U&'t\0255a\0301\02D0t'),
		(20, U&'ma\02D0na\0301'),
		(21, U&'ma\0301na\02D0'),
		(30, U&'ma\02D0n'),
		(31, 'man'),
		(40, U&'ka\032F'),
		(41, 'ka'),
		(50, U&'\025B\02D0'),
		(51, U&'\0259\02D0'),
		(60, U&'ka\0301.ma'),
		(61, U&'ka ma\0300');

	INSERT INTO minimal_test_word
	SELECT id, id, true FROM minimal_test_transcription;

	view_sql := pg_get_viewdef('thai."MinimalSetView"'::regclass, true);
	view_sql := replace(view_sql, 'thai."Word"', 'pg_temp.minimal_test_word');
	view_sql := replace(view_sql, 'thai."Transcription"', 'pg_temp.minimal_test_transcription');
	view_sql := replace(view_sql, 'thai."Sound"', 'pg_temp.minimal_test_sound');
	EXECUTE 'CREATE TEMP VIEW minimal_test_sets AS ' || view_sql;

	SELECT jsonb_agg(members ORDER BY members) INTO actual_sets
	FROM (
		SELECT array_agg(word_id ORDER BY word_id) AS members
		FROM minimal_test_sets GROUP BY set_id
	) s;
	SELECT jsonb_agg(members ORDER BY members) INTO expected_sets
	FROM (VALUES
		(ARRAY[1,2,3,7]), (ARRAY[4,5,6]),
		(ARRAY[1,4]), (ARRAY[2,5]), (ARRAY[3,6]),
		(ARRAY[10,11,12]), (ARRAY[30,31]), (ARRAY[60,61])
	) s(members);

	IF actual_sets IS DISTINCT FROM expected_sets THEN
		RAISE EXCEPTION 'Minimal-set regression: expected %, got %', expected_sets, actual_sets;
	END IF;
	DROP VIEW minimal_test_sets;
END $test$;
