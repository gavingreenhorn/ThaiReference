package com.thaireference.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Table(name = "Season", schema = "thai")
@Getter
@Setter
public class Season {

	@Id
	private Integer id;

	@Column(name = "ordinal", nullable = false)
	private Integer ordinal;

	@Column(name = "name", nullable = false)
	private Integer compositeNameId;

	@Column(name = "english_label", nullable = false)
	private String englishLabel;
}
