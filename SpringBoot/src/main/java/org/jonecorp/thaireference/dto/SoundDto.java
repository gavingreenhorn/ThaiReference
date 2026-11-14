package org.jonecorp.thaireference.dto;

public record SoundDto(
        Integer id,
        String value,
        String soundType,
        boolean isConsonantSound,
        boolean isInitialOnly,
        String mediaUrl) {}
