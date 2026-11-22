package org.jonecorp.thaireference.dto;

public record DirectionDto(
        Integer id,
        String label,
        String lexeme,
        WordDto word) {}
