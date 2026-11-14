package org.jonecorp.thaireference.dto;

import java.util.List;

public record SentenceDto(Integer id, String translation, List<WordDto> words) {}
