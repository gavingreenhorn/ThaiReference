package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;
import java.util.ArrayList;
import java.util.List;

@Entity
@Table(name = "Word", schema = "thai")
@Inheritance(strategy = InheritanceType.JOINED)
@DiscriminatorColumn(name = "kind", discriminatorType = DiscriminatorType.STRING)
@Getter
@Setter
public abstract class Word {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "is_primary", nullable = false)
    private boolean isPrimary;

    @OneToMany(mappedBy = "word")
    private List<WordLexeme> lexemes = new ArrayList<>();
}