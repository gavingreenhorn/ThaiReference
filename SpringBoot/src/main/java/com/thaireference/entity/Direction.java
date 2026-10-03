package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Direction", schema = "thai")
@Getter
@Setter
public class Direction {

    @Id
    private Integer id;

    @Column(name = "label", nullable = false, length = 8)
    private String label;

    @Column(name = "lexeme", nullable = false, length = 32)
    private String lexeme;

    @Column(name = "composite_name_id", nullable = false)
    private Integer compositeNameId;
}
