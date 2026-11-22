package org.jonecorp.thaireference.dto;

import java.util.List;

public record SeasonDto(
		Integer ordinal,
		WordDto word,
		String englishLabel,
		String glyph,
		List<MonthDto> months) {}
