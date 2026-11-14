package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "ConsonantSample", schema = "thai")
@Getter
@Setter
public class ConsonantSample {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "consonant", nullable = true)
    private Integer consonantId;

    @Column(name = "word", nullable = true)
    private Integer wordId;

}