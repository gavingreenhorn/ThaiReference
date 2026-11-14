package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "DayOfWeek", schema = "thai")
@Getter
@Setter
public class DayOfWeek {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "ordinal", nullable = false)
    private Integer ordinal;

    @Column(name = "word_id", nullable = false)
    private Integer wordId;

    @Column(name = "deity", nullable = true)
    private String deity;

    @Column(name = "image_url", nullable = true)
    private String imageUrl;

    @Column(name = "celestial_body_id", nullable = true)
    private Integer celestialBodyId;

}