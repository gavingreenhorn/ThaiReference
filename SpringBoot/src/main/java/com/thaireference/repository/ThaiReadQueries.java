package com.thaireference.repository;

/*
 * Word projections select straight from WordView (the flat, pre-joined materialized view):
 * one row per word carrying spelling, the three transcription columns and a lexemes text[].
 * That replaces the old Word + Spelling + Transcription + WordLexeme + Lexeme join with
 * array_agg + GROUP BY that every feature query used to repeat.
 *
 * WordDto(id, spelling, TranscriptionDto, String[]) maps wv.lexemes directly; its String[]
 * constructor normalises the array to a list.
 */
final class ThaiReadQueries {

    private ThaiReadQueries() {}

    static final String NUMERALS =
            """
            SELECT new com.thaireference.dto.NumeralDto(
                n.grapheme,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM Numeral n
            JOIN WordView wv ON wv.id = n.wordId
            ORDER BY n.id
            """;

    static final String WEEKDAYS =
            """
            SELECT new com.thaireference.dto.DayOfWeekDto(
                d.ordinal,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes),
                d.deity,
                d.imageUrl)
            FROM DayOfWeek d
            JOIN WordView wv ON wv.id = d.wordId
            ORDER BY d.ordinal
            """;

    static final String TIME_OF_DAY =
            """
            SELECT new com.thaireference.dto.TimeOfDayDto(
                td.ordinal,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM TimeOfDay td
            JOIN WordView wv ON wv.id = td.wordId
            ORDER BY td.ordinal
            """;

    static final String HOURS =
            """
            SELECT new com.thaireference.dto.HourDto(
                h.ordinal,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM Hour h
            JOIN WordView wv ON wv.id = h.wordId
            ORDER BY h.ordinal
            """;

    static final String MONTHS =
            """
            SELECT new com.thaireference.repository.MonthRow(
                m.ordinal,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes),
                m.shortName,
                m.englishName,
                new com.thaireference.dto.ZodiacDto(
                    z.ordinal,
                    z.englishName,
                    z.sanskritName,
                    z.sanskritLexeme,
                    z.element,
                    z.imageUrl),
                s.id,
                s.ordinal,
                s.englishLabel,
                s.glyph)
            FROM Month m
            JOIN WordView wv ON wv.id = m.wordId
            JOIN Zodiac z ON z.id = m.zodiacId
            JOIN Season s ON s.id = m.seasonId
            ORDER BY m.ordinal
            """;

    static final String MONTH_SEASON_WORDS =
            """
            SELECT new com.thaireference.repository.WordRow(
                m.ordinal,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM Month m
            JOIN Season s ON s.id = m.seasonId
            JOIN CompositeName cn ON cn.id = s.compositeNameId
            JOIN WordView wv ON wv.id IN (cn.word1Id, cn.word2Id, cn.word3Id, cn.word4Id, cn.word5Id)
            ORDER BY m.ordinal,
                CASE
                    WHEN wv.id = cn.word1Id THEN 1
                    WHEN wv.id = cn.word2Id THEN 2
                    WHEN wv.id = cn.word3Id THEN 3
                    WHEN wv.id = cn.word4Id THEN 4
                    WHEN wv.id = cn.word5Id THEN 5
                    ELSE 6 END
            """;

    static final String CELESTIAL_BODIES =
            """
            SELECT new com.thaireference.dto.CelestialBodyDto(
                cb.ordinal,
                cb.glyph,
                cb.colorHex,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM CelestialBody cb
            JOIN WordView wv ON wv.id = cb.wordId
            ORDER BY cb.ordinal
            """;

    static final String CONSONANTS =
            """
            SELECT new com.thaireference.repository.ConsonantRow(
                c.id,
                c.grapheme,
                c.phoneme,
                tc.code,
                e.name,
                new com.thaireference.dto.TranscriptionDto(ct.paiboon, ct.ipa),
                new com.thaireference.dto.SoundDto(
                    COALESCE(sInit.id, 0),
                    COALESCE(sInit.value, ''),
                    COALESCE(stInit.name, ''),
                    CASE WHEN sInit.consonantSound IS NOT NULL THEN true ELSE false END,
                    COALESCE(sInit.initialOnly, false),
                    sInit.mediapath),
                new com.thaireference.dto.SoundDto(
                    COALESCE(sFinal.id, 0),
                    COALESCE(sFinal.value, ''),
                    COALESCE(stFinal.name, ''),
                    CASE WHEN sFinal.consonantSound IS NOT NULL THEN true ELSE false END,
                    COALESCE(sFinal.initialOnly, false),
                    sFinal.mediapath),
                c.mediapath)
            FROM Consonant c
            JOIN ThaiClassEntity tc ON tc.id = c.classId
            JOIN Ending e ON e.id = c.endingId
            LEFT JOIN Transcription ct ON ct.id = c.transcriptionId
            LEFT JOIN Sound sInit ON sInit.id = c.initialId
            LEFT JOIN SoundType stInit ON stInit.id = sInit.soundTypeId
            LEFT JOIN Sound sFinal ON sFinal.id = c.finalSoundId
            LEFT JOIN SoundType stFinal ON stFinal.id = sFinal.soundTypeId
            ORDER BY c.id
            """;

