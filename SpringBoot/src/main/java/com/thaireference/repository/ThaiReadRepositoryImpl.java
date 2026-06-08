package com.thaireference.repository;

import com.thaireference.dto.*;
import jakarta.persistence.EntityManager;
import jakarta.persistence.PersistenceContext;
import java.util.ArrayList;
import java.util.List;
import org.springframework.stereotype.Repository;
import org.springframework.transaction.annotation.Transactional;

@Repository
@Transactional(readOnly = true)
public class ThaiReadRepositoryImpl implements ThaiReadRepositoryCustom {

    @PersistenceContext
    private EntityManager entityManager;

    @Override
    public List<NumeralDto> findNumerals() {
        return entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.dto.NumeralDto(
                            n.grapheme,
                            new com.thaireference.dto.WordDto(
                                w.id,
                                s.value,
                                new com.thaireference.dto.TranscriptionDto(t.phonemic, t.paiboon, t.ipa),
                                multiset(
                                    SELECT l.value
                                    FROM WordLexeme wl
                                    JOIN Lexeme l ON l.id = wl.lexemeId
                                    WHERE wl.wordId = w.id)))
                        FROM Numeral n
                        JOIN Word w ON w.id = n.wordId
                        JOIN Spelling s ON s.id = w.spellingId
                        LEFT JOIN Transcription t ON t.id = w.transcriptionId
                        ORDER BY n.id
                        """,
                        NumeralDto.class)
                .getResultList();
    }

    @Override
    public List<DayOfWeekDto> findWeekdays() {
        return entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.dto.DayOfWeekDto(
                            d.ordinal,
                            new com.thaireference.dto.WordDto(
                                w.id,
                                s.value,
                                new com.thaireference.dto.TranscriptionDto(t.phonemic, t.paiboon, t.ipa),
                                multiset(
                                    SELECT l.value
                                    FROM WordLexeme wl
                                    JOIN Lexeme l ON l.id = wl.lexemeId
                                    WHERE wl.wordId = w.id)),
                            d.deity,
                            d.imageUrl)
                        FROM DayOfWeek d
                        JOIN Word w ON w.id = d.wordId
                        JOIN Spelling s ON s.id = w.spellingId
                        LEFT JOIN Transcription t ON t.id = w.transcriptionId
                        ORDER BY d.ordinal
                        """,
                        DayOfWeekDto.class)
                .getResultList();
    }

    @Override
    public List<TimeOfDayDto> findTimeOfDay() {
        return entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.dto.TimeOfDayDto(
                            td.ordinal,
                            new com.thaireference.dto.WordDto(
                                w.id,
                                s.value,
                                new com.thaireference.dto.TranscriptionDto(t.phonemic, t.paiboon, t.ipa),
                                multiset(
                                    SELECT l.value
                                    FROM WordLexeme wl
                                    JOIN Lexeme l ON l.id = wl.lexemeId
                                    WHERE wl.wordId = w.id)))
                        FROM TimeOfDay td
                        JOIN Word w ON w.id = td.wordId
                        JOIN Spelling s ON s.id = w.spellingId
                        LEFT JOIN Transcription t ON t.id = w.transcriptionId
                        ORDER BY td.ordinal
                        """,
                        TimeOfDayDto.class)
                .getResultList();
    }

    @Override
    public List<HourDto> findHours() {
        return entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.dto.HourDto(
                            h.ordinal,
                            new com.thaireference.dto.WordDto(
                                w.id,
                                s.value,
                                new com.thaireference.dto.TranscriptionDto(t.phonemic, t.paiboon, t.ipa),
                                multiset(
                                    SELECT l.value
                                    FROM WordLexeme wl
                                    JOIN Lexeme l ON l.id = wl.lexemeId
                                    WHERE wl.wordId = w.id)))
                        FROM Hour h
                        JOIN Word w ON w.id = h.wordId
                        JOIN Spelling s ON s.id = w.spellingId
                        LEFT JOIN Transcription t ON t.id = w.transcriptionId
                        ORDER BY h.ordinal
                        """,
                        HourDto.class)
                .getResultList();
    }

    @Override
    public List<MonthDto> findMonths() {
        return entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.dto.MonthDto(
                            m.ordinal,
                            new com.thaireference.dto.WordDto(
                                w.id,
                                s.value,
                                new com.thaireference.dto.TranscriptionDto(t.phonemic, t.paiboon, t.ipa),
                                multiset(
                                    SELECT l.value
                                    FROM WordLexeme wl
                                    JOIN Lexeme l ON l.id = wl.lexemeId
                                    WHERE wl.wordId = w.id)),
                            m.shortName,
                            m.englishName,
                            new com.thaireference.dto.ZodiacDto(
                                z.ordinal,
                                z.englishName,
                                z.sanskritName,
                                z.sanskritLexeme,
                                z.element,
                                z.imageUrl))
                        FROM Month m
                        JOIN Word w ON w.id = m.wordId
                        JOIN Spelling s ON s.id = w.spellingId
                        LEFT JOIN Transcription t ON t.id = w.transcriptionId
                        JOIN Zodiac z ON z.id = m.zodiacId
                        ORDER BY m.ordinal
                        """,
                        MonthDto.class)
                .getResultList();
    }

    @Override
    public List<CelestialBodyDto> findCelestialBodies() {
        return entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.dto.CelestialBodyDto(
                            cb.ordinal,
                            cb.glyph,
                            cb.colorHex,
                            new com.thaireference.dto.WordDto(
                                w.id,
                                s.value,
                                new com.thaireference.dto.TranscriptionDto(t.phonemic, t.paiboon, t.ipa),
                                multiset(
                                    SELECT l.value
                                    FROM WordLexeme wl
                                    JOIN Lexeme l ON l.id = wl.lexemeId
                                    WHERE wl.wordId = w.id)))
                        FROM CelestialBody cb
                        JOIN Word w ON w.id = cb.wordId
                        JOIN Spelling s ON s.id = w.spellingId
                        LEFT JOIN Transcription t ON t.id = w.transcriptionId
                        ORDER BY cb.ordinal
                        """,
                        CelestialBodyDto.class)
                .getResultList();
    }

    @Override
    public List<ConsonantDto> findConsonants() {
        return entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.dto.ConsonantDto(
                            c.grapheme,
                            c.phoneme,
                            COALESCE(tc.name, c.classRaw),
                            COALESCE(e.name, c.ending),
                            new com.thaireference.dto.TranscriptionDto(ct.phonemic, ct.paiboon, ct.ipa),
                            new com.thaireference.dto.SoundDto(
                                COALESCE(sInit.id, 0),
                                COALESCE(sInit.value, ''),
                                COALESCE(stInit.name, ''),
                                CASE WHEN sInit.consonantSound IS NOT NULL THEN true ELSE false END,
                                COALESCE(sInit.initialOnly, false),
                                sInit.mediapath),
                            new com.thaireference.dto.SoundDto(
                                sFinal.id,
                                sFinal.value,
                                COALESCE(stFinal.name, ''),
                                CASE WHEN sFinal.consonantSound IS NOT NULL THEN true ELSE false END,
                                COALESCE(sFinal.initialOnly, false),
                                sFinal.mediapath),
                            c.mediapath,
                            multiset(
                                SELECT new com.thaireference.dto.WordDto(
                                    w.id,
                                    sp.value,
                                    new com.thaireference.dto.TranscriptionDto(tr.phonemic, tr.paiboon, tr.ipa),
                                    multiset(
                                        SELECT l.value
                                        FROM WordLexeme wl
                                        JOIN Lexeme l ON l.id = wl.lexemeId
                                        WHERE wl.wordId = w.id))
                                FROM ConsonantSample cs
                                JOIN Word w ON w.id = cs.wordId
                                JOIN Spelling sp ON sp.id = w.spellingId
                                LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                                WHERE cs.consonantId = c.id
                                ORDER BY cs.id))
                        FROM Consonant c
                        LEFT JOIN Transcription ct ON ct.id = c.transcriptionId
                        LEFT JOIN ThaiClassEntity tc ON tc.id = CAST(c.classRaw AS integer)
                        LEFT JOIN Ending e ON e.id = CAST(c.ending AS integer)
                        LEFT JOIN Sound sInit ON sInit.id = c.initialId
                        LEFT JOIN SoundType stInit ON stInit.id = sInit.soundTypeId
                        LEFT JOIN Sound sFinal ON sFinal.id = c.finalSoundId
                        LEFT JOIN SoundType stFinal ON stFinal.id = sFinal.soundTypeId
                        ORDER BY c.id
                        """,
                        ConsonantDto.class)
                .getResultList();
    }

    @Override
    public List<VowelDto> findVowels() {
        return entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.dto.VowelDto(
                            COALESCE(v.isDiphtong, false),
                            new com.thaireference.dto.SoundDto(
                                vs.id,
                                vs.value,
                                COALESCE(vst.name, ''),
                                CASE WHEN vs.consonantSound IS NOT NULL THEN true ELSE false END,
                                vs.initialOnly,
                                vs.mediapath),
                            multiset(
                                SELECT new com.thaireference.dto.VowelGraphemeDto(
                                    COALESCE(vg.value, ''),
                                    COALESCE(vg.closed, false))
                                FROM VowelGrapheme vg
                                WHERE vg.vowelId = v.id
                                ORDER BY vg.id),
                            v.mediapath,
                            multiset(
                                SELECT new com.thaireference.dto.WordDto(
                                    w.id,
                                    sp.value,
                                    new com.thaireference.dto.TranscriptionDto(tr.phonemic, tr.paiboon, tr.ipa),
                                    multiset(
                                        SELECT l.value
                                        FROM WordLexeme wl
                                        JOIN Lexeme l ON l.id = wl.lexemeId
                                        WHERE wl.wordId = w.id))
                                FROM VowelSample vsmp
                                JOIN Word w ON w.id = vsmp.wordId
                                JOIN Spelling sp ON sp.id = w.spellingId
                                LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                                WHERE vsmp.vowelId = v.id
                                ORDER BY vsmp.id))
                        FROM Vowel v
                        JOIN Sound vs ON vs.id = v.soundId
                        LEFT JOIN SoundType vst ON vst.id = vs.soundTypeId
                        ORDER BY v.id
                        """,
                        VowelDto.class)
                .getResultList();
    }

    @Override
    public List<MarkDto> findMarks() {
        List<MarkDto> marks = new ArrayList<>();
        marks.addAll(entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.dto.MarkDto(
                            COALESCE(ss.grapheme, ''),
                            false,
                            multiset(
                                SELECT new com.thaireference.dto.WordDto(
                                    w.id,
                                    sp.value,
                                    new com.thaireference.dto.TranscriptionDto(tr.phonemic, tr.paiboon, tr.ipa),
                                    multiset(
                                        SELECT l.value
                                        FROM WordLexeme wl
                                        JOIN Lexeme l ON l.id = wl.lexemeId
                                        WHERE wl.wordId = w.id))
                                FROM Word w
                                JOIN Spelling sp ON sp.id = w.spellingId
                                LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                                WHERE w.id = ss.wordId))
                        FROM SpecialSymbol ss
                        ORDER BY ss.id
                        """,
                        MarkDto.class)
                .getResultList());
        marks.addAll(entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.dto.MarkDto(
                            COALESCE(tm.mark, ''),
                            true,
                            multiset(
                                SELECT new com.thaireference.dto.WordDto(
                                    w.id,
                                    sp.value,
                                    new com.thaireference.dto.TranscriptionDto(tr.phonemic, tr.paiboon, tr.ipa),
                                    multiset(
                                        SELECT l.value
                                        FROM WordLexeme wl
                                        JOIN Lexeme l ON l.id = wl.lexemeId
                                        WHERE wl.wordId = w.id))
                                FROM CompositeName cn
                                JOIN Word w ON w.id IN (cn.word1Id, cn.word2Id, cn.word3Id, cn.word4Id, cn.word5Id)
                                JOIN Spelling sp ON sp.id = w.spellingId
                                LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                                WHERE cn.id = tm.nameId
                                ORDER BY CASE
                                    WHEN w.id = cn.word1Id THEN 1
                                    WHEN w.id = cn.word2Id THEN 2
                                    WHEN w.id = cn.word3Id THEN 3
                                    WHEN w.id = cn.word4Id THEN 4
                                    WHEN w.id = cn.word5Id THEN 5
                                    ELSE 6 END),
                            tm.englishLabel)
                        FROM ToneMark tm
                        ORDER BY tm.id
                        """,
                        MarkDto.class)
                .getResultList());
        return marks;
    }

    @Override
    public List<PronounDto> findPronouns() {
        return entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.dto.PronounDto(
                            new com.thaireference.dto.WordDto(
                                w.id,
                                sp.value,
                                new com.thaireference.dto.TranscriptionDto(tr.phonemic, tr.paiboon, tr.ipa),
                                multiset(
                                    SELECT l.value
                                    FROM WordLexeme wl
                                    JOIN Lexeme l ON l.id = wl.lexemeId
                                    WHERE wl.wordId = w.id)),
                            COALESCE(p.grammaticalPerson, 0),
                            CASE
                                WHEN LOWER(pl.name) LIKE '%singular%' THEN 1
                                WHEN LOWER(pl.name) LIKE '%plural%' THEN 2
                                ELSE 3
                            END,
                            COALESCE(f.name, ''),
                            COALESCE(g.name, ''),
                            multiset(
                                SELECT pt.name
                                FROM PronounTypeBreakout ptb
                                JOIN PronounType pt ON pt.id = ptb.typeId
                                WHERE ptb.pronounId = p.id AND pt.name IS NOT NULL
                                ORDER BY ptb.id))
                        FROM Pronoun p
                        JOIN Word w ON w.id = p.wordId
                        JOIN Spelling sp ON sp.id = w.spellingId
                        LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                        LEFT JOIN Plurality pl ON pl.id = p.pluralityId
                        LEFT JOIN Formality f ON f.id = p.formalityId
                        LEFT JOIN Gender g ON g.id = p.genderId
                        ORDER BY p.id
                        """,
                        PronounDto.class)
                .getResultList();
    }

    @Override
    public List<PositionCellDto> findPositions() {
        return entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.dto.PositionCellDto(
                            pos.id,
                            pos.rowIndex,
                            pos.colIndex,
                            pos.tileStyle,
                            pos.name,
                            multiset(
                                SELECT new com.thaireference.dto.WordDto(
                                    w.id,
                                    sp.value,
                                    new com.thaireference.dto.TranscriptionDto(tr.phonemic, tr.paiboon, tr.ipa),
                                    multiset(
                                        SELECT l.value
                                        FROM WordLexeme wl
                                        JOIN Lexeme l ON l.id = wl.lexemeId
                                        WHERE wl.wordId = w.id))
                                FROM PositionWord pw
                                JOIN Word w ON w.id = pw.wordId
                                JOIN Spelling sp ON sp.id = w.spellingId
                                LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                                WHERE pw.positionId = pos.id
                                ORDER BY pw.wordOrder))
                        FROM Position pos
                        ORDER BY pos.rowIndex, pos.colIndex
                        """,
                        PositionCellDto.class)
                .getResultList();
    }

    @Override
    public List<ClusterDto> findClusters() {
        return entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.dto.ClusterDto(
                            cl.id,
                            cl.clusterType,
                            cl.subtype,
                            cl.value,
                            multiset(
                                SELECT new com.thaireference.dto.WordDto(
                                    w.id,
                                    sp.value,
                                    new com.thaireference.dto.TranscriptionDto(tr.phonemic, tr.paiboon, tr.ipa),
                                    multiset(
                                        SELECT l.value
                                        FROM WordLexeme wl
                                        JOIN Lexeme l ON l.id = wl.lexemeId
                                        WHERE wl.wordId = w.id))
                                FROM ClusterWord cw
                                JOIN Word w ON w.id = cw.wordId
                                JOIN Spelling sp ON sp.id = w.spellingId
                                LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                                WHERE cw.clusterId = cl.id
                                ORDER BY cw.id))
                        FROM Cluster cl
                        ORDER BY cl.clusterType, cl.subtype, cl.value, cl.id
                        """,
                        ClusterDto.class)
                .getResultList();
    }

    @Override
    public HomonymsDto findHomonyms() {
        List<HomographGroupDto> homographs = entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.dto.HomographGroupDto(
                            sp.value,
                            multiset(
                                SELECT new com.thaireference.dto.WordDto(
                                    w.id,
                                    sp.value,
                                    new com.thaireference.dto.TranscriptionDto(tr.phonemic, tr.paiboon, tr.ipa),
                                    multiset(
                                        SELECT l.value
                                        FROM WordLexeme wl
                                        JOIN Lexeme l ON l.id = wl.lexemeId
                                        WHERE wl.wordId = w.id))
                                FROM Word w
                                LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                                WHERE w.spellingId = sp.id
                                ORDER BY w.id))
                        FROM Spelling sp
                        JOIN Word w ON w.spellingId = sp.id
                        GROUP BY sp.id, sp.value
                        HAVING COUNT(w) >= 2
                        ORDER BY sp.id
                        """,
                        HomographGroupDto.class)
                .getResultList();

        List<HomophoneGroupDto> homophones = entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.dto.HomophoneGroupDto(
                            new com.thaireference.dto.TranscriptionDto(t.phonemic, t.paiboon, t.ipa),
                            multiset(
                                SELECT new com.thaireference.dto.WordDto(
                                    w.id,
                                    sp.value,
                                    new com.thaireference.dto.TranscriptionDto(tr.phonemic, tr.paiboon, tr.ipa),
                                    multiset(
                                        SELECT l.value
                                        FROM WordLexeme wl
                                        JOIN Lexeme l ON l.id = wl.lexemeId
                                        WHERE wl.wordId = w.id))
                                FROM Word w
                                JOIN Spelling sp ON sp.id = w.spellingId
                                LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                                WHERE w.transcriptionId = t.id
                                ORDER BY w.id))
                        FROM Transcription t
                        JOIN Word w ON w.transcriptionId = t.id
                            AND (SELECT COUNT(w2) FROM Word w2 WHERE w2.spellingId = w.spellingId) = 1
                        GROUP BY t.id, t.phonemic, t.paiboon, t.ipa
                        HAVING COUNT(w) >= 2
                        ORDER BY t.id
                        """,
                        HomophoneGroupDto.class)
                .getResultList();

        return new HomonymsDto(homophones, homographs);
    }
}
