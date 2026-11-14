package org.jonecorp.thaireference.entity;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "MinimalSet", schema = "thai")
@Getter
@Setter
public class MinimalSet {

    @Id
    private Integer id;

    @Column(name = "label", nullable = true)
    private String label;
}
