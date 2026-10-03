package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "SentenceWord", schema = "thai")
@Getter
@Setter
public class SentenceWord {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "sentence_id", nullable = false)
    private Integer sentenceId;

    @Column(name = "word_id", nullable = false)
    private Integer wordId;

    @Column(name = "word_order", nullable = false)
    private Integer wordOrder;
}
