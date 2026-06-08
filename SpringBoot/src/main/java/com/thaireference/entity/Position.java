package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Position", schema = "thai")
@Getter
@Setter
public class Position {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "row_index", nullable = false)
    private Integer rowIndex;

    @Column(name = "col_index", nullable = false)
    private Integer colIndex;

    @Column(name = "name", nullable = true)
    private String name;

    @Column(name = "tile_style", nullable = true)
    private String tileStyle;

}