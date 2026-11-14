package org.jonecorp.thaireference.dto;

import java.util.List;

public record SeasonDto(
		Integer ordinal,
		List<WordDto> words,
		String englishLabel,
		String glyph,
		List<MonthDto> months) {}
