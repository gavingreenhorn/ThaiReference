package com.thaireference.account.entity;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
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
@Table(name = "device", schema = "thai")
public class Device {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(name = "uuid", nullable = false, unique = true)
    private UUID uuid;

    @Column(name = "device_group_id", nullable = false)
    private Long groupId;

    @Column(name = "platform")
    private String platform;

    @Column(name = "model", length = 128)
    private String model;

    @Column(name = "build")
    private String build;

    @Column(name = "last_ip", length = 45)
    private String lastIp;

    @JdbcTypeCode(SqlTypes.TIMESTAMP_UTC)
    @Column(name = "linked_at")
    private Instant linkedAt;

    @JdbcTypeCode(SqlTypes.TIMESTAMP_UTC)
    @Column(name = "created_at", nullable = false)
    private Instant createdAt;

    @JdbcTypeCode(SqlTypes.TIMESTAMP_UTC)
    @Column(name = "last_seen")
    private Instant lastSeen;

    @Column(name = "disabled", nullable = false)
    private boolean disabled;

    @Column(name = "locked", nullable = false)
    private boolean locked;
}
