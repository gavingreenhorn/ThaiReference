package org.jonecorp.thaireference.dto;

import java.util.List;

public record MoodSentenceDto(
        Integer id,
        String translation,
        List<SentenceTokenDto> words,
        String sourceUrl,
        String sourceAuthor,
        String sourceLicense) {}
