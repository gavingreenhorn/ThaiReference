package com.thaireference.dto;

import java.util.List;

public record WordDto(
        Integer id,
        String value,
        TranscriptionDto transcription,
        List<String> lexemes) {

    public WordDto(Integer id, String value, TranscriptionDto transcription) {
        this(id, value, transcription, List.of());
    }
}
