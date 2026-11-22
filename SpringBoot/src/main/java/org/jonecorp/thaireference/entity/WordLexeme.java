package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;
import tech.ailef.snapadmin.external.annotations.Disable;

@Entity
@Table(name = "WordLexeme", schema = "thai")
@Getter
@Setter
@Disable
public class WordLexeme {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "word")
    private Word word;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "lexeme")
    private Lexeme lexeme;

}