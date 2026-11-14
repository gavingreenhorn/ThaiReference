package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Position", schema = "thai")
@Getter
@Setter
public class Position {

    @Id
    private Integer id;

    @Column(name = "composite_name_id", nullable = false)
    private Integer compositeNameId;

    @Column(name = "sentence_id", nullable = false)
    private Integer sentenceId;

    @Column(name = "lexeme", nullable = false, length = 32)
    private String lexeme;
}
