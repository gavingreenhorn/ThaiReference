package org.jonecorp.thaireference.dto;

import java.util.List;

public record ColorDto(
        Integer id,
        String hex,
        String englishLabel,
        List<WordDto> words,
        Integer luminanceOrder) {}
