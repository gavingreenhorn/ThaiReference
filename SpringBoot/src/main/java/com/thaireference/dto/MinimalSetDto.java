package com.thaireference.dto;

import java.util.List;

public record MinimalSetDto(Integer id, String label, List<WordDto> words) {

    public MinimalSetDto(Integer id, String label) {
        this(id, label, List.of());
    }
}
