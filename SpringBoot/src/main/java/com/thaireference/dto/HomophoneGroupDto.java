package com.thaireference.dto;

import java.util.List;

public record HomophoneGroupDto(TranscriptionDto transcription, List<WordDto> words) {}
