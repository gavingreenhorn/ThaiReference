package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "ClusterWord", schema = "thai")
@Getter
@Setter
public class ClusterWord {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "cluster_id", nullable = false)
    private Integer clusterId;

    @Column(name = "word_id", nullable = false)
    private Integer wordId;

}