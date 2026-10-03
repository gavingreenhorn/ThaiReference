package com.thaireference.dto;

public record DayOfWeekDto(Integer ordinal, WordDto word, String deity, String imageUrl) {

    public DayOfWeekDto(Integer ordinal, WordDto word) {
        this(ordinal, word, null, null);
    }
}
