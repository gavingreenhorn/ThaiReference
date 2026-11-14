package org.jonecorp.thaireference.auth;

/** Account roles. The granted authority is the enum name (e.g. "ADMIN"), matching SecurityConfig. */
public enum Role {
    ADMIN,
    USER;

    public String getAuthority() {
        return name();
    }
}
