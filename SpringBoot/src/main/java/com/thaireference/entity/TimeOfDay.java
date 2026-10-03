package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "TimeOfDay", schema = "thai")
@Getter
@Setter
public class TimeOfDay {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "ordinal", nullable = false)
    private Integer ordinal;

    @Column(name = "word_id", nullable = false)
    private Integer wordId;

}