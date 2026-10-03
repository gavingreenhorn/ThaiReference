package com.thaireference.config.properties;

import org.springframework.boot.context.properties.ConfigurationProperties;


@ConfigurationProperties(prefix = "app")
public record AppProperties(String corsOrigins, Admin admin) {

    public record Admin(String username, String password) {}

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
