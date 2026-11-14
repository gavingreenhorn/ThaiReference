package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(
    name = "Lexeme",
    schema = "thai",
    uniqueConstraints = @UniqueConstraint(
        name = "uq_thai_lexeme_value_part_of_speech",
        columnNames = {"value", "part_of_speech"}
    )
)
@Getter
@Setter
public class Lexeme {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "value", nullable = true)
    private String value;

    @Column(name = "part_of_speech", nullable = true)
    private String partOfSpeech;

    @Column(name = "tags", nullable = true)
    private String tags;

}