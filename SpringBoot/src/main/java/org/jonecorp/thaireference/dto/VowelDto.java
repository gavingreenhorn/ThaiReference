package org.jonecorp.thaireference.dto;

import java.util.List;

public record VowelDto(
        boolean isDiphtong,
        SoundDto sound,
        List<VowelGraphemeDto> graphemes,
        String mediaUrl,
        List<WordDto> sampleWords) {

    public VowelDto(boolean isDiphtong, SoundDto sound, String mediaUrl) {
        this(isDiphtong, sound, List.of(), mediaUrl, List.of());
    }
}
