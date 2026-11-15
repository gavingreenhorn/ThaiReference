package org.jonecorp.thaireference.dto;

import java.util.List;

public record MoodDto(Integer id, WordDto word, String svg, List<MoodSentenceDto> sentences) {}
