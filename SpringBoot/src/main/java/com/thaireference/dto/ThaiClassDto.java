package com.thaireference.dto;

import java.util.List;

public record ThaiClassDto(String code, String englishLabel, List<WordDto> words) {}
