package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Cluster", schema = "thai")
@Getter
@Setter
public class Cluster {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "type", nullable = false)
    private String clusterType;

    @Column(name = "subtype", nullable = true)
    private String subtype;

    @Column(name = "value", nullable = false)
    private String value;

}