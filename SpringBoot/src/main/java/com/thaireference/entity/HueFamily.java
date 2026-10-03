package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "HueFamily", schema = "thai")
@Getter
@Setter
public class HueFamily {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "thai_name", nullable = false, length = 50)
    private String thaiName;

    @Column(name = "english_name", nullable = false, length = 50)
    private String englishName;

    @Column(name = "hex_representative", nullable = false, length = 7)
    private String hexRepresentative;

    @Column(name = "display_order", nullable = false)
    private Short displayOrder;
}
