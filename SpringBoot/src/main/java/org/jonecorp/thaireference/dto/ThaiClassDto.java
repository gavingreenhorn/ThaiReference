package org.jonecorp.thaireference.dto;

import java.util.List;

public record ThaiClassDto(String code, String englishLabel, List<WordDto> words) {}
