package com.thaireference.dto;

import java.util.List;

public record HomonymsDto(List<HomophoneGroupDto> homophones, List<HomographGroupDto> homographs) {

    public HomonymsDto() {
        this(List.of(), List.of());
    }
}
