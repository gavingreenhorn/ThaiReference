package org.jonecorp.thaireference.security;

import java.util.UUID;

public record DevicePrincipal(Long deviceId, Long groupId, UUID uuid) {}
