package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Transcription", schema = "thai")
@Getter
@Setter
public class Transcription {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "paiboon", nullable = true)
    private String paiboon;

    @Column(name = "IPA", nullable = true)
    private String ipa;

}
