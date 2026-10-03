package com.thaireference.dto;

import java.util.List;

public record PositionDto(Integer id, String lexeme, List<WordDto> words, SentenceDto example) {}
