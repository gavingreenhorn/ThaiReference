package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Zodiac", schema = "thai")
@Getter
@Setter
public class Zodiac {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "ordinal", nullable = false)
    private Integer ordinal;

    @Column(name = "english_name", nullable = false)
    private String englishName;

    @Column(name = "sanskrit_name", nullable = false)
    private String sanskritName;

    @Column(name = "sanskrit_lexeme", nullable = false)
    private String sanskritLexeme;

    @Column(name = "element", nullable = false)
    private String element;

    @Column(name = "image_url", nullable = true)
    private String imageUrl;

    @Column(name = "glyph", nullable = false)
    private String glyph;

}