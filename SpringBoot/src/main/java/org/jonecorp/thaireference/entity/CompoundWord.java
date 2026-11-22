package org.jonecorp.thaireference.entity;

import jakarta.persistence.DiscriminatorValue;
import jakarta.persistence.Entity;
import jakarta.persistence.OneToMany;
import jakarta.persistence.OrderBy;
import jakarta.persistence.Table;
import java.util.ArrayList;
import java.util.List;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "CompoundWord", schema = "thai")
@DiscriminatorValue("compound")
@Getter
@Setter
public class CompoundWord extends Word {

    @OneToMany(mappedBy = "compound")
    @OrderBy("position")
    private List<WordComponent> components = new ArrayList<>();
}
