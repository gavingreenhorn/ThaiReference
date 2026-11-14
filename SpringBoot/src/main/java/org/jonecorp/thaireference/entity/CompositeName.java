package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "CompositeName", schema = "thai")
@Getter
@Setter
public class CompositeName {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "word1", nullable = true)
    private Integer word1Id;

    @Column(name = "word2", nullable = true)
    private Integer word2Id;

    @Column(name = "word3", nullable = true)
    private Integer word3Id;

    @Column(name = "word4", nullable = true)
    private Integer word4Id;

    @Column(name = "word5", nullable = true)
    private Integer word5Id;

}