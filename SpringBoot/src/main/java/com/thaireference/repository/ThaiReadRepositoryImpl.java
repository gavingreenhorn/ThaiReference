package com.thaireference.repository;

import static java.util.stream.Collectors.groupingBy;
import static java.util.stream.Collectors.mapping;
import static java.util.stream.Collectors.toList;

import com.thaireference.dto.*;
import jakarta.persistence.EntityManager;
import jakarta.persistence.PersistenceContext;
import java.sql.Array;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Repository;
import org.springframework.transaction.annotation.Transactional;

@Repository
@Transactional(readOnly = true)
public class ThaiReadRepositoryImpl implements ThaiReadRepositoryCustom {

    private static final Logger LOGGER = LoggerFactory.getLogger(ThaiReadRepositoryImpl.class);

    @PersistenceContext
    private EntityManager entityManager;

    @Override
    public List<NumeralDto> findNumerals() {
        return entityManager.createQuery(ThaiReadQueries.NUMERALS, NumeralDto.class).getResultList();
    }

    @Override
    public List<DayOfWeekDto> findWeekdays() {
        return entityManager.createQuery(ThaiReadQueries.WEEKDAYS, DayOfWeekDto.class).getResultList();
    }

    @Override
    public List<TimeOfDayDto> findTimeOfDay() {
        return entityManager.createQuery(ThaiReadQueries.TIME_OF_DAY, TimeOfDayDto.class).getResultList();
    }

    @Override
    public List<HourDto> findHours() {
        return entityManager.createQuery(ThaiReadQueries.HOURS, HourDto.class).getResultList();
    }

    @Override
    public List<MonthDto> findMonths() {
        Map<Integer, List<WordDto>> seasonWordsByMonthOrdinal = wordsByParentId(ThaiReadQueries.MONTH_SEASON_WORDS);

        return entityManager.createQuery(ThaiReadQueries.MONTHS, MonthRow.class).getResultList().stream()
                .map(m -> new MonthDto(
                        m.ordinal(),
                        m.word(),
                        m.shortName(),
                        m.englishName(),
                        m.zodiac(),
                        new SeasonDto(
                                m.seasonOrdinal(),
                                seasonWordsByMonthOrdinal.getOrDefault(m.ordinal(), List.of()),
                                m.seasonEnglishLabel())))
                .toList();
    }

    @Override
    public List<CelestialBodyDto> findCelestialBodies() {
        return entityManager.createQuery(ThaiReadQueries.CELESTIAL_BODIES, CelestialBodyDto.class).getResultList();
    }

    @Override
    public List<ConsonantDto> findConsonants() {
        Map<Integer, List<WordDto>> sampleWords = wordsByParentId(ThaiReadQueries.CONSONANT_SAMPLE_WORDS);

        return entityManager.createQuery(ThaiReadQueries.CONSONANTS, ConsonantRow.class).getResultList().stream()
                .map(c -> new ConsonantDto(
                        c.grapheme(),
                        c.phoneme(),
                        c.consonantClass(),
                        c.ending(),
                        c.transcription(),
                        c.initialSound(),
                        c.endingSound(),
                        c.mediaUrl(),
                        sampleWords.getOrDefault(c.id(), List.of())))
                .toList();
    }

    @Override
    public List<VowelDto> findVowels() {
        Map<Integer, List<VowelGraphemeDto>> graphemes = entityManager
                .createQuery(ThaiReadQueries.VOWEL_GRAPHEMES, GraphemeRow.class).getResultList().stream()
                .collect(groupingBy(GraphemeRow::vowelId, LinkedHashMap::new, mapping(GraphemeRow::grapheme, toList())));
        Map<Integer, List<WordDto>> sampleWords = wordsByParentId(ThaiReadQueries.VOWEL_SAMPLE_WORDS);

        return entityManager.createQuery(ThaiReadQueries.VOWELS, VowelRow.class).getResultList().stream()
                .map(v -> new VowelDto(
                        v.isDiphtong(),
                        v.sound(),
                        graphemes.getOrDefault(v.id(), List.of()),
                        v.mediaUrl(),
                        sampleWords.getOrDefault(v.id(), List.of())))
                .toList();
    }

    @Override
    public List<MarkDto> findMarks() {
        List<MarkDto> marks = new ArrayList<>();
        marks.addAll(specialSymbolMarks());
        marks.addAll(toneMarks());
        return marks;
    }

    @Override
    public List<ThaiClassDto> findClasses() {
        Map<Integer, List<WordDto>> words = wordsByParentId(ThaiReadQueries.CLASS_WORDS);

        return entityManager.createQuery(ThaiReadQueries.CLASSES, ClassRow.class).getResultList().stream()
                .map(c -> new ThaiClassDto(
                        c.code(), c.englishLabel(), words.getOrDefault(c.id(), List.of())))
                .toList();
    }

