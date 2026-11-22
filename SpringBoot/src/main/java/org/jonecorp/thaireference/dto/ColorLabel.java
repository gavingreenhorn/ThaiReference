package org.jonecorp.thaireference.dto;

import java.util.List;

public record ColorLabel(
        Integer id,
        String hex,
        String englishLabel,
        List<WordDto> words,
        Integer luminanceOrder) {}
