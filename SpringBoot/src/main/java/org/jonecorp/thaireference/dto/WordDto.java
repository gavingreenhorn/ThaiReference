package org.jonecorp.thaireference.dto;

import com.fasterxml.jackson.annotation.JsonInclude;
import java.util.List;

public record WordDto(
        Integer id,
        String value,
        TranscriptionDto transcription,
        List<String> lexemes,
        @JsonInclude(JsonInclude.Include.NON_EMPTY) List<Integer> componentWordIds) {

    // array_agg(...) filter(...) yields SQL NULL for words with no lexemes; the client treats
    // lexemes as a non-null list, so normalise here (dialect-safe, unlike an empty-array literal).
    public WordDto(Integer id, String value, TranscriptionDto transcription,
            String[] lexemes, Integer[] componentWordIds) {
        this(id, value, transcription, lexemes == null ? List.of() : List.of(lexemes),
                List.of(componentWordIds));
    }
}
