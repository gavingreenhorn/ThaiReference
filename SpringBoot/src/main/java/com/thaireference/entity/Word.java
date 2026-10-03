package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Word", schema = "thai")
@Getter
@Setter
public class Word {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "spelling_id", nullable = false)
    private Integer spellingId;

    @Column(name = "transcription", nullable = false)
    private Integer transcriptionId;

    @Column(name = "is_primary", nullable = false)
    private boolean isPrimary;

}