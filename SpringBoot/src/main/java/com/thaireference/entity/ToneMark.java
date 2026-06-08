package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "ToneMark", schema = "thai")
@Getter
@Setter
public class ToneMark {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "mark", nullable = false)
    private String mark;

    @Column(name = "name", nullable = true)
    private Integer nameId;

    @Column(name = "mediapath", nullable = true)
    private String mediapath;

    @Column(name = "english_label", nullable = true)
    private String englishLabel;

}