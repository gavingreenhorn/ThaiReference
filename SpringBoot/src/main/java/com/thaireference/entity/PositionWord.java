package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "PositionWord", schema = "thai")
@Getter
@Setter
public class PositionWord {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "position_id", nullable = false)
    private Integer positionId;

    @Column(name = "word_id", nullable = false)
    private Integer wordId;

    @Column(name = "word_order", nullable = false)
    private Integer wordOrder;

}