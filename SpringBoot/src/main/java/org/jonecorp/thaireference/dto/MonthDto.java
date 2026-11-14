package org.jonecorp.thaireference.dto;

public record MonthDto(
        Integer ordinal,
        WordDto word,
        String shortName,
        String englishName,
        ZodiacDto zodiac) {}
