package org.jonecorp.thaireference.dto;

public record ZodiacDto(
        Integer ordinal,
        String englishName,
        String sanskritName,
        String sanskritLexeme,
        String element,
        String glyph) {}
