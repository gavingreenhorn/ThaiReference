package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Sentence", schema = "thai")
@Getter
@Setter
public class Sentence {

    @Id
    private Integer id;

    @Column(name = "translation", nullable = false, length = 128)
    private String translation;
}
