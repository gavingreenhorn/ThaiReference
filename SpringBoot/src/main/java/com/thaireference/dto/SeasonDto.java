package com.thaireference.dto;

import java.util.List;

public record SeasonDto(
		Integer ordinal,
		List<WordDto> words,
		String englishLabel) {}
