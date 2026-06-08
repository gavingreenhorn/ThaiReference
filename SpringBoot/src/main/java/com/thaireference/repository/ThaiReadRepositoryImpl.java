package com.thaireference.repository;

import com.thaireference.dto.*;
import jakarta.persistence.EntityManager;
import jakarta.persistence.PersistenceContext;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import org.springframework.stereotype.Repository;
import org.springframework.transaction.annotation.Transactional;

@Repository
@Transactional(readOnly = true)
public class ThaiReadRepositoryImpl implements ThaiReadRepositoryCustom {

    private static final String LEXEMES =
            "(SELECT array_agg(l.value) FROM WordLexeme wl JOIN Lexeme l ON l.id = wl.lexemeId WHERE wl.wordId = w.id)";

    private static final String WORD_DTO =
            """
            new com.thaireference.dto.WordDto(
                w.id,
                sp.value,
                new com.thaireference.dto.TranscriptionDto(tr.phonemic, tr.paiboon, tr.ipa),
                """
                    + LEXEMES
                    + ")";

    @PersistenceContext
    private EntityManager entityManager;

    private record IdWordRow(Integer parentId, WordDto word) {}

    private record IdStringRow(Integer parentId, String value) {}

    private record IdGraphemeRow(Integer vowelId, VowelGraphemeDto grapheme) {}

    private record ConsonantCoreRow(
            Integer id,
            String grapheme,
            String phoneme,
            String consonantClass,
            String ending,
            TranscriptionDto transcription,
            SoundDto initialSound,
            SoundDto endingSound,
            String mediaUrl) {}

    private record VowelCoreRow(
            Integer id, boolean isDiphtong, SoundDto sound, String mediaUrl) {}

    private record MarkCoreRow(Integer id, String grapheme, boolean toneMark, String englishLabel) {}

    private record PronounCoreRow(
            Integer id,
            WordDto name,
            int person,
            int plurality,
            String formality,
            String gender) {}

    private record ClusterCoreRow(Integer id, String clusterType, String subtype, String value) {}

    private record PositionCoreRow(
            Integer id, Integer rowIndex, Integer colIndex, String tileStyle, String label) {}

    private record HomographCoreRow(Integer spellingId, String spelling) {}

    private record HomophoneCoreRow(Integer transcriptionId, TranscriptionDto transcription) {}

