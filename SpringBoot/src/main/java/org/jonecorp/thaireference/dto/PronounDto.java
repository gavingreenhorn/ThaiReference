package org.jonecorp.thaireference.dto;

import java.util.List;

public record PronounDto(
        WordDto name,
        int person,
        int plurality,
        String formality,
        String gender,
        List<String> types) {

    public PronounDto(WordDto name, int person, int plurality, String formality, String gender) {
        this(name, person, plurality, formality, gender, List.of());
    }
}
