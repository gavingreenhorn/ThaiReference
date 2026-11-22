CREATE TABLE thai."Season" (
	id integer NOT NULL,
	ordinal integer NOT NULL,
	name integer NOT NULL,
	english_label text NOT NULL,
	CONSTRAINT "Season_pkey" PRIMARY KEY (id),
	CONSTRAINT "Season_name_fkey" FOREIGN KEY (name) REFERENCES thai."CompositeName"(id)
);

CREATE UNIQUE INDEX uq_thai_season_ordinal ON thai."Season"(ordinal);

CREATE UNIQUE INDEX uq_thai_season_name ON thai."Season"(name);

ALTER TABLE thai."Month"
	ADD COLUMN season_id integer;

ALTER TABLE thai."Month"
	ALTER COLUMN season_id SET NOT NULL;

ALTER TABLE thai."Month"
	ADD CONSTRAINT "Month_season_id_fkey"
		FOREIGN KEY (season_id) REFERENCES thai."Season"(id);

CREATE INDEX idx_thai_month_season_id ON thai."Month"(season_id);
