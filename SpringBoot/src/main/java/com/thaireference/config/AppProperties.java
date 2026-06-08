package com.thaireference.config;

import org.springframework.boot.context.properties.ConfigurationProperties;

@ConfigurationProperties(prefix = "app")
public record AppProperties(String corsOrigins) {

    public String[] corsOriginsArray() {
        if (corsOrigins == null || corsOrigins.isBlank()) {
            return new String[0];
        }
        return java.util.Arrays.stream(corsOrigins.split(","))
                .map(String::trim)
                .filter(s -> !s.isEmpty())
                .toArray(String[]::new);
    }
}
