package com.thaireference.account.entity;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.EnumType;
import jakarta.persistence.Enumerated;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import java.time.Instant;
import java.util.UUID;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;
import org.hibernate.annotations.JdbcTypeCode;
import org.hibernate.type.SqlTypes;

@Entity
@Getter
@Setter
@NoArgsConstructor
@Table(name = "account_token", schema = "account")
public class AccountToken {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(name = "token_hash", nullable = false, unique = true)
    private String tokenHash;

    @Enumerated(EnumType.STRING)
    @Column(name = "purpose", nullable = false)
    private AccountTokenPurpose purpose;

    @Column(name = "group_id", nullable = false)
    private Long groupId;

    @Column(name = "device_uuid")
    private UUID deviceUuid;

    @Column(name = "email")
    private String email;

    @Column(name = "expires_at", nullable = false)
    private long expiresAtEpochMs;

    @JdbcTypeCode(SqlTypes.TIMESTAMP_UTC)
    @Column(name = "used_at")
    private Instant usedAt;

    @JdbcTypeCode(SqlTypes.TIMESTAMP_UTC)
    @Column(name = "created_at", nullable = false)
    private Instant createdAt;
}
