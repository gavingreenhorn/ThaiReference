package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Month", schema = "thai")
@Getter
@Setter
public class Month {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "ordinal", nullable = false)
    private Integer ordinal;

    @Column(name = "word_id", nullable = false)
    private Integer wordId;

    @Column(name = "short_name", nullable = false)
    private String shortName;

    @Column(name = "english_name", nullable = false)
    private String englishName;

    @Column(name = "zodiac_id", nullable = false)
    private Integer zodiacId;

    @Column(name = "season_id", nullable = false)
    private Integer seasonId;

}