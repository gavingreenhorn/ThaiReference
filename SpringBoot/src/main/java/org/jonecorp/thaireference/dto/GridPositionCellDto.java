package org.jonecorp.thaireference.dto;

import java.util.List;

public record GridPositionCellDto(
        Integer ordinal,
        Integer row,
        Integer col,
        String tileStyle,
        String label,
        List<WordDto> words) {}
