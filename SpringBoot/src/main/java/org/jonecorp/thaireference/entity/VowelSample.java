package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "VowelSample", schema = "thai")
@Getter
@Setter
public class VowelSample {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "vowel", nullable = true)
    private Integer vowelId;

    @Column(name = "word", nullable = true)
    private Integer wordId;

}