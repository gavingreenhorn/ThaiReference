package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "PronounTypeBreakout", schema = "thai")
@Getter
@Setter
public class PronounTypeBreakout {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "pronoun_id", nullable = true)
    private Integer pronounId;

    @Column(name = "type_id", nullable = true)
    private Integer typeId;

}