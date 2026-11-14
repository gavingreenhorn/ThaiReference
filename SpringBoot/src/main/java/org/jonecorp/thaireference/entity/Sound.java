package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Sound", schema = "thai")
@Getter
@Setter
public class Sound {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "value", nullable = false)
    private String value;

    @Column(name = "mediapath", nullable = true)
    private String mediapath;

    @Column(name = "initial_only", nullable = false)
    private boolean initialOnly;

    @Column(name = "consonant_sound", nullable = true)
    private Integer consonantSound;

    @Column(name = "sound_type", nullable = true)
    private Integer soundTypeId;

}