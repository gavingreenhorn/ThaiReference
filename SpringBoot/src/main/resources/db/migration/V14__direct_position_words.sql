ALTER TABLE thai."Position"
    ADD COLUMN word_id integer REFERENCES thai."Word"(id),
    ALTER COLUMN composite_name_id DROP NOT NULL,
    ADD CONSTRAINT "Position_label_reference_check"
        CHECK ((word_id IS NOT NULL) <> (composite_name_id IS NOT NULL));
