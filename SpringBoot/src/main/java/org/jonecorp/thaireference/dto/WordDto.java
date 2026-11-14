package org.jonecorp.thaireference.dto;

import java.util.List;

public record WordDto(
        Integer id,
        String value,
        TranscriptionDto transcription,
        List<String> lexemes) {

    // array_agg(...) filter(...) yields SQL NULL for words with no lexemes; the client treats
    // lexemes as a non-null list, so normalise here (dialect-safe, unlike an empty-array literal).
    public WordDto(Integer id, String value, TranscriptionDto transcription, String[] lexemes) {
        this(id, value, transcription, lexemes == null ? List.of() : List.of(lexemes));
    }
}
