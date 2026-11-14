package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "CelestialBody", schema = "thai")
@Getter
@Setter
public class CelestialBody {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "ordinal", nullable = false)
    private Integer ordinal;

    @Column(name = "glyph", nullable = false)
    private String glyph;

    @Column(name = "color_hex", nullable = false)
    private String colorHex;

    @Column(name = "word_id", nullable = false)
    private Integer wordId;

}