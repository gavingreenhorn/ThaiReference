package org.jonecorp.thaireference.entity;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "MinimalSetWord", schema = "thai")
@Getter
@Setter
public class MinimalSetWord {

    @Id
    private Integer id;

    @Column(name = "set_id", nullable = false)
    private Integer setId;

    @Column(name = "word_id", nullable = false)
    private Integer wordId;
}
