package org.jonecorp.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "MoodSentence", schema = "thai")
@IdClass(MoodSentenceId.class)
@Getter
@Setter
public class MoodSentence {

    @Id
    @Column(name = "mood_id", nullable = false)
    private Integer moodId;

    @Id
    @Column(name = "sentence_id", nullable = false)
    private Integer sentenceId;

    @Column(name = "position", nullable = false)
    private Integer position;

    @Column(name = "source_url")
    private String sourceUrl;

    @Column(name = "source_author")
    private String sourceAuthor;

    @Column(name = "source_license")
    private String sourceLicense;
}
