package org.jonecorp.thaireference.config.properties;

import org.springframework.boot.context.properties.ConfigurationProperties;

@ConfigurationProperties(prefix = "app.mail")
public record AccountMailProperties(String from, String resendApiKey) {}
