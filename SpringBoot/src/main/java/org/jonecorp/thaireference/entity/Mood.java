package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Mood", schema = "thai")
@Getter
@Setter
public class Mood {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "word_id", nullable = false)
    private Integer wordId;
}