    @Override
    public List<PronounDto> findPronouns() {
        Map<Integer, List<String>> types = entityManager
                .createQuery(ThaiReadQueries.PRONOUN_TYPES, PronounTypeRow.class).getResultList().stream()
                .collect(groupingBy(
                        PronounTypeRow::pronounId, LinkedHashMap::new, mapping(PronounTypeRow::type, toList())));

        return entityManager.createQuery(ThaiReadQueries.PRONOUNS, PronounRow.class).getResultList().stream()
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
    public List<GridPositionCellDto> findGridPositions() {
        Map<Integer, List<WordDto>> words = wordsByParentId(ThaiReadQueries.GRID_POSITION_WORDS);

        return entityManager.createQuery(ThaiReadQueries.GRID_POSITIONS, GridPositionRow.class).getResultList()
                .stream()
                .map(p -> new GridPositionCellDto(
                        p.id(),
                        p.row(),
                        p.col(),
                        p.tileStyle(),
                        p.label(),
                        words.getOrDefault(p.id(), List.of())))
                .toList();
    }

    @Override
    public List<ClusterDto> findClusters() {
        Map<Integer, List<WordDto>> sampleWords = wordsByParentId(ThaiReadQueries.CLUSTER_WORDS);

        return entityManager.createQuery(ThaiReadQueries.CLUSTERS, ClusterDto.class).getResultList().stream()
                .map(c -> new ClusterDto(
                        c.id(), c.type(), c.subtype(), c.value(), sampleWords.getOrDefault(c.id(), List.of())))
                .toList();
    }

    @Override
    public List<HomophoneGroupDto> findHomophones() {
        Map<Integer, List<WordDto>> wordsByTranscription = wordsByParentId(ThaiReadQueries.HOMOPHONE_WORDS);

        return entityManager.createQuery(ThaiReadQueries.HOMOPHONES, HomophoneRow.class).getResultList().stream()
                .map(h -> new HomophoneGroupDto(
                        h.transcription(), wordsByTranscription.getOrDefault(h.transcriptionId(), List.of())))
                .toList();
    }

    @Override
    public List<MinimalSetDto> findMinimalSetsPage(String label, int offset, int limit) {
        @SuppressWarnings("unchecked")
        List<Object[]> setRows = entityManager
                .createNativeQuery(ThaiReadQueries.MINIMAL_SETS)
                .setParameter("label", label)
                .setParameter("offset", offset)
                .setParameter("limit", limit)
                .getResultList();

        if (setRows.isEmpty()) {
            return List.of();
        }

        Map<Integer, List<WordDto>> wordsBySet = wordsByMinimalSetsPage(label, offset, limit);

        return setRows.stream()
                .map(row -> {
                    int setId = ((Number) row[0]).intValue();
                    String setLabel = (String) row[1];
                    return new MinimalSetDto(setId, setLabel, wordsBySet.getOrDefault(setId, List.of()));
                })
                .toList();
    }

    @Override
    public long countMinimalSets(String label) {
        Object count = entityManager
                .createNativeQuery(ThaiReadQueries.MINIMAL_SET_COUNT)
                .setParameter("label", label)
                .getSingleResult();
        return ((Number) count).longValue();
    }

    @Override
    public List<String> findMinimalSetLabels() {
        @SuppressWarnings("unchecked")
        List<Object> labels = entityManager.createNativeQuery(ThaiReadQueries.MINIMAL_SET_LABELS).getResultList();
        return labels.stream().filter(Objects::nonNull).map(String::valueOf).toList();
    }

    @Override
    public List<DirectionDto> findDirections() {
        Map<Integer, List<WordDto>> words = wordsByParentId(ThaiReadQueries.DIRECTION_WORDS);

        return entityManager.createQuery(ThaiReadQueries.DIRECTIONS, DirectionRow.class).getResultList().stream()
                .map(d -> {
                    List<WordDto> parts = words.getOrDefault(d.id(), List.of());
                    String spelling = parts.stream().map(WordDto::value).reduce("", String::concat);
                    String paiboon = joinTranscription(parts, t -> t.paiboon());
                    String ipa = joinTranscription(parts, t -> t.ipa());
                    return new DirectionDto(
                            d.id(),
                            d.label(),
                            d.lexeme(),
                            spelling,
                            new TranscriptionDto(paiboon, ipa));
                })
                .toList();
    }

    @Override
    public List<PositionDto> findPositions() {
        Map<Integer, List<WordDto>> headWords = wordsByParentId(ThaiReadQueries.POSITION_WORDS);
        Map<Integer, List<WordDto>> sentenceWords = wordsByParentId(ThaiReadQueries.SENTENCE_WORDS);

        return entityManager.createQuery(ThaiReadQueries.POSITIONS, PositionHeadRow.class).getResultList().stream()
                .map(p -> new PositionDto(
                        p.id(),
                        p.lexeme(),
                        headWords.getOrDefault(p.id(), List.of()),
                        new SentenceDto(
                                p.sentenceId(),
                                p.translation(),
                                sentenceWords.getOrDefault(p.sentenceId(), List.of()))))
                .toList();
    }

    private static String joinTranscription(
            List<WordDto> parts, java.util.function.Function<TranscriptionDto, String> field) {
        return parts.stream()
                .map(WordDto::transcription)
                .filter(t -> t != null)
                .map(field)
                .filter(s -> s != null && !s.isBlank())
                .reduce((a, b) -> a + " " + b)
                .orElse("");
    }

    private List<MarkDto> specialSymbolMarks() {
        Map<Integer, List<WordDto>> words = wordsByParentId(ThaiReadQueries.MARK_SPECIAL_SYMBOL_WORDS);

        return entityManager.createQuery(ThaiReadQueries.MARK_SPECIAL_SYMBOLS, MarkRow.class).getResultList().stream()
                .map(m -> new MarkDto(m.grapheme(), m.toneMark(), words.getOrDefault(m.id(), List.of())))
                .toList();
    }

    private List<MarkDto> toneMarks() {
        Map<Integer, List<WordDto>> words = wordsByParentId(ThaiReadQueries.MARK_TONE_MARK_WORDS);

        return entityManager.createQuery(ThaiReadQueries.MARK_TONE_MARKS, MarkRow.class).getResultList().stream()
                .map(m -> new MarkDto(
                        m.grapheme(), m.toneMark(), words.getOrDefault(m.id(), List.of()), m.englishLabel()))
                .toList();
    }

    private Map<Integer, List<WordDto>> wordsByParentId(String wordQuery) {
        return entityManager.createQuery(wordQuery, WordRow.class).getResultList().stream()
                .collect(groupingBy(WordRow::parentId, LinkedHashMap::new, mapping(WordRow::word, toList())));
    }

    private Map<Integer, List<WordDto>> wordsByMinimalSetsPage(String label, int offset, int limit) {
        @SuppressWarnings("unchecked")
        List<Object[]> rows = entityManager
                .createNativeQuery(ThaiReadQueries.MINIMAL_SET_WORDS_PAGE)
                .setParameter("label", label)
                .setParameter("offset", offset)
                .setParameter("limit", limit)
                .getResultList();

        Map<Integer, List<WordDto>> result = new LinkedHashMap<>();
        for (Object[] row : rows) {
            Integer setId = ((Number) row[0]).intValue();
            Integer wordId = ((Number) row[1]).intValue();
            String spelling = (String) row[2];
            String paiboon = (String) row[3];
            String ipa = (String) row[4];
            List<String> lexemes = toStringList(row[5]);

            WordDto word = new WordDto(wordId, spelling, new TranscriptionDto(paiboon, ipa), lexemes);
            result.computeIfAbsent(setId, _ -> new ArrayList<>()).add(word);
        }

        return result;
    }

    private static List<String> toStringList(Object sqlArrayOrNull) {
        if (sqlArrayOrNull == null) {
            return List.of();
        }
        // Hibernate 6 maps a native-query text[] column to a Java array directly; only some
        // drivers hand back java.sql.Array. Handle both rather than silently dropping the data.
        if (sqlArrayOrNull instanceof String[] strings) {
            return Arrays.stream(strings).filter(Objects::nonNull).toList();
        }
        if (sqlArrayOrNull instanceof Object[] objects) {
            return Arrays.stream(objects).filter(Objects::nonNull).map(String::valueOf).toList();
        }
        if (sqlArrayOrNull instanceof Collection<?> collection) {
            return collection.stream().filter(Objects::nonNull).map(String::valueOf).toList();
        }
        if (sqlArrayOrNull instanceof Array array) {
            try {
                Object value = array.getArray();
                if (value instanceof Object[] objects) {
                    return Arrays.stream(objects).filter(Objects::nonNull).map(String::valueOf).toList();
                }
            } catch (SQLException e) {
                LOGGER.warn("Could not read SQL array of lexemes", e);
            }
            return List.of();
        }
        LOGGER.warn("Unexpected lexemes column type {}; returning empty list",
                sqlArrayOrNull.getClass().getName());
        return List.of();
    }

    @Override
    public List<HueFamilyDto> findColors() {
        List<HueFamilyDto> families = entityManager
                .createQuery(ThaiReadQueries.HUE_FAMILIES, HueFamilyDto.class).getResultList();
        List<ColorRow> colors = entityManager
                .createQuery(ThaiReadQueries.COLORS, ColorRow.class).getResultList();
        Map<Integer, List<WordDto>> wordsByColor = wordsByParentId(ThaiReadQueries.COLOR_WORDS);

        Map<Integer, List<ColorRow>> colorsByFamily = colors.stream()
                .collect(groupingBy(ColorRow::familyId, LinkedHashMap::new, toList()));

        return families.stream()
                .map(f -> new HueFamilyDto(
                        f.id(),
                        f.thaiName(),
                        f.englishName(),
                        f.hex(),
                        f.displayOrder(),
                        colorsByFamily.getOrDefault(f.id(), List.of()).stream()
                                .map(c -> new ColorDto(
                                        c.colorId(),
                                        c.hex(),
                                        c.englishLabel(),
                                        wordsByColor.getOrDefault(c.colorId(), List.of()),
                                        c.luminanceOrder()))
                                .toList()))
                .toList();
    }
}
