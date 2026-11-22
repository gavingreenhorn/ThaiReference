package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Class", schema = "thai")
@Getter
@Setter
public class ThaiClassEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    /** Latin key: low / mid / high (analogous to ToneMark.mark). */
    @Column(name = "code", nullable = false, length = 15)
    private String code;

    @Column(name = "word_id", nullable = false)
    private Integer wordId;

    @Column(name = "mediapath", nullable = true)
    private String mediapath;

    @Column(name = "english_label", nullable = true)
    private String englishLabel;
}
