package org.jonecorp.thaireference.repository;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertThrows;

import java.util.List;
import java.util.Map;
import org.jonecorp.thaireference.dto.TranscriptionDto;
import org.jonecorp.thaireference.dto.WordDto;
import org.jonecorp.thaireference.entity.CompositeName;
import org.junit.jupiter.api.Test;

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
}
