package com.thaireference.dto;

import java.util.List;

public record HueFamilyDto(
        Integer id,
        String thaiName,
        String englishName,
        String hex,
        Integer displayOrder,
        List<ColorDto> colors) {

    /** Constructor for JPQL projection (without colors). */
    public HueFamilyDto(Integer id, String thaiName, String englishName, String hex, Integer displayOrder) {
        this(id, thaiName, englishName, hex, displayOrder, List.of());
    }
}