    static final String CONSONANT_SAMPLE_WORDS =
            """
            SELECT new com.thaireference.repository.WordRow(
                cs.consonantId,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM ConsonantSample cs
            JOIN WordView wv ON wv.id = cs.wordId
            ORDER BY cs.consonantId, cs.id
            """;

    static final String VOWELS =
            """
            SELECT new com.thaireference.repository.VowelRow(
                v.id,
                COALESCE(v.isDiphtong, false),
                new com.thaireference.dto.SoundDto(
                    vs.id,
                    vs.value,
                    COALESCE(vst.name, ''),
                    CASE WHEN vs.consonantSound IS NOT NULL THEN true ELSE false END,
                    vs.initialOnly,
                    vs.mediapath),
                v.mediapath)
            FROM Vowel v
            JOIN Sound vs ON vs.id = v.soundId
            LEFT JOIN SoundType vst ON vst.id = vs.soundTypeId
            ORDER BY v.id
            """;

    static final String VOWEL_GRAPHEMES =
            """
            SELECT new com.thaireference.repository.GraphemeRow(
                vg.vowelId,
                new com.thaireference.dto.VowelGraphemeDto(
                    COALESCE(vg.value, ''),
                    COALESCE(vg.closed, false)))
            FROM VowelGrapheme vg
            ORDER BY vg.vowelId, vg.id
            """;

    static final String VOWEL_SAMPLE_WORDS =
            """
            SELECT new com.thaireference.repository.WordRow(
                vsmp.vowelId,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM VowelSample vsmp
            JOIN WordView wv ON wv.id = vsmp.wordId
            ORDER BY vsmp.vowelId, vsmp.id
            """;

    static final String PRONOUNS =
            """
            SELECT new com.thaireference.repository.PronounRow(
                p.id,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes),
                COALESCE(p.grammaticalPerson, 0),
                CASE
                    WHEN LOWER(pl.name) LIKE '%singular%' THEN 1
                    WHEN LOWER(pl.name) LIKE '%plural%' THEN 2
                    ELSE 3
                END,
                COALESCE(f.name, ''),
                COALESCE(g.name, ''))
            FROM Pronoun p
            JOIN WordView wv ON wv.id = p.wordId
            LEFT JOIN Plurality pl ON pl.id = p.pluralityId
            LEFT JOIN Formality f ON f.id = p.formalityId
            LEFT JOIN Gender g ON g.id = p.genderId
            ORDER BY p.id
            """;

    static final String PRONOUN_TYPES =
            """
            SELECT new com.thaireference.repository.PronounTypeRow(
                ptb.pronounId,
                pt.name)
            FROM PronounTypeBreakout ptb
            JOIN PronounType pt ON pt.id = ptb.typeId
            WHERE pt.name IS NOT NULL
            ORDER BY ptb.pronounId, ptb.id
            """;

    static final String GRID_POSITIONS =
            """
            SELECT new com.thaireference.repository.GridPositionRow(
                pos.id,
                pos.rowIndex,
                pos.colIndex,
                pos.tileStyle,
                pos.name)
            FROM GridPosition pos
            ORDER BY pos.rowIndex, pos.colIndex
            """;

    static final String GRID_POSITION_WORDS =
            """
            SELECT new com.thaireference.repository.WordRow(
                pw.gridPositionId,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM GridPositionWord pw
            JOIN WordView wv ON wv.id = pw.wordId
            ORDER BY pw.gridPositionId, pw.wordOrder
            """;

    static final String CLUSTERS =
            """
            SELECT new com.thaireference.dto.ClusterDto(
                cl.id,
                cl.clusterType,
                cl.subtype,
                cl.value)
            FROM Cluster cl
            ORDER BY cl.clusterType, cl.subtype, cl.value, cl.id
            """;

    static final String CLUSTER_WORDS =
            """
            SELECT new com.thaireference.repository.WordRow(
                cw.clusterId,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM ClusterWord cw
            JOIN WordView wv ON wv.id = cw.wordId
            ORDER BY cw.clusterId, cw.id
            """;

