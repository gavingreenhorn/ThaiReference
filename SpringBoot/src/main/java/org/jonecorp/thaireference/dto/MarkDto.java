package org.jonecorp.thaireference.dto;

import com.fasterxml.jackson.annotation.JsonInclude;

public record MarkDto(String grapheme, boolean toneMark,
        @JsonInclude(JsonInclude.Include.ALWAYS) WordDto word, String englishLabel) {

    public MarkDto(String grapheme, boolean toneMark, WordDto word) {
        this(grapheme, toneMark, word, null);
    }
}
