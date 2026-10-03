package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Vowel", schema = "thai")
@Getter
@Setter
public class Vowel {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "is_diphtong", nullable = true)
    private Boolean isDiphtong;

    @Column(name = "mediapath", nullable = true)
    private String mediapath;

    @Column(name = "sound_id", nullable = true)
    private Integer soundId;

}