package org.jonecorp.thaireference.config.properties;

import java.time.Duration;
import org.springframework.boot.context.properties.ConfigurationProperties;


@ConfigurationProperties(prefix = "app.account")
public record AccountProperties(
    Duration pairTtl,
    Duration emailTtl,
    Integer rateLimit,
    Duration rateLimitWindow,
    Integer createQuota,
    Duration createQuotaWindow,
    Integer maxDevices
) {}
