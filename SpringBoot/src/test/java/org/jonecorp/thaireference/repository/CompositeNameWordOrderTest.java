package org.jonecorp.thaireference.repository;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNull;
import static org.junit.jupiter.api.Assertions.assertSame;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.mockito.Mockito.RETURNS_DEEP_STUBS;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import jakarta.persistence.EntityManager;
import java.util.List;
import java.util.Map;
import org.jonecorp.thaireference.dto.TranscriptionDto;
import org.jonecorp.thaireference.dto.WordDto;
import org.jonecorp.thaireference.entity.CompositeName;
import org.junit.jupiter.api.Test;
import org.springframework.test.util.ReflectionTestUtils;

class CompositeNameWordOrderTest {

    @Test
    void preservesRepeatedWordsInSlotOrder() {
        CompositeName name = new CompositeName();
        name.setWord1Id(2);
        name.setWord2Id(1);
        name.setWord4Id(2);
        WordDto first = new WordDto(1, "first", new TranscriptionDto("", ""), List.of(), List.of());
        WordDto second = new WordDto(2, "second", new TranscriptionDto("", ""), List.of(), List.of());

        assertEquals(List.of(second, first, second),
                ThaiReadRepositoryImpl.orderedCompositeWords(name, Map.of(1, first, 2, second)));
    }

    @Test
    void rejectsMissingWord() {
        CompositeName name = new CompositeName();
        name.setWord1Id(1);

        assertThrows(IllegalStateException.class,
                () -> ThaiReadRepositoryImpl.orderedCompositeWords(name, Map.of()));
    }

    @Test
    void positionsResolveWholeWordsAndRetainOrderedPhrases() {
        EntityManager manager = mock(EntityManager.class, RETURNS_DEEP_STUBS);
        ThaiReadRepositoryImpl repository = new ThaiReadRepositoryImpl();
        ReflectionTestUtils.setField(repository, "entityManager", manager);
        WordDto whole = new WordDto(520, "north", new TranscriptionDto("a", "a"), List.of("north"), List.of());
        WordDto side = new WordDto(535, "side", new TranscriptionDto("b", "b"), List.of("side"), List.of());
        WordDto left = new WordDto(174, "left", new TranscriptionDto("c", "c"), List.of("left"), List.of());
        CompositeName phrase = new CompositeName();
        phrase.setWord1Id(535);
        phrase.setWord2Id(174);
        when(manager.createQuery(ThaiReadQueries.POSITION_WORDS, CompositeNameWordRow.class).getResultList())
                .thenReturn(List.of(new CompositeNameWordRow(2, side, phrase),
                        new CompositeNameWordRow(2, left, phrase)));
        when(manager.createQuery(ThaiReadQueries.POSITION_LABEL_WORDS, WordRow.class).getResultList())
                .thenReturn(List.of(new WordRow(1, whole)));
        when(manager.createQuery(ThaiReadQueries.SENTENCE_WORDS, WordRow.class).getResultList())
                .thenReturn(List.of(new WordRow(10, left), new WordRow(10, side)));
        when(manager.createQuery(ThaiReadQueries.POSITIONS, PositionHeadRow.class).getResultList())
                .thenReturn(List.of(new PositionHeadRow(1, "above", 10, "example"),
                        new PositionHeadRow(2, "to the left", 10, "example")));

        var positions = repository.findPositions();
        assertSame(whole, positions.get(0).word());
        assertNull(positions.get(0).phraseWords());
        assertNull(positions.get(1).word());
        assertEquals(List.of(side, left), positions.get(1).phraseWords());
        assertEquals(List.of(left, side), positions.get(0).example().words());
    }
}
