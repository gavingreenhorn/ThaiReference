package org.jonecorp.thaireference.dto;

import com.fasterxml.jackson.annotation.JsonInclude;
import java.util.List;

public record PositionDto(Integer id, String lexeme,
        @JsonInclude(JsonInclude.Include.ALWAYS) WordDto word,
        List<WordDto> phraseWords, SentenceDto example) {}
