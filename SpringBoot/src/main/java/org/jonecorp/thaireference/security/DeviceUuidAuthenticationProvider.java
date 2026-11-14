package org.jonecorp.thaireference.security;

import org.jonecorp.thaireference.account.entity.Device;
import org.jonecorp.thaireference.account.repository.DeviceRepository;
import java.util.UUID;

import lombok.RequiredArgsConstructor;
import org.springframework.lang.NonNull;
import org.springframework.security.authentication.AuthenticationProvider;
import org.springframework.security.authentication.BadCredentialsException;
import org.springframework.security.authentication.DisabledException;
import org.springframework.security.authentication.LockedException;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.AuthenticationException;

@RequiredArgsConstructor
public class DeviceUuidAuthenticationProvider implements AuthenticationProvider {
    private final DeviceRepository deviceRepository;

    private static final String BAD = "Bad credentials";
    private static final String DISABLED = "Disabled";
    private static final String LOCKED = "Locked";

    @Override
    public Authentication authenticate(@NonNull Authentication authentication) throws AuthenticationException {
        Object credentials = authentication.getCredentials();
        UUID uuid;

        try {
            uuid = fromCredentials(credentials);
        }
        catch (IllegalArgumentException ex) {
            throw new BadCredentialsException(BAD);
        }

        Device device = deviceRepository
                .findByUuid(uuid)
                .orElseThrow(() -> new BadCredentialsException(BAD));

        if (device.isDisabled()) {
            throw new DisabledException(DISABLED);
        }

        if (device.isLocked()) {
            throw new LockedException(LOCKED);
        }

        return new DeviceUuidAuthenticationToken(
            new DevicePrincipal(
                device.getId(),
                device.getGroupId(),
                device.getUuid()
            ),
            null,
            true
        );
    }

    @Override
    public boolean supports(@NonNull Class<?> authentication) {
        return DeviceUuidAuthenticationToken.class.equals(authentication);
    }

    UUID fromCredentials(Object credentials) throws IllegalArgumentException {
        if (credentials == null || credentials.toString().isBlank()) {
            throw new IllegalArgumentException();
        }
        UUID uuid = UUID.fromString(credentials.toString());
        if (uuid.version() != 4) {
            throw new IllegalArgumentException();
        }
        return uuid;
    }
}
