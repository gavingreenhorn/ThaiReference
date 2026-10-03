package com.thaireference.security;

import java.util.List;
import org.springframework.security.authentication.AbstractAuthenticationToken;


class DeviceUuidAuthenticationToken extends AbstractAuthenticationToken {
    private final Object principal;
    private final Object credentials;

    DeviceUuidAuthenticationToken(Object principal, Object credentials, boolean authenticated) {
        super(List.of());
        this.principal = principal;
        this.credentials = credentials;
        setAuthenticated(authenticated);
    }

    static DeviceUuidAuthenticationToken getUnauthenticatedToken(Object credentials) {
        return new DeviceUuidAuthenticationToken(null, credentials, false);
    }

    @Override
    public Object getCredentials() {
        return credentials;
    }

    @Override
    public Object getPrincipal() {
        return principal;
    }
}
