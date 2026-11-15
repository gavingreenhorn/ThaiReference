package org.jonecorp.thaireference.entity;

import java.io.Serializable;
import lombok.EqualsAndHashCode;
import lombok.NoArgsConstructor;

@NoArgsConstructor
@EqualsAndHashCode
public class MoodSentenceId implements Serializable {
    private Integer moodId;
    private Integer sentenceId;
}
