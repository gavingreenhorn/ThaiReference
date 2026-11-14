package org.jonecorp.thaireference.auth;

import jakarta.persistence.*;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import lombok.Getter;
import lombok.NoArgsConstructor;

/**
 * Application account backing admin login. Lives outside {@code org.jonecorp.thaireference.entity} on
 * purpose: that package is SnapAdmin's {@code modelsPackage}, and credentials must never be
 * editable from the admin UI (privilege-escalation risk).
 */
@Entity
@Getter
@NoArgsConstructor
@Table(
        name = "user_accounts",
        schema = "account",
        uniqueConstraints = @UniqueConstraint(name = "uq_user_accounts_username", columnNames = "username"))
public class UserAccount {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @NotBlank
    @Column(name = "username", nullable = false, unique = true)
    private String username;

    @NotBlank
    @Column(name = "password", nullable = false)
    private String password;

    @NotNull
    @Enumerated(EnumType.STRING)
    @Column(name = "role", nullable = false)
    private Role role;

    public UserAccount(String username, String password, Role role) {
        this.username = username;
        this.password = password;
        this.role = role;
    }

    public String getAuthority() {
        return role.getAuthority();
    }

    public void setPassword(String password) {
        this.password = password;
    }
}