    static final String MINIMAL_SETS =
            """
            SELECT m.set_id, m.label
            FROM thai."MinimalSetView" m
            WHERE (CAST(:label AS text) IS NULL OR m.label = CAST(:label AS text))
            GROUP BY m.set_id, m.label
            ORDER BY m.set_id
            OFFSET :offset LIMIT :limit
            """;

    static final String MINIMAL_SET_COUNT =
            """
            SELECT COUNT(*)
            FROM (
                SELECT m.set_id
                FROM thai."MinimalSetView" m
                WHERE (CAST(:label AS text) IS NULL OR m.label = CAST(:label AS text))
                GROUP BY m.set_id
            ) s
            """;

    static final String MINIMAL_SET_LABELS =
            """
            SELECT DISTINCT m.label
            FROM thai."MinimalSetView" m
            WHERE m.label IS NOT NULL AND m.label <> ''
            ORDER BY m.label
            """;

    static final String MINIMAL_SET_WORDS_PAGE =
            """
            WITH paged_sets AS (
                SELECT m.set_id
                FROM thai."MinimalSetView" m
                WHERE (CAST(:label AS text) IS NULL OR m.label = CAST(:label AS text))
                GROUP BY m.set_id
                ORDER BY m.set_id
                OFFSET :offset LIMIT :limit
            )
            SELECT m.set_id, wv.id, wv.spelling, wv.paiboon, wv.ipa, wv.lexemes
            FROM thai."MinimalSetView" m
            JOIN paged_sets ps ON ps.set_id = m.set_id
            JOIN thai."WordView" wv ON wv.id = m.word_id
            ORDER BY m.set_id, m.word_id
            """;

    static final String HOMOPHONES =
            """
            SELECT new com.thaireference.repository.HomophoneRow(
                t.id,
                new com.thaireference.dto.TranscriptionDto(t.paiboon, t.ipa))
            FROM Transcription t
            JOIN Word w ON w.transcriptionId = t.id
                AND (SELECT COUNT(w2) FROM Word w2 WHERE w2.spellingId = w.spellingId) = 1
            GROUP BY t.id, t.paiboon, t.ipa
            HAVING COUNT(w) >= 2
            ORDER BY t.id
            """;

    static final String HOMOPHONE_WORDS =
            """
            SELECT new com.thaireference.repository.WordRow(
                wv.transcriptionId,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM WordView wv
            WHERE (SELECT COUNT(w2) FROM Word w2 WHERE w2.spellingId = wv.spellingId) = 1
              AND wv.transcriptionId IN (
                  SELECT w3.transcriptionId FROM Word w3
                  WHERE (SELECT COUNT(w4) FROM Word w4 WHERE w4.spellingId = w3.spellingId) = 1
                  GROUP BY w3.transcriptionId
                  HAVING COUNT(w3) >= 2)
            ORDER BY wv.transcriptionId, wv.id
            """;

    static final String MARK_SPECIAL_SYMBOLS =
            """
            SELECT new com.thaireference.repository.MarkRow(
                ss.id,
                COALESCE(ss.grapheme, ''),
                false,
                null)
            FROM SpecialSymbol ss
            ORDER BY ss.id
            """;

    static final String MARK_SPECIAL_SYMBOL_WORDS =
            """
            SELECT new com.thaireference.repository.WordRow(
                ss.id,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM SpecialSymbol ss
            JOIN WordView wv ON wv.id = ss.wordId
            WHERE ss.wordId IS NOT NULL
            ORDER BY ss.id
            """;

    static final String CLASSES =
            """
            SELECT new com.thaireference.repository.ClassRow(
                tc.id,
                COALESCE(tc.code, ''),
                tc.englishLabel)
            FROM ThaiClassEntity tc
            ORDER BY tc.id
            """;

    static final String CLASS_WORDS =
            """
            SELECT new com.thaireference.repository.WordRow(
                tc.id,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM ThaiClassEntity tc
            JOIN CompositeName cn ON cn.id = tc.nameId
            JOIN WordView wv ON wv.id IN (cn.word1Id, cn.word2Id, cn.word3Id, cn.word4Id, cn.word5Id)
            ORDER BY tc.id,
                CASE
                    WHEN wv.id = cn.word1Id THEN 1
                    WHEN wv.id = cn.word2Id THEN 2
                    WHEN wv.id = cn.word3Id THEN 3
                    WHEN wv.id = cn.word4Id THEN 4
                    WHEN wv.id = cn.word5Id THEN 5
                    ELSE 6 END
            """;

    static final String MARK_TONE_MARKS =
            """
            SELECT new com.thaireference.repository.MarkRow(
                tm.id,
                COALESCE(tm.mark, ''),
                true,
                tm.englishLabel)
            FROM ToneMark tm
            ORDER BY tm.id
            """;

