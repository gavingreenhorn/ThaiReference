package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "GridPositionWord", schema = "thai")
@Getter
@Setter
public class GridPositionWord {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "grid_position_id", nullable = false)
    private Integer gridPositionId;

    @Column(name = "word_id", nullable = false)
    private Integer wordId;

    @Column(name = "word_order", nullable = false)
    private Integer wordOrder;
}
