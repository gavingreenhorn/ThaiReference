package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "VowelGrapheme", schema = "thai")
@Getter
@Setter
public class VowelGrapheme {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "vowel_id", nullable = true)
    private Integer vowelId;

    @Column(name = "value", nullable = true)
    private String value;

    @Column(name = "is_closed", nullable = true)
    private Boolean closed;

}