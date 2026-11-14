package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Color", schema = "thai")
@Getter
@Setter
public class Color {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "hue_family_id", nullable = false)
    private Integer hueFamilyId;

    @Column(name = "composite_name_id")
    private Integer compositeNameId;

    @Column(name = "english_label", nullable = false, length = 100)
    private String englishLabel;

    @Column(name = "hex", nullable = false, length = 7)
    private String hex;

    @Column(name = "luminance_order", nullable = false)
    private Short luminanceOrder;
}
