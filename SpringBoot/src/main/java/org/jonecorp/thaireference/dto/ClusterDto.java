package org.jonecorp.thaireference.dto;

import com.fasterxml.jackson.annotation.JsonProperty;
import java.util.List;

public record ClusterDto(
        Integer id,
        @JsonProperty("type") String type,
        String subtype,
        String value,
        List<WordDto> sampleWords) {

    public ClusterDto(Integer id, String type, String subtype, String value) {
        this(id, type, subtype, value, List.of());
    }
}
