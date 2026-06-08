package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Pronoun", schema = "thai")
@Getter
@Setter
public class Pronoun {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "word", nullable = true)
    private Integer wordId;

    @Column(name = "gender", nullable = true)
    private Integer genderId;

    @Column(name = "formality", nullable = true)
    private Integer formalityId;

    @Column(name = "plurality", nullable = true)
    private Integer pluralityId;

    @Column(name = "grammatical_person", nullable = true)
    private Integer grammaticalPerson;

}