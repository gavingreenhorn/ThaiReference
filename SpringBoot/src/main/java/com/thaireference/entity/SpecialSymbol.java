package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "SpecialSymbol", schema = "thai")
@Getter
@Setter
public class SpecialSymbol {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "grapheme", nullable = true)
    private String grapheme;

    @Column(name = "word_id", nullable = true)
    private Integer wordId;

}