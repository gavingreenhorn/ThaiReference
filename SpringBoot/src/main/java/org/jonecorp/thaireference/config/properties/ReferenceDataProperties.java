package org.jonecorp.thaireference.config.properties;

import java.time.Duration;
import org.springframework.boot.context.properties.ConfigurationProperties;


@ConfigurationProperties(prefix = "app.reference-data")
public record ReferenceDataProperties(Duration refreshInterval, Duration adminDebounce, Duration httpCacheMaxAge) {

    public ReferenceDataProperties {
        if (refreshInterval == null) {
            refreshInterval = Duration.ofMinutes(10);
        }
        if (adminDebounce == null) {
            adminDebounce = Duration.ofSeconds(3);
        }
        if (httpCacheMaxAge == null) {
            httpCacheMaxAge = refreshInterval;
        }
    }
}
