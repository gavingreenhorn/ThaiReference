package com.thaireference.dto;

import java.util.List;

public record SentenceDto(Integer id, String translation, List<WordDto> words) {}