    @Override
    public List<NumeralDto> findNumerals() {
        return entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.dto.NumeralDto(
                            n.grapheme,
                            """
                                + WORD_DTO
                                + """
                        )
                        FROM Numeral n
                        JOIN Word w ON w.id = n.wordId
                        JOIN Spelling sp ON sp.id = w.spellingId
                        LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
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
                            """
                                + WORD_DTO
                                + """
                            ,
                            d.deity,
                            d.imageUrl)
                        FROM DayOfWeek d
                        JOIN Word w ON w.id = d.wordId
                        JOIN Spelling sp ON sp.id = w.spellingId
                        LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
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
                            """
                                + WORD_DTO
                                + """
                        )
                        FROM TimeOfDay td
                        JOIN Word w ON w.id = td.wordId
                        JOIN Spelling sp ON sp.id = w.spellingId
                        LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
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
                            """
                                + WORD_DTO
                                + """
                        )
                        FROM Hour h
                        JOIN Word w ON w.id = h.wordId
                        JOIN Spelling sp ON sp.id = w.spellingId
                        LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
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
                            """
                                + WORD_DTO
                                + """
                            ,
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
                        JOIN Spelling sp ON sp.id = w.spellingId
                        LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
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
                            """
                                + WORD_DTO
                                + """
                        )
                        FROM CelestialBody cb
                        JOIN Word w ON w.id = cb.wordId
                        JOIN Spelling sp ON sp.id = w.spellingId
                        LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                        ORDER BY cb.ordinal
                        """,
                        CelestialBodyDto.class)
                .getResultList();
    }

    @Override
    public List<ConsonantDto> findConsonants() {
        List<ConsonantCoreRow> cores = entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$ConsonantCoreRow(
                            c.id,
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
                                COALESCE(sFinal.id, 0),
                                COALESCE(sFinal.value, ''),
                                COALESCE(stFinal.name, ''),
                                CASE WHEN sFinal.consonantSound IS NOT NULL THEN true ELSE false END,
                                COALESCE(sFinal.initialOnly, false),
                                sFinal.mediapath),
                            c.mediapath)
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
                        ConsonantCoreRow.class)
                .getResultList();

        Map<Integer, List<WordDto>> samples = loadWordsByParent(
                """
                SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$IdWordRow(
                    cs.consonantId,
                    """
                        + WORD_DTO
                        + """
                )
                FROM ConsonantSample cs
                JOIN Word w ON w.id = cs.wordId
                JOIN Spelling sp ON sp.id = w.spellingId
                LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                ORDER BY cs.consonantId, cs.id
                """);

        return cores.stream()
                .map(c -> new ConsonantDto(
                        c.grapheme(),
                        c.phoneme(),
                        c.consonantClass(),
                        c.ending(),
                        c.transcription(),
                        c.initialSound(),
                        c.endingSound(),
                        c.mediaUrl(),
                        samples.getOrDefault(c.id(), List.of())))
                .toList();
    }

    @Override
    public List<VowelDto> findVowels() {
        List<VowelCoreRow> cores = entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$VowelCoreRow(
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
                        """,
                        VowelCoreRow.class)
                .getResultList();

        Map<Integer, List<VowelGraphemeDto>> graphemes = loadGraphemesByVowel();
        Map<Integer, List<WordDto>> samples = loadWordsByParent(
                """
                SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$IdWordRow(
                    vsmp.vowelId,
                    """
                        + WORD_DTO
                        + """
                )
                FROM VowelSample vsmp
                JOIN Word w ON w.id = vsmp.wordId
                JOIN Spelling sp ON sp.id = w.spellingId
                LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                ORDER BY vsmp.vowelId, vsmp.id
                """);

        return cores.stream()
                .map(v -> new VowelDto(
                        v.isDiphtong(),
                        v.sound(),
                        graphemes.getOrDefault(v.id(), List.of()),
                        v.mediaUrl(),
                        samples.getOrDefault(v.id(), List.of())))
                .toList();
    }

    @Override
    public List<MarkDto> findMarks() {
        List<MarkDto> marks = new ArrayList<>();
        marks.addAll(loadSpecialSymbolMarks());
        marks.addAll(loadToneMarks());
        return marks;
    }

    @Override
    public List<PronounDto> findPronouns() {
        List<PronounCoreRow> cores = entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$PronounCoreRow(
                            p.id,
                            """
                                + WORD_DTO
                                + """
                            ,
                            COALESCE(p.grammaticalPerson, 0),
                            CASE
                                WHEN LOWER(pl.name) LIKE '%singular%' THEN 1
                                WHEN LOWER(pl.name) LIKE '%plural%' THEN 2
                                ELSE 3
                            END,
                            COALESCE(f.name, ''),
                            COALESCE(g.name, ''))
                        FROM Pronoun p
                        JOIN Word w ON w.id = p.wordId
                        JOIN Spelling sp ON sp.id = w.spellingId
                        LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                        LEFT JOIN Plurality pl ON pl.id = p.pluralityId
                        LEFT JOIN Formality f ON f.id = p.formalityId
                        LEFT JOIN Gender g ON g.id = p.genderId
                        ORDER BY p.id
                        """,
                        PronounCoreRow.class)
                .getResultList();

        Map<Integer, List<String>> types = loadPronounTypes();

        return cores.stream()
                .map(p -> new PronounDto(
                        p.name(),
                        p.person(),
                        p.plurality(),
                        p.formality(),
                        p.gender(),
                        types.getOrDefault(p.id(), List.of())))
                .toList();
    }

    @Override
    public List<PositionCellDto> findPositions() {
        List<PositionCoreRow> cores = entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$PositionCoreRow(
                            pos.id,
                            pos.rowIndex,
                            pos.colIndex,
                            pos.tileStyle,
                            pos.name)
                        FROM Position pos
                        ORDER BY pos.rowIndex, pos.colIndex
                        """,
                        PositionCoreRow.class)
                .getResultList();

        Map<Integer, List<WordDto>> words = loadWordsByParent(
                """
                SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$IdWordRow(
                    pw.positionId,
                    """
                        + WORD_DTO
                        + """
                )
                FROM PositionWord pw
                JOIN Word w ON w.id = pw.wordId
                JOIN Spelling sp ON sp.id = w.spellingId
                LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                ORDER BY pw.positionId, pw.wordOrder
                """);

        return cores.stream()
                .map(c -> new PositionCellDto(
                        c.id(),
                        c.rowIndex(),
                        c.colIndex(),
                        c.tileStyle(),
                        c.label(),
                        words.getOrDefault(c.id(), List.of())))
                .toList();
    }

    @Override
    public List<ClusterDto> findClusters() {
        List<ClusterCoreRow> cores = entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$ClusterCoreRow(
                            cl.id,
                            cl.clusterType,
                            cl.subtype,
                            cl.value)
                        FROM Cluster cl
                        ORDER BY cl.clusterType, cl.subtype, cl.value, cl.id
                        """,
                        ClusterCoreRow.class)
                .getResultList();

        Map<Integer, List<WordDto>> samples = loadWordsByParent(
                """
                SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$IdWordRow(
                    cw.clusterId,
                    """
                        + WORD_DTO
                        + """
                )
                FROM ClusterWord cw
                JOIN Word w ON w.id = cw.wordId
                JOIN Spelling sp ON sp.id = w.spellingId
                LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                ORDER BY cw.clusterId, cw.id
                """);

        return cores.stream()
                .map(c -> new ClusterDto(
                        c.id(), c.clusterType(), c.subtype(), c.value(), samples.getOrDefault(c.id(), List.of())))
                .toList();
    }

    @Override
    public HomonymsDto findHomonyms() {
        List<HomographCoreRow> homographCores = entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$HomographCoreRow(
                            sp.id,
                            sp.value)
                        FROM Spelling sp
                        JOIN Word w ON w.spellingId = sp.id
                        GROUP BY sp.id, sp.value
                        HAVING COUNT(w) >= 2
                        ORDER BY sp.id
                        """,
                        HomographCoreRow.class)
                .getResultList();

        Map<Integer, List<WordDto>> homographWords = loadWordsBySpelling();

        List<HomographGroupDto> homographs = homographCores.stream()
                .map(h -> new HomographGroupDto(h.spelling(), homographWords.getOrDefault(h.spellingId(), List.of())))
                .toList();

        List<HomophoneCoreRow> homophoneCores = entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$HomophoneCoreRow(
                            t.id,
                            new com.thaireference.dto.TranscriptionDto(t.phonemic, t.paiboon, t.ipa))
                        FROM Transcription t
                        JOIN Word w ON w.transcriptionId = t.id
                            AND (SELECT COUNT(w2) FROM Word w2 WHERE w2.spellingId = w.spellingId) = 1
                        GROUP BY t.id, t.phonemic, t.paiboon, t.ipa
                        HAVING COUNT(w) >= 2
                        ORDER BY t.id
                        """,
                        HomophoneCoreRow.class)
                .getResultList();

        Map<Integer, List<WordDto>> homophoneWords = loadWordsByTranscription();

        List<HomophoneGroupDto> homophones = homophoneCores.stream()
                .map(h -> new HomophoneGroupDto(h.transcription(), homophoneWords.getOrDefault(h.transcriptionId(), List.of())))
                .toList();

        return new HomonymsDto(homophones, homographs);
    }

    private Map<Integer, List<WordDto>> loadWordsByParent(String jpql) {
        List<IdWordRow> rows = entityManager.createQuery(jpql, IdWordRow.class).getResultList();
        Map<Integer, List<WordDto>> grouped = new LinkedHashMap<>();
        for (IdWordRow row : rows) {
            grouped.computeIfAbsent(row.parentId(), ignored -> new ArrayList<>()).add(row.word());
        }
        return grouped;
    }

    private Map<Integer, List<VowelGraphemeDto>> loadGraphemesByVowel() {
        List<IdGraphemeRow> rows = entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$IdGraphemeRow(
                            vg.vowelId,
                            new com.thaireference.dto.VowelGraphemeDto(
                                COALESCE(vg.value, ''),
                                COALESCE(vg.closed, false)))
                        FROM VowelGrapheme vg
                        ORDER BY vg.vowelId, vg.id
                        """,
                        IdGraphemeRow.class)
                .getResultList();
        Map<Integer, List<VowelGraphemeDto>> grouped = new LinkedHashMap<>();
        for (IdGraphemeRow row : rows) {
            grouped.computeIfAbsent(row.vowelId(), ignored -> new ArrayList<>()).add(row.grapheme());
        }
        return grouped;
    }

    private Map<Integer, List<String>> loadPronounTypes() {
        List<IdStringRow> rows = entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$IdStringRow(
                            ptb.pronounId,
                            pt.name)
                        FROM PronounTypeBreakout ptb
                        JOIN PronounType pt ON pt.id = ptb.typeId
                        WHERE pt.name IS NOT NULL
                        ORDER BY ptb.pronounId, ptb.id
                        """,
                        IdStringRow.class)
                .getResultList();
        Map<Integer, List<String>> grouped = new LinkedHashMap<>();
        for (IdStringRow row : rows) {
            grouped.computeIfAbsent(row.parentId(), ignored -> new ArrayList<>()).add(row.value());
        }
        return grouped;
    }

    private Map<Integer, List<WordDto>> loadWordsBySpelling() {
        return loadWordsByParent(
                """
                SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$IdWordRow(
                    w.spellingId,
                    """
                        + WORD_DTO
                        + """
                )
                FROM Word w
                JOIN Spelling sp ON sp.id = w.spellingId
                LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                ORDER BY w.spellingId, w.id
                """);
    }

    private Map<Integer, List<WordDto>> loadWordsByTranscription() {
        return loadWordsByParent(
                """
                SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$IdWordRow(
                    w.transcriptionId,
                    """
                        + WORD_DTO
                        + """
                )
                FROM Word w
                JOIN Spelling sp ON sp.id = w.spellingId
                LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                WHERE w.transcriptionId IS NOT NULL
                ORDER BY w.transcriptionId, w.id
                """);
    }

    private List<MarkDto> loadSpecialSymbolMarks() {
        List<MarkCoreRow> cores = entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$MarkCoreRow(
                            ss.id,
                            COALESCE(ss.grapheme, ''),
                            false,
                            null)
                        FROM SpecialSymbol ss
                        ORDER BY ss.id
                        """,
                        MarkCoreRow.class)
                .getResultList();

        Map<Integer, List<WordDto>> words = loadWordsByParent(
                """
                SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$IdWordRow(
                    ss.id,
                    """
                        + WORD_DTO
                        + """
                )
                FROM SpecialSymbol ss
                JOIN Word w ON w.id = ss.wordId
                JOIN Spelling sp ON sp.id = w.spellingId
                LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                WHERE ss.wordId IS NOT NULL
                ORDER BY ss.id
                """);

        return cores.stream()
                .map(m -> new MarkDto(m.grapheme(), m.toneMark(), words.getOrDefault(m.id(), List.of())))
                .toList();
    }

    private List<MarkDto> loadToneMarks() {
        List<MarkCoreRow> cores = entityManager
                .createQuery(
                        """
                        SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$MarkCoreRow(
                            tm.id,
                            COALESCE(tm.mark, ''),
                            true,
                            tm.englishLabel)
                        FROM ToneMark tm
                        ORDER BY tm.id
                        """,
                        MarkCoreRow.class)
                .getResultList();

        Map<Integer, List<WordDto>> words = loadWordsByParent(
                """
                SELECT new com.thaireference.repository.ThaiReadRepositoryImpl$IdWordRow(
                    tm.id,
                    """
                        + WORD_DTO
                        + """
                )
                FROM ToneMark tm
                JOIN CompositeName cn ON cn.id = tm.nameId
                JOIN Word w ON w.id IN (cn.word1Id, cn.word2Id, cn.word3Id, cn.word4Id, cn.word5Id)
                JOIN Spelling sp ON sp.id = w.spellingId
                LEFT JOIN Transcription tr ON tr.id = w.transcriptionId
                ORDER BY tm.id,
                    CASE
                        WHEN w.id = cn.word1Id THEN 1
                        WHEN w.id = cn.word2Id THEN 2
                        WHEN w.id = cn.word3Id THEN 3
                        WHEN w.id = cn.word4Id THEN 4
                        WHEN w.id = cn.word5Id THEN 5
                        ELSE 6 END
                """);

        return cores.stream()
                .map(m -> new MarkDto(m.grapheme(), m.toneMark(), words.getOrDefault(m.id(), List.of()), m.englishLabel()))
                .toList();
    }
}
