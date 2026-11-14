package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "WordLexeme", schema = "thai")
@Getter
@Setter
public class WordLexeme {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "word", nullable = true)
    private Integer wordId;

    @Column(name = "lexeme", nullable = true)
    private Integer lexemeId;

}