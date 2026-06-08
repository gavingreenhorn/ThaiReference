package com.thaireference.dto;

import java.util.List;

public record MarkDto(String grapheme, boolean toneMark, List<WordDto> words, String englishLabel) {

    public MarkDto(String grapheme, boolean toneMark, List<WordDto> words) {
        this(grapheme, toneMark, words, null);
    }
}