    static final String MARK_TONE_MARK_WORDS =
            """
            SELECT new com.thaireference.repository.WordRow(
                tm.id,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM ToneMark tm
            JOIN CompositeName cn ON cn.id = tm.nameId
            JOIN WordView wv ON wv.id IN (cn.word1Id, cn.word2Id, cn.word3Id, cn.word4Id, cn.word5Id)
            ORDER BY tm.id,
                CASE
                    WHEN wv.id = cn.word1Id THEN 1
                    WHEN wv.id = cn.word2Id THEN 2
                    WHEN wv.id = cn.word3Id THEN 3
                    WHEN wv.id = cn.word4Id THEN 4
                    WHEN wv.id = cn.word5Id THEN 5
                    ELSE 6 END
            """;

    static final String DIRECTIONS =
            """
            SELECT new com.thaireference.repository.DirectionRow(
                d.id,
                d.label,
                d.lexeme)
            FROM Direction d
            ORDER BY d.id
            """;

    static final String DIRECTION_WORDS =
            """
            SELECT new com.thaireference.repository.WordRow(
                d.id,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM Direction d
            JOIN CompositeName cn ON cn.id = d.compositeNameId
            JOIN WordView wv ON wv.id IN (cn.word1Id, cn.word2Id, cn.word3Id, cn.word4Id, cn.word5Id)
            ORDER BY d.id,
                CASE
                    WHEN wv.id = cn.word1Id THEN 1
                    WHEN wv.id = cn.word2Id THEN 2
                    WHEN wv.id = cn.word3Id THEN 3
                    WHEN wv.id = cn.word4Id THEN 4
                    WHEN wv.id = cn.word5Id THEN 5
                    ELSE 6 END
            """;

    static final String POSITIONS =
            """
            SELECT new com.thaireference.repository.PositionHeadRow(
                p.id,
                p.lexeme,
                s.id,
                s.translation)
            FROM Position p
            JOIN Sentence s ON s.id = p.sentenceId
            ORDER BY p.id
            """;

    static final String POSITION_WORDS =
            """
            SELECT new com.thaireference.repository.WordRow(
                p.id,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM Position p
            JOIN CompositeName cn ON cn.id = p.compositeNameId
            JOIN WordView wv ON wv.id IN (cn.word1Id, cn.word2Id, cn.word3Id, cn.word4Id, cn.word5Id)
            ORDER BY p.id,
                CASE
                    WHEN wv.id = cn.word1Id THEN 1
                    WHEN wv.id = cn.word2Id THEN 2
                    WHEN wv.id = cn.word3Id THEN 3
                    WHEN wv.id = cn.word4Id THEN 4
                    WHEN wv.id = cn.word5Id THEN 5
                    ELSE 6 END
            """;

    static final String SENTENCE_WORDS =
            """
            SELECT new com.thaireference.repository.WordRow(
                sw.sentenceId,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM SentenceWord sw
            JOIN WordView wv ON wv.id = sw.wordId
            ORDER BY sw.sentenceId, sw.wordOrder
            """;

    static final String HUE_FAMILIES =
            """
            SELECT new com.thaireference.dto.HueFamilyDto(
                hf.id,
                hf.thaiName,
                hf.englishName,
                hf.hexRepresentative,
                CAST(hf.displayOrder AS int))
            FROM HueFamily hf
            ORDER BY hf.displayOrder
            """;

    static final String COLORS =
            """
            SELECT new com.thaireference.repository.ColorRow(
                c.id,
                c.hueFamilyId,
                c.compositeNameId,
                c.hex,
                c.englishLabel,
                CAST(c.luminanceOrder AS int))
            FROM Color c
            ORDER BY c.hueFamilyId, c.luminanceOrder
            """;

    static final String COLOR_WORDS =
            """
            SELECT new com.thaireference.repository.WordRow(
                c.id,
                new com.thaireference.dto.WordDto(
                    wv.id,
                    wv.spelling,
                    new com.thaireference.dto.TranscriptionDto(wv.paiboon, wv.ipa),
                    wv.lexemes))
            FROM Color c
            JOIN CompositeName cn ON cn.id = c.compositeNameId
            JOIN WordView wv ON wv.id IN (cn.word1Id, cn.word2Id, cn.word3Id, cn.word4Id, cn.word5Id)
            ORDER BY c.id,
                CASE
                    WHEN wv.id = cn.word1Id THEN 1
                    WHEN wv.id = cn.word2Id THEN 2
                    WHEN wv.id = cn.word3Id THEN 3
                    WHEN wv.id = cn.word4Id THEN 4
                    WHEN wv.id = cn.word5Id THEN 5
                    ELSE 6 END
            """;
}
