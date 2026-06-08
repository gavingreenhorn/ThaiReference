package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Consonant", schema = "thai")
@Getter
@Setter
public class Consonant {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "grapheme", nullable = false)
    private String grapheme;

    @Column(name = "phoneme", nullable = false)
    private String phoneme;

    @Column(name = "soundtype", nullable = false)
    private String soundtype;

    @Column(name = "class", nullable = false)
    private String classRaw;

    @Column(name = "ending", nullable = false)
    private String ending;

    @Column(name = "transcription", nullable = true)
    private Integer transcriptionId;

    @Column(name = "acrophonic", nullable = true)
    private Integer acrophonicId;

    @Column(name = "mediapath", nullable = true)
    private String mediapath;

    @Column(name = "initial", nullable = true)
    private Integer initialId;

    @Column(name = "final", nullable = true)
    private Integer finalSoundId;

}