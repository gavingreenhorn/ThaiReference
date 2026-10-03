package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Numeral", schema = "thai")
@Getter
@Setter
public class Numeral {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "word", nullable = true)
    private Integer wordId;

    @Column(name = "grapheme", nullable = true)
    private String grapheme;

}