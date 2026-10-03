package com.thaireference.dto;

public record ZodiacDto(
        Integer ordinal,
        String englishName,
        String sanskritName,
        String sanskritLexeme,
        String element,
        String imageUrl) {}
