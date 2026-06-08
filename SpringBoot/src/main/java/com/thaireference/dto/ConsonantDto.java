package com.thaireference.dto;

import java.util.List;

public record ConsonantDto(
        String grapheme,
        String phoneme,
        String consonantClass,
        String ending,
        TranscriptionDto transcription,
        SoundDto initialSound,
        SoundDto endingSound,
        String mediaUrl,
        List<WordDto> sampleWords) {

    public ConsonantDto(
            String grapheme,
            String phoneme,
            String consonantClass,
            String ending,
            TranscriptionDto transcription,
            SoundDto initialSound,
            SoundDto endingSound,
            String mediaUrl) {
        this(grapheme, phoneme, consonantClass, ending, transcription, initialSound, endingSound, mediaUrl, List.of());
    }
}
