package org.jonecorp.thaireference.view;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;
import org.hibernate.annotations.Immutable;
import org.hibernate.annotations.JdbcTypeCode;
import org.hibernate.type.SqlTypes;


// Read-only mapping of materialized view, one flat row per Word and all of its relations
@Entity
@Immutable
@Table(name = "WordView", schema = "thai")
@Getter
@Setter
public class WordView {

    @Id
    @Column(name = "id")
    private Integer id;

    @Column(name = "spelling_id")
    private Integer spellingId;

    @Column(name = "transcription_id")
    private Integer transcriptionId;

    @Column(name = "spelling")
    private String spelling;

    @Column(name = "paiboon")
    private String paiboon;

    @Column(name = "ipa")
    private String ipa;

    @JdbcTypeCode(SqlTypes.ARRAY)
    @Column(name = "lexemes")
    private String[] lexemes;
}
