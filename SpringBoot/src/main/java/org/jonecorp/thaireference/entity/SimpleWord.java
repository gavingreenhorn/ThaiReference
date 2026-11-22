package org.jonecorp.thaireference.entity;

import jakarta.persistence.Column;
import jakarta.persistence.DiscriminatorValue;
import jakarta.persistence.Entity;
import jakarta.persistence.Table;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "SimpleWord", schema = "thai")
@DiscriminatorValue("simple")
@Getter
@Setter
public class SimpleWord extends Word {

    @Column(name = "spelling_id", nullable = false)
    private Integer spellingId;

    @Column(name = "transcription", nullable = false)
    private Integer transcriptionId;
}
