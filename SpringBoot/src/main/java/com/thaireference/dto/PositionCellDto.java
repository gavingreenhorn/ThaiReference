package com.thaireference.dto;

import java.util.List;

public record PositionCellDto(
        Integer ordinal,
        Integer row,
        Integer col,
        String tileStyle,
        String label,
        List<WordDto> words) {}
